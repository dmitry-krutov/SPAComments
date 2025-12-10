import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import PageLayout from '../components/PageLayout'
import { searchComments } from '../features/comments/api'
import type { CommentSearchItemDto } from '../features/comments/types'
import { formatUnknownError } from '../lib/apiClient'
import { formatDate } from '../lib/formatDate'

interface SearchState {
  items: CommentSearchItemDto[]
  page: number
  pageSize: number
  totalCount: number
  status: 'idle' | 'loading' | 'succeeded' | 'failed'
  error?: string
}

const initialState: SearchState = {
  items: [],
  page: 1,
  pageSize: 10,
  totalCount: 0,
  status: 'idle',
}

function SearchResultsPage() {
  const { text = '' } = useParams<{ text: string }>()
  const decodedText = decodeURIComponent(text)
  const navigate = useNavigate()
  const [searchInput, setSearchInput] = useState(decodedText)
  const [state, setState] = useState<SearchState>(initialState)

  const totalPages = useMemo(() => Math.max(1, Math.ceil(state.totalCount / state.pageSize)), [state.totalCount, state.pageSize])

  const runSearch = useCallback(
    async (queryText: string, page = 1) => {
      if (!queryText.trim()) {
        setState((prev) => ({ ...prev, items: [], totalCount: 0, status: 'idle' }))
        return
      }

      setState((prev) => ({ ...prev, status: 'loading', error: undefined }))
      try {
        const response = await searchComments({ text: queryText, page, pageSize: state.pageSize })
        setState({
          items: response.items,
          page: response.page,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          status: 'succeeded',
        })
      } catch (error) {
        setState((prev) => ({
          ...prev,
          status: 'failed',
          error: formatUnknownError(error, 'Не удалось выполнить поиск'),
        }))
      }
    },
    [state.pageSize]
  )

  useEffect(() => {
    setSearchInput(decodedText)
    runSearch(decodedText, 1)
  }, [decodedText, runSearch])

  const handleSearchSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const query = searchInput.trim()
    if (!query) return
    navigate(`/comments/search/${encodeURIComponent(query)}`)
  }

  const goToPage = (page: number) => {
    if (page < 1 || page > totalPages || state.status === 'loading') return
    runSearch(decodedText, page)
  }

  return (
    <PageLayout title="Результаты поиска" subtitle={`Запрос: “${decodedText}”`}>
      <section className="flex flex-col gap-4 rounded-2xl border border-white/10 bg-white/5 p-6 shadow-soft backdrop-blur">
        <form onSubmit={handleSearchSubmit} className="flex flex-col gap-3 md:flex-row md:items-center md:gap-4">
          <div className="flex-1 space-y-1">
            <label htmlFor="search-input" className="text-sm text-slate-200">
              Изменить запрос
            </label>
            <input
              id="search-input"
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
              placeholder="Введите текст для поиска"
            />
          </div>
          <button
            type="submit"
            className="inline-flex items-center justify-center gap-2 rounded-full bg-brand-500 px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-brand-500/30 transition hover:bg-brand-600"
          >
            Найти
          </button>
        </form>

        <div className="rounded-2xl border border-white/10 bg-slate-950/40 p-4">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h2 className="text-xl font-semibold text-white">Найденные комментарии</h2>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => goToPage(state.page - 1)}
                disabled={state.page <= 1 || state.status === 'loading'}
                className="rounded-full border border-white/10 bg-white/10 px-3 py-2 text-sm text-white transition hover:bg-white/20 disabled:cursor-not-allowed disabled:opacity-60"
              >
                Назад
              </button>
              <div className="rounded-full bg-white/10 px-4 py-2 text-sm text-slate-200">
                Стр. {state.page} / {totalPages}
              </div>
              <button
                type="button"
                onClick={() => goToPage(state.page + 1)}
                disabled={state.page >= totalPages || state.status === 'loading'}
                className="rounded-full border border-white/10 bg-white/10 px-3 py-2 text-sm text-white transition hover:bg-white/20 disabled:cursor-not-allowed disabled:opacity-60"
              >
                Вперёд
              </button>
            </div>
          </div>

          <div className="mt-6 space-y-3">
            {state.status === 'loading' && (
              <div className="rounded-xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-slate-300">Ищем комментарии…</div>
            )}

            {state.status === 'failed' && state.error && (
              <div className="rounded-xl border border-rose-400/30 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">{state.error}</div>
            )}

            {state.status === 'succeeded' && state.items.length === 0 && (
              <div className="rounded-xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-slate-300">Ничего не найдено.</div>
            )}

            {state.items.map((item) => (
              <Link
                key={item.id}
                to={`/comments/${item.id}`}
                className="block rounded-2xl border border-white/10 bg-white/5 p-4 transition hover:border-brand-400/50 hover:bg-white/10"
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="text-base font-semibold text-white">{item.userName}</p>
                  <p className="text-xs text-slate-400">{formatDate(item.createdAt)}</p>
                </div>
                <p className="mt-2 line-clamp-3 text-sm leading-relaxed text-slate-100">{item.text}</p>
              </Link>
            ))}
          </div>
        </div>
      </section>
    </PageLayout>
  )
}

export default SearchResultsPage

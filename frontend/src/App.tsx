import { type JSX, useEffect, useMemo, useState } from 'react'
import { useAppDispatch, useAppSelector } from './app/hooks'
import { prependComment, fetchLatestComments } from './features/comments/commentFeedSlice'
import type { CommentDto } from './features/comments/types'
import CommentForm from './components/CommentForm'

const formatDate = (value: string) =>
  new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value))

const getAttachmentLabel = (contentType?: string) => {
  if (!contentType) return 'Файл'
  if (contentType.startsWith('image/')) return 'Изображение'
  if (contentType.startsWith('text/')) return 'Текстовый файл'
  return 'Файл'
}

type CommentNode = CommentDto & { children: CommentNode[] }

const buildCommentTree = (items: CommentDto[]): CommentNode[] => {
  const nodes = new Map<string, CommentNode>()
  const roots: CommentNode[] = []

  items.forEach((item) => {
    nodes.set(item.id, { ...item, children: [] })
  })

  items.forEach((item) => {
    const node = nodes.get(item.id)
    if (!node) return

    if (item.parentId && nodes.has(item.parentId)) {
      nodes.get(item.parentId)?.children.push(node)
    } else {
      roots.push(node)
    }
  })

  return roots
}

function App() {
  const dispatch = useAppDispatch()
  const commentFeed = useAppSelector((state) => state.commentFeed)
  const [activeReplyFor, setActiveReplyFor] = useState<string | null>(null)

  useEffect(() => {
    dispatch(fetchLatestComments())
  }, [dispatch])

  const commentTree = useMemo(() => buildCommentTree(commentFeed.items), [commentFeed.items])
  const totalPages = Math.max(1, Math.ceil(commentFeed.totalCount / commentFeed.pageSize))

  const goToPage = (page: number) => {
    if (page < 1 || page > totalPages) return
    dispatch(fetchLatestComments({ page }))
    setActiveReplyFor(null)
  }

  const handleCommentAdded = (comment: CommentDto) => {
    dispatch(prependComment(comment))
  }

  const renderComment = (node: CommentNode, depth = 0): JSX.Element => (
    <div className="space-y-3" style={{ marginLeft: depth * 18 }}>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white/10 text-sm font-semibold text-white">
            {node.userName.charAt(0).toUpperCase()}
          </div>
          <div className="space-y-1">
            <div className="flex flex-wrap items-center gap-2">
              <p className="text-base font-semibold text-white">{node.userName}</p>
              {node.email && <p className="text-xs text-slate-400">{node.email}</p>}
              {node.homePage && (
                <a
                  href={node.homePage}
                  target="_blank"
                  rel="noreferrer"
                  className="text-xs text-brand-200 underline decoration-dashed underline-offset-4"
                >
                  {node.homePage}
                </a>
              )}
            </div>
            <p className="text-xs text-slate-400">{formatDate(node.createdAt)}</p>
          </div>
        </div>

        {node.attachments.length > 0 && (
          <span className="rounded-full bg-brand-500/10 px-3 py-1 text-xs text-brand-100">
            Вложений: {node.attachments.length}
          </span>
        )}
      </div>

      <p className="whitespace-pre-line text-sm leading-relaxed text-slate-100">{node.text}</p>

      {node.attachments.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {node.attachments.map((attachment) => (
            <a
              key={attachment.fileId}
              href={attachment.url}
              target="_blank"
              rel="noreferrer"
              className="group flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-3 py-2 text-xs text-slate-100 transition hover:border-brand-400/40 hover:text-white"
            >
              <span className="h-2 w-2 rounded-full bg-brand-400" />
              <span>{getAttachmentLabel(attachment.contentType)}</span>
            </a>
          ))}
        </div>
      )}

      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={() => setActiveReplyFor((prev) => (prev === node.id ? null : node.id))}
          className="rounded-full border border-white/10 bg-white/10 px-3 py-2 text-xs text-white transition hover:bg-white/20"
        >
          Ответить
        </button>
      </div>

      {activeReplyFor === node.id && (
        <CommentForm
          parentId={node.id}
          onSubmitted={handleCommentAdded}
          onCancel={() => setActiveReplyFor(null)}
          compact
        />
      )}

      {node.children.length > 0 && (
        <div className="space-y-3 border-l border-white/10 pl-4">
          {node.children.map((child) => renderComment(child, depth + 1))}
        </div>
      )}
    </div>
  )

  return (
    <div className="min-h-screen bg-slate-900/90 text-slate-50">
      <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-12">
        <header className="flex flex-col gap-2 rounded-2xl bg-white/5 p-6 shadow-soft backdrop-blur">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-sm uppercase tracking-[0.28em] text-slate-300">SPA Comments</p>
              <h1 className="text-3xl font-semibold text-white">Страница комментариев</h1>
            </div>
          </div>
        </header>

        <section className="flex flex-col gap-4 rounded-2xl border border-white/10 bg-white/5 p-6 shadow-soft backdrop-blur">
          <CommentForm heading="Новый комментарий" onSubmitted={handleCommentAdded} />

          <div className="rounded-2xl border border-white/10 bg-slate-950/40 p-4">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <h2 className="text-2xl font-semibold text-white">Лента комментариев</h2>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => goToPage(commentFeed.page - 1)}
                  disabled={commentFeed.page <= 1 || commentFeed.status === 'loading'}
                  className="rounded-full border border-white/10 bg-white/10 px-3 py-2 text-sm text-white transition hover:bg-white/20 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  Назад
                </button>
                <div className="rounded-full bg-white/10 px-4 py-2 text-sm text-slate-200">
                  Стр. {commentFeed.page} / {totalPages}
                </div>
                <button
                  type="button"
                  onClick={() => goToPage(commentFeed.page + 1)}
                  disabled={commentFeed.page >= totalPages || commentFeed.status === 'loading'}
                  className="rounded-full border border-white/10 bg-white/10 px-3 py-2 text-sm text-white transition hover:bg-white/20 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  Вперёд
                </button>
              </div>
            </div>

            <div className="mt-6 space-y-4">
              {commentFeed.status === 'loading' && (
                <div className="rounded-xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-slate-300">
                  Обновляем ленту…
                </div>
              )}

              {commentFeed.status === 'failed' && commentFeed.error && (
                <div className="rounded-xl border border-rose-400/30 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">
                  {commentFeed.error}
                </div>
              )}

              {commentFeed.status === 'succeeded' && commentFeed.items.length === 0 && (
                <div className="rounded-xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-slate-300">
                  Пока нет комментариев.
                </div>
              )}

              {commentTree.map((node) => (
                <div key={node.id} className="rounded-2xl border border-white/10 bg-white/5 p-4 shadow-sm">
                  {renderComment(node)}
                </div>
              ))}
            </div>
          </div>
        </section>
      </div>
    </div>
  )
}

export default App

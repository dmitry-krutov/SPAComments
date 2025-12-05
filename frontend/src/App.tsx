import { ChangeEvent, FormEvent, useEffect, useMemo, useState } from 'react'
import { useAppDispatch, useAppSelector } from './app/hooks'
import { config } from './config'
import { fetchCaptcha, submitComment, uploadAttachment } from './features/comments/commentFormSlice'

const formatSize = (size: number) => {
  if (size < 1024) return `${size} Б`
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} КБ`
  return `${(size / (1024 * 1024)).toFixed(1)} МБ`
}

const initialFormState = {
  userName: '',
  email: '',
  homePage: '',
  text: '',
  captchaAnswer: '',
}

function App() {
  const dispatch = useAppDispatch()
  const { captcha, attachments, submit } = useAppSelector((state) => state.commentForm)
  const [form, setForm] = useState(initialFormState)

  useEffect(() => {
    dispatch(fetchCaptcha())
  }, [dispatch])

  useEffect(() => {
    if (submit.status === 'succeeded') {
      setForm(initialFormState)
      dispatch(fetchCaptcha())
    }
  }, [submit.status, dispatch])

  const captchaImageSrc = useMemo(() => {
    if (!captcha.data) return ''
    return `data:${captcha.data.contentType};base64,${captcha.data.imageBase64}`
  }, [captcha.data])

  const handleInput = (field: keyof typeof initialFormState) => (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setForm((prev) => ({ ...prev, [field]: event.target.value }))
  }

  const handleFiles = (event: ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files
    if (!files?.length) return

    Array.from(files).forEach((file) => {
      const localId = crypto.randomUUID()
      dispatch(uploadAttachment({ file, localId }))
    })

    event.target.value = ''
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    dispatch(submitComment(form))
  }

  const isSubmitting = submit.status === 'submitting'
  const hasUploading = attachments.some((a) => a.status === 'uploading')

  return (
    <div className="min-h-screen bg-slate-900/90 text-slate-50">
      <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-12">
        <header className="flex flex-col gap-2 rounded-2xl bg-white/5 p-6 shadow-soft backdrop-blur">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-sm uppercase tracking-[0.28em] text-slate-300">SPA Comments</p>
              <h1 className="text-3xl font-semibold text-white">Форма добавления комментария</h1>
            </div>
            <div className="rounded-full bg-brand-500/10 px-4 py-2 text-sm text-brand-100">
              API: <span className="font-semibold text-brand-200">{config.apiBaseUrl}</span>
            </div>
          </div>
          <p className="max-w-3xl text-slate-300">
            Минималистичная форма, которая демонстрирует работу эндпоинтов <code className="text-brand-200">/api/captcha</code>,
            <code className="text-brand-200"> /api/Comments/attachments</code> и
            <code className="text-brand-200"> /api/Comments</code>. Загрузите вложения, введите капчу и отправьте сообщение.
          </p>
        </header>

        <main className="grid gap-6 lg:grid-cols-3">
          <section className="lg:col-span-2">
            <form
              className="rounded-2xl border border-white/10 bg-white/5 p-8 shadow-soft backdrop-blur"
              onSubmit={handleSubmit}
            >
              <div className="flex items-start justify-between gap-4">
                <div>
                  <h2 className="text-2xl font-semibold text-white">Новый комментарий</h2>
                  <p className="text-sm text-slate-300">Поля, отмеченные звёздочкой, обязательны для заполнения.</p>
                </div>
                <button
                  type="button"
                  onClick={() => dispatch(fetchCaptcha())}
                  className="group inline-flex items-center gap-2 rounded-full bg-white/10 px-4 py-2 text-sm font-medium text-white transition hover:bg-white/20"
                >
                  <span className="h-2 w-2 rounded-full bg-emerald-400 group-hover:scale-110" />
                  Обновить капчу
                </button>
              </div>

              <div className="mt-8 grid gap-6 md:grid-cols-2">
                <div className="space-y-2">
                  <label className="text-sm text-slate-200" htmlFor="userName">
                    Имя*
                  </label>
                  <input
                    id="userName"
                    required
                    value={form.userName}
                    onChange={handleInput('userName')}
                    className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
                    placeholder="Как вас зовут?"
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm text-slate-200" htmlFor="email">
                    Email*
                  </label>
                  <input
                    id="email"
                    type="email"
                    required
                    value={form.email}
                    onChange={handleInput('email')}
                    className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
                    placeholder="name@example.com"
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm text-slate-200" htmlFor="homePage">
                    Домашняя страница
                  </label>
                  <input
                    id="homePage"
                    type="url"
                    value={form.homePage}
                    onChange={handleInput('homePage')}
                    className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
                    placeholder="https://example.com"
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm text-slate-200" htmlFor="attachments">
                    Вложения
                  </label>
                  <label className="flex h-[52px] cursor-pointer items-center justify-between rounded-xl border border-dashed border-brand-400/60 bg-brand-500/10 px-4 text-sm text-brand-100 transition hover:bg-brand-500/20">
                    <input id="attachments" type="file" className="hidden" multiple onChange={handleFiles} />
                    <span>Перетащите или выберите файлы</span>
                    <span className="rounded-full bg-white/20 px-3 py-1 text-xs text-white">Загрузить</span>
                  </label>
                </div>
              </div>

              <div className="mt-6 space-y-2">
                <label className="text-sm text-slate-200" htmlFor="text">
                  Текст комментария*
                </label>
                <textarea
                  id="text"
                  required
                  value={form.text}
                  onChange={handleInput('text')}
                  rows={5}
                  className="w-full rounded-2xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
                  placeholder="Поделитесь своими мыслями"
                />
              </div>

              <div className="mt-8 grid gap-4 md:grid-cols-[auto,1fr] md:items-center">
                <div className="flex items-center gap-3">
                  <div className="flex h-24 w-32 items-center justify-center rounded-xl border border-white/10 bg-slate-950/40">
                    {captcha.status === 'loading' && <div className="text-sm text-slate-300">Загрузка...</div>}
                    {captcha.status === 'failed' && <div className="text-sm text-rose-300">Не удалось загрузить капчу</div>}
                    {captcha.status === 'succeeded' && (
                      <img
                        src={captchaImageSrc}
                        alt="Капча"
                        className="h-full w-full rounded-md object-contain"
                      />
                    )}
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm text-slate-200" htmlFor="captchaAnswer">
                      Ответ на капчу*
                    </label>
                    <input
                      id="captchaAnswer"
                      required
                      value={form.captchaAnswer}
                      onChange={handleInput('captchaAnswer')}
                      className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
                      placeholder="Введите буквы с картинки"
                    />
                    {captcha.error && <p className="text-xs text-rose-300">{captcha.error}</p>}
                  </div>
                </div>

                <div className="flex flex-col gap-3 md:items-end md:justify-between">
                  {submit.error && (
                    <div className="w-full rounded-xl border border-rose-400/30 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">
                      {submit.error}
                    </div>
                  )}
                  {submit.successMessage && (
                    <div className="w-full rounded-xl border border-emerald-400/30 bg-emerald-500/10 px-4 py-3 text-sm text-emerald-100">
                      {submit.successMessage}
                    </div>
                  )}
                  <button
                    type="submit"
                    disabled={isSubmitting || hasUploading || captcha.status !== 'succeeded'}
                    className="inline-flex items-center justify-center gap-2 rounded-full bg-brand-500 px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-brand-500/30 transition hover:bg-brand-600 disabled:cursor-not-allowed disabled:bg-slate-500"
                  >
                    {isSubmitting ? 'Отправляем…' : 'Отправить комментарий'}
                  </button>
                  {hasUploading && <p className="text-xs text-slate-300">Подождите, пока завершится загрузка файлов.</p>}
                </div>
              </div>
            </form>
          </section>

          <aside className="space-y-4">
            <div className="rounded-2xl border border-white/10 bg-slate-950/40 p-6 shadow-soft">
              <h3 className="text-lg font-semibold text-white">Загруженные вложения</h3>
              <p className="text-sm text-slate-400">Файлы отправляются через /api/Comments/attachments</p>
              <div className="mt-4 space-y-3">
                {attachments.length === 0 && <p className="text-sm text-slate-400">Пока ничего не выбрано.</p>}
                {attachments.map((file) => (
                  <div
                    key={file.localId}
                    className="flex items-start justify-between gap-3 rounded-xl border border-white/10 bg-white/5 px-3 py-3"
                  >
                    <div className="space-y-1">
                      <p className="text-sm font-medium text-white">{file.fileName}</p>
                      <p className="text-xs text-slate-400">{formatSize(file.size)}</p>
                      {file.meta && (
                        <p className="text-xs text-slate-400">
                          {file.meta.contentType}
                          {file.meta.width && file.meta.height ? ` · ${file.meta.width}×${file.meta.height}` : ''}
                        </p>
                      )}
                      {file.error && <p className="text-xs text-rose-200">{file.error}</p>}
                    </div>
                    <span
                      className={`mt-1 inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${
                        file.status === 'uploading'
                          ? 'bg-amber-500/20 text-amber-100'
                          : file.status === 'succeeded'
                            ? 'bg-emerald-500/20 text-emerald-100'
                            : 'bg-rose-500/20 text-rose-100'
                      }`}
                    >
                      {file.status === 'uploading' && 'Загрузка…'}
                      {file.status === 'succeeded' && 'Готово'}
                      {file.status === 'failed' && 'Ошибка'}
                    </span>
                  </div>
                ))}
              </div>
            </div>

            <div className="rounded-2xl border border-white/10 bg-slate-950/40 p-6 shadow-soft">
              <h3 className="text-lg font-semibold text-white">Советы</h3>
              <ul className="mt-3 space-y-2 text-sm text-slate-300">
                <li>• Линки API вынесены в переменную окружения <code className="text-brand-200">VITE_API_BASE_URL</code>.</li>
                <li>• После успешной отправки форма очищается, а капча обновляется автоматически.</li>
                <li>• Пока вложение загружается, отправка комментария блокируется.</li>
              </ul>
            </div>
          </aside>
        </main>
      </div>
    </div>
  )
}

export default App

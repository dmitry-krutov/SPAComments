import { ChangeEvent, FormEvent, useEffect, useMemo, useState } from 'react'
import { useAppDispatch, useAppSelector } from './app/hooks'
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

  const handleInput = (field: keyof typeof initialFormState) =>
    (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
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
    <div className="relative min-h-screen overflow-hidden bg-slate-950 text-slate-50">
      <div className="pointer-events-none absolute inset-0 opacity-50">
        <div className="absolute -left-24 top-[-10%] h-64 w-64 rounded-full bg-brand-500/30 blur-[120px]" />
        <div className="absolute right-0 top-1/3 h-72 w-72 rounded-full bg-emerald-400/20 blur-[140px]" />
      </div>

      <div className="relative mx-auto flex max-w-5xl flex-col gap-8 px-4 py-12">
        <header className="flex flex-col gap-3">
          <div className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-4 py-2 text-xs uppercase tracking-[0.18em] text-slate-200">
            Комментарии
            <span className="h-1.5 w-1.5 rounded-full bg-emerald-400" />
          </div>
          <h1 className="text-4xl font-semibold text-white">Создайте новый комментарий</h1>
          <p className="max-w-2xl text-lg text-slate-300">
            Чистый фокус на вашем сообщении: заполните форму, прикрепите нужные файлы и подтвердите капчу. Скоро здесь появится
            лента отправленных комментариев.
          </p>
        </header>

        <form
          className="rounded-3xl border border-white/10 bg-white/5 p-8 shadow-soft backdrop-blur"
          onSubmit={handleSubmit}
        >
          <div className="flex flex-col gap-6 lg:grid lg:grid-cols-[1.1fr,0.9fr]">
            <div className="space-y-6">
              <div className="grid gap-4 md:grid-cols-2">
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
                    placeholder="Как к вам обращаться?"
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
                    Сайт или профиль
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
                  <label className="flex items-center justify-between text-sm text-slate-200" htmlFor="attachments">
                    Вложения
                    <span className="text-xs text-slate-400">до нескольких файлов</span>
                  </label>
                  <label className="flex h-[52px] cursor-pointer items-center justify-between rounded-xl border border-dashed border-brand-400/60 bg-brand-500/10 px-4 text-sm text-brand-100 transition hover:bg-brand-500/20">
                    <input id="attachments" type="file" className="hidden" multiple onChange={handleFiles} />
                    <span>Перетащите или выберите файлы</span>
                    <span className="rounded-full bg-white/20 px-3 py-1 text-xs text-white">Загрузить</span>
                  </label>
                </div>
              </div>

              <div className="space-y-2">
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
                  placeholder="Поделитесь идеей или отзывом"
                />
              </div>

              {attachments.length > 0 && (
                <div className="space-y-3 rounded-2xl border border-white/10 bg-slate-900/60 p-4">
                  <div className="flex items-center justify-between text-sm text-slate-200">
                    <span>Прикреплённые файлы</span>
                    {hasUploading && <span className="text-xs text-amber-200">идёт загрузка…</span>}
                  </div>
                  <div className="space-y-2">
                    {attachments.map((file) => (
                      <div
                        key={file.localId}
                        className="flex items-center justify-between gap-3 rounded-xl border border-white/5 bg-white/5 px-3 py-2"
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
                          className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${
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
              )}
            </div>

            <div className="space-y-6 rounded-2xl border border-white/10 bg-slate-900/60 p-6">
              <div className="flex items-center justify-between gap-3">
                <div className="space-y-1">
                  <p className="text-sm font-medium text-white">Проверка безопасности</p>
                  <p className="text-xs text-slate-400">Введите буквы с картинки, чтобы отправить форму.</p>
                </div>
                <button
                  type="button"
                  onClick={() => dispatch(fetchCaptcha())}
                  className="inline-flex items-center gap-2 rounded-full bg-white/10 px-3 py-2 text-xs font-medium text-white transition hover:bg-white/20"
                >
                  Обновить
                </button>
              </div>

              <div className="flex flex-col gap-4">
                <div className="flex h-24 items-center justify-center rounded-xl border border-white/10 bg-slate-950/60">
                  {captcha.status === 'loading' && <div className="text-sm text-slate-300">Загрузка...</div>}
                  {captcha.status === 'failed' && <div className="text-sm text-rose-300">Не удалось загрузить капчу</div>}
                  {captcha.status === 'succeeded' && (
                    <img src={captchaImageSrc} alt="Капча" className="h-full w-full rounded-md object-contain" />
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
                    placeholder="Введите символы"
                  />
                  {captcha.error && <p className="text-xs text-rose-300">{captcha.error}</p>}
                </div>
              </div>

              <div className="space-y-3">
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
                  className="inline-flex w-full items-center justify-center gap-2 rounded-full bg-brand-500 px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-brand-500/30 transition hover:bg-brand-600 disabled:cursor-not-allowed disabled:bg-slate-600"
                >
                  {isSubmitting ? 'Отправляем…' : 'Отправить комментарий'}
                </button>
                <p className="text-xs text-slate-400">Заполнение займёт меньше минуты.</p>
              </div>
            </div>
          </div>
        </form>
      </div>
    </div>
  )
}

export default App

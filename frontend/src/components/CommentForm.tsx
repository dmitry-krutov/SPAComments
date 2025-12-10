import { useCallback, useMemo, useState, type ChangeEvent, type DragEvent, type FormEvent } from 'react'
import { ApiErrorResponse, formatUnknownError } from '../lib/apiClient'
import { createComment, getCaptcha, uploadCommentAttachment } from '../features/comments/api'
import type { CaptchaResponse, CommentDto, UploadCommentAttachmentResult } from '../features/comments/types'

const initialFormState = {
  userName: '',
  email: '',
  homePage: '',
  text: '',
  captchaAnswer: '',
}

const formatSize = (size: number) => {
  if (size < 1024) return `${size} Б`
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} КБ`
  return `${(size / (1024 * 1024)).toFixed(1)} МБ`
}

interface AttachmentItem {
  localId: string
  fileName: string
  size: number
  status: 'idle' | 'uploading' | 'succeeded' | 'failed'
  error?: string
  serverId?: string
  meta?: Omit<UploadCommentAttachmentResult, 'fileId'>
}

type SubmitStatus = 'idle' | 'submitting' | 'succeeded' | 'failed'

interface CommentFormProps {
  parentId?: string | null
  onSubmitted: (comment: CommentDto) => void
  onCancel?: () => void
  heading?: string
  compact?: boolean
}

export function CommentForm({ parentId = null, onSubmitted, onCancel, heading, compact }: CommentFormProps) {
  const [form, setForm] = useState(initialFormState)
  const [attachments, setAttachments] = useState<AttachmentItem[]>([])
  const [captchaVisible, setCaptchaVisible] = useState(false)
  const [captcha, setCaptcha] = useState<{ status: 'idle' | 'loading' | 'succeeded' | 'failed'; data?: CaptchaResponse; error?: string }>({
    status: 'idle',
  })
  const [submitStatus, setSubmitStatus] = useState<SubmitStatus>('idle')
  const [submitError, setSubmitError] = useState<string | undefined>()

  const isUploading = attachments.some((a) => a.status === 'uploading')

  const captchaImageSrc = useMemo(() => {
    if (!captcha.data) return ''
    return `data:${captcha.data.contentType};base64,${captcha.data.imageBase64}`
  }, [captcha.data])

  const handleInput = (field: keyof typeof initialFormState) =>
    (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      setForm((prev) => ({ ...prev, [field]: event.target.value }))
    }

  const refreshCaptcha = useCallback(async () => {
    setCaptcha({ status: 'loading' })
    try {
      const data = await getCaptcha()
      setCaptcha({ status: 'succeeded', data, error: undefined })
    } catch (error) {
      setCaptcha({ status: 'failed', error: formatUnknownError(error, 'Не удалось загрузить капчу') })
    }
  }, [])

  const uploadFiles = useCallback((files: FileList) => {
    Array.from(files).forEach(async (file) => {
      const localId = crypto.randomUUID()
      setAttachments((prev) => [...prev, { localId, fileName: file.name, size: file.size, status: 'uploading' }])

      try {
        const response = await uploadCommentAttachment(file)
        setAttachments((prev) =>
          prev.map((item) =>
            item.localId === localId
              ? {
                  ...item,
                  status: 'succeeded',
                  serverId: response.fileId,
                  meta: {
                    kind: response.kind,
                    contentType: response.contentType,
                    size: response.size,
                    width: response.width,
                    height: response.height,
                  },
                  error: undefined,
                }
              : item
          )
        )
      } catch (error) {
        setAttachments((prev) =>
          prev.map((item) =>
            item.localId === localId
              ? { ...item, status: 'failed', error: formatUnknownError(error, 'Не удалось загрузить файл') }
              : item
          )
        )
      }
    })
  }, [])

  const handleFiles = (event: ChangeEvent<HTMLInputElement>) => {
    if (event.target.files?.length) {
      uploadFiles(event.target.files)
      event.target.value = ''
    }
  }

  const handleDrop = (event: DragEvent<HTMLLabelElement>) => {
    event.preventDefault()
    if (event.dataTransfer.files?.length) {
      uploadFiles(event.dataTransfer.files)
    }
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!captchaVisible) {
      setCaptchaVisible(true)
      if (captcha.status === 'idle') {
        refreshCaptcha()
      }
      return
    }

    if (captcha.status !== 'succeeded') {
      if (captcha.status === 'idle') refreshCaptcha()
      return
    }

    setSubmitStatus('submitting')
    setSubmitError(undefined)

    try {
      const attachmentIds = attachments
        .filter((a) => a.status === 'succeeded' && a.serverId)
        .map((a) => a.serverId as string)

      const payload = {
        parentId,
        userName: form.userName.trim(),
        email: form.email.trim(),
        homePage: form.homePage.trim() || null,
        text: form.text.trim(),
        captchaId: captcha.data!.id,
        captchaAnswer: form.captchaAnswer.trim(),
        attachmentIds,
      }

      const created = await createComment(payload)

      const merged: CommentDto = {
        ...created,
        attachments: created.attachments.map((attachment) => {
          const meta = attachments.find((a) => a.serverId === attachment.fileId)?.meta
          return meta ? { ...attachment, contentType: meta.contentType } : attachment
        }),
      }

      setSubmitStatus('succeeded')
      setForm(initialFormState)
      setAttachments([])
      setCaptchaVisible(false)
      setCaptcha({ status: 'idle' })
      onSubmitted(merged)
    } catch (error) {
      setSubmitStatus('failed')
      if (error instanceof ApiErrorResponse && error.errors.some((item) => item.code === 'captcha.invalid')) {
        refreshCaptcha()
        setForm((prev) => ({ ...prev, captchaAnswer: '' }))
      }

      setSubmitError(formatUnknownError(error, 'Не удалось отправить комментарий'))
    }
  }

  const disabled = submitStatus === 'submitting' || isUploading || (captchaVisible && captcha.status !== 'succeeded')

  return (
    <form
      className={`rounded-2xl border border-white/10 bg-white/5 p-6 shadow-soft backdrop-blur ${compact ? 'mt-4' : ''}`}
      onSubmit={handleSubmit}
    >
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="space-y-1">
          {heading && <h2 className="text-xl font-semibold text-white">{heading}</h2>}
          <p className="text-sm text-slate-300">Поля, отмеченные звёздочкой, обязательны.</p>
        </div>
      </div>

      <div className="mt-6 grid gap-4 md:grid-cols-2">
        <div className="space-y-2">
          <label className="text-sm text-slate-200" htmlFor={`${parentId || 'root'}-userName`}>
            Username*
          </label>
          <input
            id={`${parentId || 'root'}-userName`}
            required
            value={form.userName}
            onChange={handleInput('userName')}
            className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
            placeholder="username"
          />
        </div>

        <div className="space-y-2">
          <label className="text-sm text-slate-200" htmlFor={`${parentId || 'root'}-email`}>
            Email*
          </label>
          <input
            id={`${parentId || 'root'}-email`}
            type="email"
            required
            value={form.email}
            onChange={handleInput('email')}
            className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
            placeholder="name@example.com"
          />
        </div>

        <div className="space-y-2">
          <label className="text-sm text-slate-200" htmlFor={`${parentId || 'root'}-homePage`}>
            Домашняя страница
          </label>
          <input
            id={`${parentId || 'root'}-homePage`}
            type="url"
            value={form.homePage}
            onChange={handleInput('homePage')}
            className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
            placeholder="https://example.com"
          />
        </div>

        <div className="space-y-2">
          <label className="text-sm text-slate-200" htmlFor={`${parentId || 'root'}-attachments`}>
            Вложения
          </label>
          <label
            htmlFor={`${parentId || 'root'}-attachments`}
            onDragOver={(event) => event.preventDefault()}
            onDrop={handleDrop}
            className="flex h-[52px] cursor-pointer items-center justify-between rounded-xl border border-dashed border-brand-400/60 bg-brand-500/10 px-4 text-sm text-brand-100 transition hover:bg-brand-500/20"
          >
            <input id={`${parentId || 'root'}-attachments`} type="file" className="hidden" multiple onChange={handleFiles} />
            <span>Перетащите или выберите файлы</span>
            <span className="rounded-full bg-white/20 px-3 py-1 text-xs text-white">Загрузить</span>
          </label>
          {attachments.length > 0 && (
            <div className="space-y-2 rounded-xl border border-white/10 bg-white/5 p-3">
              {attachments.map((file) => (
                <div key={file.localId} className="flex items-start justify-between gap-3 rounded-lg bg-white/5 px-3 py-2">
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
          )}
        </div>
      </div>

      <div className="mt-4 space-y-2">
        <label className="text-sm text-slate-200" htmlFor={`${parentId || 'root'}-text`}>
          Текст комментария*
        </label>
        <textarea
          id={`${parentId || 'root'}-text`}
          required
          value={form.text}
          onChange={handleInput('text')}
          rows={4}
          className="w-full rounded-2xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
          placeholder="Поделитесь своими мыслями"
        />
      </div>

      {captchaVisible && (
        <div className="mt-6 grid gap-4 md:grid-cols-[auto,1fr] md:items-center">
          <div className="flex items-center gap-3">
            <div className="flex flex-col items-center gap-2">
              <div className="flex h-24 w-32 items-center justify-center rounded-xl border border-white/10 bg-slate-950/40">
                {captcha.status === 'loading' && <div className="text-sm text-slate-300">Загрузка...</div>}
                {captcha.status === 'failed' && <div className="text-sm text-rose-300">Не удалось загрузить капчу</div>}
                {captcha.status === 'succeeded' && (
                  <img src={captchaImageSrc} alt="Капча" className="h-full w-full rounded-md object-contain" />
                )}
              </div>
              <button
                type="button"
                onClick={refreshCaptcha}
                className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-3 py-2 text-xs font-medium text-white transition hover:border-brand-400/50 hover:text-white"
              >
                <span className="text-emerald-300">↻</span>
                Обновить капчу
              </button>
            </div>
            <div className="space-y-2">
              <label className="text-sm text-slate-200" htmlFor={`${parentId || 'root'}-captchaAnswer`}>
                Ответ на капчу*
              </label>
              <input
                id={`${parentId || 'root'}-captchaAnswer`}
                required
                value={form.captchaAnswer}
                onChange={handleInput('captchaAnswer')}
                className="w-full rounded-xl border border-white/10 bg-white/10 px-4 py-3 text-white outline-none transition focus:border-brand-400 focus:ring-2 focus:ring-brand-400/40"
                placeholder="Введите буквы с картинки"
              />
              {captcha.error && <p className="text-xs text-rose-300">{captcha.error}</p>}
            </div>
          </div>
        </div>
      )}

      <div className="mt-6 flex flex-wrap items-center gap-3">
        <button
          type="submit"
          disabled={disabled}
          className="inline-flex items-center justify-center gap-2 rounded-full bg-brand-500 px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-brand-500/30 transition hover:bg-brand-600 disabled:cursor-not-allowed disabled:bg-slate-500"
        >
          {submitStatus === 'submitting' ? 'Отправляем…' : 'Отправить комментарий'}
        </button>
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            className="rounded-full border border-white/10 bg-white/10 px-4 py-2 text-sm text-white transition hover:bg-white/20"
          >
            Отмена
          </button>
        )}
        {isUploading && <p className="text-xs text-slate-300">Подождите, пока завершится загрузка файлов.</p>}
        {submitError && <p className="text-xs text-rose-300">{submitError}</p>}
      </div>
    </form>
  )
}

export default CommentForm

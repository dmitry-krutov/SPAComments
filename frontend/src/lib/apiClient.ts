import { config } from '../config'

export interface ApiError {
  code: string
  message: string
  invalidField?: string | null
  type?: string
}

export interface Envelope<T> {
  result?: T
  errors?: ApiError[]
  isError?: boolean
  timeGenerated?: string
}

const fieldTitles: Record<string, string> = {
  username: 'Имя пользователя',
  userName: 'Имя пользователя',
  email: 'Email',
  homepage: 'Домашняя страница',
  homePage: 'Домашняя страница',
  text: 'Текст комментария',
  captchaanswer: 'Ответ на капчу',
  captchaAnswer: 'Ответ на капчу',
  captchaId: 'Идентификатор капчи',
  captchaid: 'Идентификатор капчи',
  parentid: 'Родительский комментарий',
  id: 'Идентификатор',
  page: 'Номер страницы',
  Page: 'Номер страницы',
  pageSize: 'Размер страницы',
  pagesize: 'Размер страницы',
  file: 'Файл',
  fileid: 'Файл',
  contenttype: 'Тип файла',
}

const codeMessageBuilders: Record<string, (error: ApiError) => string> = {
  'value.is.required': (error) => `${formatFieldTitle(error.invalidField)} обязательно для заполнения`,
  'value.too.small': (error) => `${formatFieldTitle(error.invalidField)} не может быть короче ${extractNumber(error.message)} символов`,
  'value.too.long': (error) => `${formatFieldTitle(error.invalidField)} не может превышать ${extractNumber(error.message)} символов`,
  'value.too.large': (error) => `${formatFieldTitle(error.invalidField)} не должно быть больше ${extractNumber(error.message)}`,
  'value.invalid.format': (error) => {
    const detail = extractDetail(error.message)
    const base = `${formatFieldTitle(error.invalidField)} имеет неверный формат`
    return detail ? `${base}: ${detail}` : base
  },
  'captcha.id.required': () => 'Капча не загружена. Обновите страницу и попробуйте снова',
  'captcha.answer.required': () => 'Введите ответ на капчу',
  'captcha.invalid': () => 'Неверный ответ на капчу. Попробуйте ещё раз',
  'comments.attachments.invalid-content-type': () =>
    'Формат файла не поддерживается. Разрешены PNG, JPG, GIF или текстовые файлы',
  'comments.attachments.text-too-large': () => 'Текстовый файл слишком большой. Максимальный размер — 100 КБ',
  'comments.attachments.not-found': () => 'Не удалось найти один или несколько файлов. Загрузите вложения заново',
  'comments.latest.attachments.not-found': () => 'Не удалось получить ссылки на вложения. Попробуйте обновить страницу',
  'comments.latest.page.min': () => 'Номер страницы должен быть больше нуля',
  'comments.latest.pageSize.range': () => 'Размер страницы должен быть от 1 до 100',
  'comments.search.page.min': () => 'Номер страницы должен быть больше нуля',
  'comments.search.pageSize.range': () => 'Размер страницы должен быть от 1 до 100',
  'comments.get-by-id.id.required': () => 'Не указан идентификатор комментария',
  'comments.get-by-id.not-found': () => 'Комментарий не найден или был удалён',
}

const defaultHeaders = {
  Accept: 'application/json',
}

const normalizeFieldKey = (field?: string | null) => (field ?? '').replace(/\W+/g, '').toLowerCase()

function formatFieldTitle(field?: string | null) {
  if (!field) return 'Поле'
  const normalized = normalizeFieldKey(field)
  return fieldTitles[normalized] ?? fieldTitles[field] ?? field
}

const extractNumber = (text: string) => {
  const match = text.match(/([0-9]+)/)
  return match ? match[1] : 'заданного значения'
}

const extractDetail = (text: string) => {
  const parts = text.split(':')
  return parts.length > 1 ? parts.slice(1).join(':').trim() : ''
}

export const formatApiErrorMessage = (error: ApiError) => {
  const builder = codeMessageBuilders[error.code]
  if (builder) return builder(error)

  if (error.message) {
    const formattedField = formatFieldTitle(error.invalidField)
    if (error.invalidField) {
      return `${formattedField}: ${error.message}`
    }

    return error.message
  }

  return 'Произошла ошибка. Попробуйте позже'
}

export class ApiErrorResponse extends Error {
  errors: ApiError[]
  userMessages: string[]

  constructor(errors: ApiError[], fallbackMessage = 'Не удалось выполнить запрос') {
    const safeErrors = errors.map((error) => ({ ...error, invalidField: error.invalidField ?? undefined }))
    const userMessages = safeErrors.map(formatApiErrorMessage)
    super(userMessages.join('; ') || fallbackMessage)
    this.errors = safeErrors
    this.userMessages = userMessages
  }
}

export function formatUnknownError(error: unknown, fallbackMessage: string) {
  if (error instanceof ApiErrorResponse) {
    return error.userMessages.join('; ')
  }

  if (error instanceof Error) return error.message

  return fallbackMessage
}

const defaultErrorMessage = 'Не удалось выполнить запрос'

function buildUrl(path: string) {
  return `${config.apiBaseUrl}${path}`
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(buildUrl(path), {
    ...options,
    headers: {
      ...defaultHeaders,
      ...(options.headers ?? {}),
    },
  })

  const raw = await response.text()
  let data: Envelope<T> | null = null

  if (raw) {
    try {
      data = JSON.parse(raw) as Envelope<T>
    } catch {
      data = null
    }
  }

  if (!response.ok) {
    if (data?.errors?.length) {
      throw new ApiErrorResponse(data.errors, defaultErrorMessage)
    }

    throw new Error(`Не удалось выполнить запрос (код ${response.status})`)
  }

  if (!data) {
    throw new Error('Некорректный ответ от сервера')
  }

  if (data.errors && data.errors.length) {
    throw new ApiErrorResponse(data.errors, defaultErrorMessage)
  }

  return data.result as T
}

export async function apiFetchForm<T>(path: string, formData: FormData, options: RequestInit = {}) {
  return apiFetch<T>(path, {
    method: 'POST',
    body: formData,
    ...options,
    headers: {
      ...defaultHeaders,
      ...(options.headers ?? {}),
    },
  })
}

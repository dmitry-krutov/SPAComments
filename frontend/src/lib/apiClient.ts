import { config } from '../config'

export interface ApiError {
  code: string
  message: string
  invalidField?: string | null
}

export interface Envelope<T> {
  result?: T
  errors?: ApiError[]
  isError?: boolean
  timeGenerated?: string
}

export class ApiErrorResponse extends Error {
  errors: ApiError[]

  constructor(errors: ApiError[], message?: string) {
    super(message ?? errors.map((e) => e.message).join('; '))
    this.errors = errors
  }
}

const defaultHeaders = {
  Accept: 'application/json',
}

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

  if (!response.ok) {
    throw new Error(`Запрос завершился с кодом ${response.status}`)
  }

  const data = (await response.json()) as Envelope<T>

  if (data.errors && data.errors.length) {
    throw new ApiErrorResponse(data.errors)
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

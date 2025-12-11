import { apiFetch, apiFetchForm } from '../../lib/apiClient'
import type {
  CaptchaResponse,
  CommentDto,
  CommentSearchItemDto,
  CreateCommentRequest,
  PagedResult,
  UploadCommentAttachmentResult,
} from './types'

export const getCaptcha = () => apiFetch<CaptchaResponse>('/captcha')

export const uploadCommentAttachment = (file: File) => {
  const formData = new FormData()
  formData.append('file', file)
  return apiFetchForm<UploadCommentAttachmentResult>('/Comments/attachments', formData)
}

export const createComment = (payload: CreateCommentRequest) =>
  apiFetch<CommentDto>('/Comments', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

interface LatestCommentsParams {
  page: number
  pageSize: number
}

export const getLatestComments = ({ page, pageSize }: LatestCommentsParams) =>
  apiFetch<PagedResult<CommentDto>>(`/Comments?page=${page}&pageSize=${pageSize}`)

interface SearchCommentsParams {
  text: string
  page?: number
  pageSize?: number
}

export const searchComments = ({ text, page = 1, pageSize = 10 }: SearchCommentsParams) =>
  apiFetch<PagedResult<CommentSearchItemDto>>('/Comments/search', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ text, page, pageSize }),
  })

export const getCommentById = (id: string) => apiFetch<CommentDto>(`/Comments/${id}`)
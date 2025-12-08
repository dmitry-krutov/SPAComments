import { apiFetch, apiFetchForm } from '../../lib/apiClient'
import type {
  CaptchaResponse,
  CommentDto,
  CreateCommentRequest,
  PagedResult,
  UploadCommentAttachmentResult,
} from './types'

export const getCaptcha = () => apiFetch<CaptchaResponse>('/api/captcha')

export const uploadCommentAttachment = (file: File) => {
  const formData = new FormData()
  formData.append('file', file)
  return apiFetchForm<UploadCommentAttachmentResult>('/api/Comments/attachments', formData)
}

export const createComment = (payload: CreateCommentRequest) =>
  apiFetch<CommentDto>('/api/Comments', {
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
  apiFetch<PagedResult<CommentDto>>(`/api/Comments?page=${page}&pageSize=${pageSize}`)

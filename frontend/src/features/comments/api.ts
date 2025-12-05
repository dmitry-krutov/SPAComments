import { apiFetch, apiFetchForm } from '../../lib/apiClient'
import type {
  CaptchaResponse,
  CommentDto,
  CreateCommentRequest,
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

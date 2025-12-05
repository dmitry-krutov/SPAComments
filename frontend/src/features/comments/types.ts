export interface CaptchaResponse {
  id: string
  imageBase64: string
  contentType: string
}

export interface UploadCommentAttachmentResult {
  fileId: string
  kind: string
  contentType: string
  size: number
  width?: number
  height?: number
}

export interface CommentAttachmentDto {
  fileId: string
  url: string
  expiresAtUtc: string
}

export interface CommentDto {
  id: string
  parentId?: string | null
  userName: string
  email: string
  homePage?: string | null
  text: string
  createdAt: string
  attachments: CommentAttachmentDto[]
}

export interface CreateCommentRequest {
  parentId: string | null
  userName: string
  email: string
  homePage: string | null
  text: string
  captchaId: string
  captchaAnswer: string
  attachmentIds?: string[]
}

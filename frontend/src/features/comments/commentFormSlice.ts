import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { RootState } from '../../app/store'
import { ApiErrorResponse } from '../../lib/apiClient'
import { createComment, getCaptcha, uploadCommentAttachment } from './api'
import type {
  CaptchaResponse,
  CommentDto,
  UploadCommentAttachmentResult,
} from './types'

interface CaptchaState {
  status: 'idle' | 'loading' | 'succeeded' | 'failed'
  data?: CaptchaResponse
  error?: string
}

interface AttachmentItem {
  localId: string
  fileName: string
  size: number
  status: 'uploading' | 'succeeded' | 'failed'
  error?: string
  serverId?: string
  meta?: Omit<UploadCommentAttachmentResult, 'fileId'>
}

interface SubmitState {
  status: 'idle' | 'submitting' | 'succeeded' | 'failed'
  error?: string
  successMessage?: string
  createdComment?: CommentDto
}

interface CommentFormState {
  captcha: CaptchaState
  attachments: AttachmentItem[]
  submit: SubmitState
}

const initialState: CommentFormState = {
  captcha: { status: 'idle' },
  attachments: [],
  submit: { status: 'idle' },
}

const stringifyError = (error: unknown) => {
  if (error instanceof ApiErrorResponse) {
    return error.errors
      .map((err) => (err.invalidField ? `${err.invalidField}: ${err.message}` : err.message))
      .join('; ')
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Не удалось выполнить запрос'
}

export const fetchCaptcha = createAsyncThunk<CaptchaResponse, void, { rejectValue: string }>(
  'commentForm/fetchCaptcha',
  async (_, { rejectWithValue }) => {
    try {
      return await getCaptcha()
    } catch (error) {
      return rejectWithValue(stringifyError(error))
    }
  }
)

interface UploadAttachmentArgs {
  file: File
  localId: string
}

export const uploadAttachment = createAsyncThunk<
  { localId: string; file: File; response: UploadCommentAttachmentResult },
  UploadAttachmentArgs,
  { rejectValue: string }
>('commentForm/uploadAttachment', async ({ file, localId }, { rejectWithValue }) => {
  try {
    const response = await uploadCommentAttachment(file)
    return { localId, file, response }
  } catch (error) {
    return rejectWithValue(stringifyError(error))
  }
})

interface SubmitCommentArgs {
  userName: string
  email: string
  homePage: string
  text: string
  captchaAnswer: string
}

export const submitComment = createAsyncThunk<CommentDto, SubmitCommentArgs, { rejectValue: string; state: RootState }>(
  'commentForm/submitComment',
  async (payload, { rejectWithValue, getState }) => {
    const state = getState()
    const captchaId = state.commentForm.captcha.data?.id

    if (!captchaId) {
      return rejectWithValue('Капча не загружена, попробуйте обновить страницу')
    }

    const attachmentIds = state.commentForm.attachments
      .filter((a) => a.status === 'succeeded' && a.serverId)
      .map((a) => a.serverId as string)

    try {
      const request = {
        parentId: null,
        userName: payload.userName.trim(),
        email: payload.email.trim(),
        homePage: payload.homePage.trim() || null,
        text: payload.text.trim(),
        captchaId,
        captchaAnswer: payload.captchaAnswer.trim(),
        attachmentIds,
      }

      return await createComment(request)
    } catch (error) {
      return rejectWithValue(stringifyError(error))
    }
  }
)

const commentFormSlice = createSlice({
  name: 'commentForm',
  initialState,
  reducers: {
    resetSubmitState(state) {
      state.submit = { status: 'idle' }
    },
    clearAttachments(state) {
      state.attachments = []
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchCaptcha.pending, (state) => {
        state.captcha.status = 'loading'
        state.captcha.error = undefined
      })
      .addCase(fetchCaptcha.fulfilled, (state, action: PayloadAction<CaptchaResponse>) => {
        state.captcha.status = 'succeeded'
        state.captcha.data = action.payload
      })
      .addCase(fetchCaptcha.rejected, (state, action) => {
        state.captcha.status = 'failed'
        state.captcha.error = action.payload || 'Не удалось загрузить капчу'
      })

    builder
      .addCase(uploadAttachment.pending, (state, action) => {
        const { localId, file } = action.meta.arg
        const existing = state.attachments.find((x) => x.localId === localId)
        const item: AttachmentItem = {
          localId,
          fileName: file.name,
          size: file.size,
          status: 'uploading',
        }

        if (existing) {
          Object.assign(existing, item)
        } else {
          state.attachments.push(item)
        }
      })
      .addCase(uploadAttachment.fulfilled, (state, action) => {
        const { localId, file, response } = action.payload
        const target = state.attachments.find((x) => x.localId === localId)
        if (!target) return

        target.status = 'succeeded'
        target.serverId = response.fileId
        target.meta = {
          kind: response.kind,
          contentType: response.contentType,
          size: response.size,
          width: response.width,
          height: response.height,
        }
        target.fileName = file.name
        target.size = file.size
        target.error = undefined
      })
      .addCase(uploadAttachment.rejected, (state, action) => {
        const { localId } = action.meta.arg
        const target = state.attachments.find((x) => x.localId === localId)

        if (target) {
          target.status = 'failed'
          target.error = action.payload || 'Ошибка загрузки файла'
        } else {
          state.attachments.push({
            localId,
            fileName: 'Неизвестный файл',
            size: 0,
            status: 'failed',
            error: action.payload || 'Ошибка загрузки файла',
          })
        }
      })

    builder
      .addCase(submitComment.pending, (state) => {
        state.submit.status = 'submitting'
        state.submit.error = undefined
        state.submit.successMessage = undefined
      })
      .addCase(submitComment.fulfilled, (state, action) => {
        state.submit.status = 'succeeded'
        state.submit.successMessage = 'Комментарий отправлен!'
        state.submit.createdComment = action.payload
        state.attachments = []
      })
      .addCase(submitComment.rejected, (state, action) => {
        state.submit.status = 'failed'
        state.submit.error = action.payload || 'Не удалось отправить комментарий'
      })
  },
})

export const { resetSubmitState, clearAttachments } = commentFormSlice.actions
export default commentFormSlice.reducer

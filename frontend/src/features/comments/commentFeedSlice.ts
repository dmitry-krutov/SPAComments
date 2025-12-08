import { createAsyncThunk, createSlice, type PayloadAction } from '@reduxjs/toolkit'
import { RootState } from '../../app/store'
import { ApiErrorResponse } from '../../lib/apiClient'
import { getLatestComments } from './api'
import type { CommentDto, PagedResult } from './types'

interface CommentFeedState {
  items: CommentDto[]
  page: number
  pageSize: number
  totalCount: number
  status: 'idle' | 'loading' | 'succeeded' | 'failed'
  error?: string
}

const initialState: CommentFeedState = {
  items: [],
  page: 1,
  pageSize: 10,
  totalCount: 0,
  status: 'idle',
}

const stringifyError = (error: unknown) => {
  if (error instanceof ApiErrorResponse) {
    return error.errors.map((e) => e.message).join('; ')
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Не удалось загрузить комментарии'
}

export const fetchLatestComments = createAsyncThunk<
  PagedResult<CommentDto>,
  { page?: number } | undefined,
  { rejectValue: string; state: RootState }
>('commentFeed/fetchLatest', async (args, { getState, rejectWithValue }) => {
  const state = getState().commentFeed
  const page = args?.page ?? state.page

  try {
    return await getLatestComments({ page, pageSize: state.pageSize })
  } catch (error) {
    return rejectWithValue(stringifyError(error))
  }
})

const commentFeedSlice = createSlice({
  name: 'commentFeed',
  initialState,
  reducers: {
    prependComment(state, action: PayloadAction<CommentDto>) {
      const exists = state.items.some((item) => item.id === action.payload.id)
      if (exists) return

      state.items.unshift(action.payload)
      state.totalCount += 1
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchLatestComments.pending, (state, action) => {
        const requestedPage = action.meta.arg?.page
        if (requestedPage) state.page = requestedPage
        state.status = 'loading'
        state.error = undefined
      })
      .addCase(fetchLatestComments.fulfilled, (state, action) => {
        state.status = 'succeeded'
        state.items = action.payload.items
        state.page = action.payload.page
        state.pageSize = action.payload.pageSize
        state.totalCount = action.payload.totalCount
      })
      .addCase(fetchLatestComments.rejected, (state, action) => {
        state.status = 'failed'
        state.error = action.payload || 'Не удалось загрузить комментарии'
      })
  },
})

export const { prependComment } = commentFeedSlice.actions
export default commentFeedSlice.reducer

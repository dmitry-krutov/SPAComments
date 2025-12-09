import { configureStore } from '@reduxjs/toolkit'
import commentFeedReducer from '../features/comments/commentFeedSlice'
import commentFormReducer from '../features/comments/commentFormSlice'

export const store = configureStore({
  reducer: {
    commentForm: commentFormReducer,
    commentFeed: commentFeedReducer,
  },
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

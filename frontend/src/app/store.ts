import { configureStore } from '@reduxjs/toolkit'
import commentFormReducer from '../features/comments/commentFormSlice'

export const store = configureStore({
  reducer: {
    commentForm: commentFormReducer,
  },
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

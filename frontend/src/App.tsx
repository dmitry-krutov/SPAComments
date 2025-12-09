import { Navigate, Route, Routes } from 'react-router-dom'
import CommentDetailsPage from './pages/CommentDetailsPage'
import CommentsPage from './pages/CommentsPage'
import SearchResultsPage from './pages/SearchResultsPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<CommentsPage />} />
      <Route path="/comments/search/:text" element={<SearchResultsPage />} />
      <Route path="/comments/:id" element={<CommentDetailsPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App

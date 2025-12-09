import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import CommentCard from '../components/CommentCard'
import PageLayout from '../components/PageLayout'
import { getCommentById } from '../features/comments/api'
import type { CommentDto } from '../features/comments/types'
import { ApiErrorResponse } from '../lib/apiClient'

const stringifyError = (error: unknown) => {
  if (error instanceof ApiErrorResponse) {
    return error.errors.map((e) => e.message).join('; ')
  }

  if (error instanceof Error) return error.message
  return 'Не удалось загрузить комментарий'
}

type LoadState = 'idle' | 'loading' | 'succeeded' | 'failed'

function CommentDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const [status, setStatus] = useState<LoadState>('idle')
  const [comment, setComment] = useState<CommentDto | null>(null)
  const [error, setError] = useState<string | undefined>()

  useEffect(() => {
    if (!id) return
    const fetchComment = async () => {
      setStatus('loading')
      setError(undefined)
      try {
        const data = await getCommentById(id)
        setComment(data)
        setStatus('succeeded')
      } catch (err) {
        setStatus('failed')
        setError(stringifyError(err))
      }
    }

    fetchComment()
  }, [id])

  return (
    <PageLayout title="Комментарий">
      <section className="flex flex-col gap-4 rounded-2xl border border-white/10 bg-white/5 p-6 shadow-soft backdrop-blur">
        {status === 'loading' && (
          <div className="rounded-xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-slate-300">Загружаем комментарий…</div>
        )}

        {status === 'failed' && error && (
          <div className="rounded-xl border border-rose-400/30 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">{error}</div>
        )}

        {status === 'succeeded' && comment && (
          <div className="rounded-2xl border border-white/10 bg-white/5 p-4 shadow-sm">
            <CommentCard comment={comment} showReplyAction={false} />
          </div>
        )}
      </section>
    </PageLayout>
  )
}

export default CommentDetailsPage

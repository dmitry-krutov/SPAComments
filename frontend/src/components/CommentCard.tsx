import { Link } from 'react-router-dom'
import type { CommentDto } from '../features/comments/types'
import { formatDate } from '../lib/formatDate'

const getAttachmentInfo = (contentType?: string) => {
  if (contentType?.startsWith('image/')) return { icon: '📎', label: 'Изображение' }
  if (contentType?.startsWith('text/')) return { icon: '📄', label: 'Текстовый файл' }
  return { icon: '📁', label: 'Файл' }
}

interface CommentCardProps {
  comment: CommentDto
  onReply?: (target: { id: string; userName: string; text: string }) => void
  showReplyAction?: boolean
  showPermalink?: boolean
  permalinkTo?: string
  registerRef?: (el: HTMLDivElement | null) => void
}

export function CommentCard({
  comment,
  onReply,
  showReplyAction = true,
  showPermalink = false,
  permalinkTo,
  registerRef,
}: CommentCardProps) {
  const shortText = comment.text.trim()

  return (
    <div ref={registerRef} className="relative space-y-3 pr-8">
      {showPermalink && permalinkTo && (
        <Link
          to={permalinkTo}
          className="absolute right-0 top-0 text-lg text-slate-300 transition hover:text-white"
          aria-label="Открыть комментарий"
        >
          ⧉
        </Link>
      )}

      <div className="flex flex-wrap items-start gap-3">
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white/10 text-sm font-semibold text-white">
            {comment.userName.charAt(0).toUpperCase()}
          </div>
          <div className="space-y-1">
            <div className="flex flex-wrap items-center gap-2">
              <p className="text-base font-semibold text-white">{comment.userName}</p>
              {comment.email && <p className="text-xs text-slate-400">{comment.email}</p>}
              {comment.homePage && (
                <a
                  href={comment.homePage}
                  target="_blank"
                  rel="noreferrer"
                  className="text-xs text-brand-200 underline decoration-dashed underline-offset-4"
                >
                  {comment.homePage}
                </a>
              )}
            </div>
            <p className="text-xs text-slate-400">{formatDate(comment.createdAt)}</p>
          </div>
        </div>
      </div>

      <p className="whitespace-pre-line text-sm leading-relaxed text-slate-100">{comment.text}</p>

      <div className="flex flex-wrap items-center gap-2 text-xs">
        {comment.attachments.map((attachment) => {
          const info = getAttachmentInfo(attachment.contentType)
          return (
            <a
              key={attachment.fileId}
              href={attachment.url}
              target="_blank"
              rel="noreferrer"
              className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-3 py-1.5 text-slate-100 transition hover:border-brand-400/50 hover:bg-white/10 hover:text-white"
            >
              <span>{info.icon}</span>
              <span className="font-medium">{info.label}</span>
            </a>
          )
        })}

        {showReplyAction && onReply && (
          <button
            type="button"
            onClick={() => onReply({ id: comment.id, userName: comment.userName, text: shortText })}
            className="inline-flex items-center gap-2 rounded-full border border-transparent px-2 py-1.5 text-slate-200 transition hover:border-brand-400/40 hover:text-white"
          >
            <span className="text-base leading-none">↩︎</span>
            <span className="font-medium">Ответить</span>
          </button>
        )}
      </div>
    </div>
  )
}

export default CommentCard

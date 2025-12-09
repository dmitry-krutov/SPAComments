import { type JSX, useEffect, useMemo, useRef, useState } from 'react'
import { useAppDispatch, useAppSelector } from './app/hooks'
import { prependComment, fetchLatestComments } from './features/comments/commentFeedSlice'
import type { CommentDto } from './features/comments/types'
import CommentForm from './components/CommentForm'

const formatDate = (value: string) =>
  new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value))

type CommentNode = CommentDto & { children: CommentNode[] }

const buildCommentTree = (items: CommentDto[]): CommentNode[] => {
  const nodes = new Map<string, CommentNode>()
  const roots: CommentNode[] = []

  items.forEach((item) => {
    nodes.set(item.id, { ...item, children: [] })
  })

  items.forEach((item) => {
    const node = nodes.get(item.id)
    if (!node) return

    if (item.parentId && nodes.has(item.parentId)) {
      nodes.get(item.parentId)?.children.push(node)
    } else {
      roots.push(node)
    }
  })

  return roots
}

function App() {
  const dispatch = useAppDispatch()
  const commentFeed = useAppSelector((state) => state.commentFeed)
  const [replyTarget, setReplyTarget] = useState<{ id: string; userName: string; text: string } | null>(null)
  const [replyOrigin, setReplyOrigin] = useState<{ commentId: string; top: number } | null>(null)
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set())
  const [pendingScrollToCommentId, setPendingScrollToCommentId] = useState<string | null>(null)
  const formRef = useRef<HTMLDivElement | null>(null)
  const commentRefs = useRef<Record<string, HTMLDivElement | null>>({})

  useEffect(() => {
    dispatch(fetchLatestComments())
  }, [dispatch])

  const commentTree = useMemo(() => buildCommentTree(commentFeed.items), [commentFeed.items])
  const totalPages = Math.max(1, Math.ceil(commentFeed.totalCount / commentFeed.pageSize))
  const parentLookup = useMemo(() => {
    const map = new Map<string, string | null>()
    commentFeed.items.forEach((item) => {
      map.set(item.id, item.parentId ?? null)
    })
    return map
  }, [commentFeed.items])

  const goToPage = (page: number) => {
    if (page < 1 || page > totalPages) return
    dispatch(fetchLatestComments({ page }))
    setReplyTarget(null)
    setReplyOrigin(null)
    setExpandedIds(new Set())
    setPendingScrollToCommentId(null)
  }

  const handleCommentAdded = (comment: CommentDto) => {
    dispatch(prependComment(comment))
    if (comment.parentId) {
      expandPathToComment(comment)
      setPendingScrollToCommentId(comment.id)
    } else {
      setPendingScrollToCommentId(null)
    }
    setReplyTarget(null)
    setReplyOrigin(null)
  }

  const scrollToForm = () => {
    if (formRef.current) {
      formRef.current.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }
  }

  const registerCommentRef = (id: string, el: HTMLDivElement | null) => {
    if (el) {
      commentRefs.current[id] = el
    } else {
      delete commentRefs.current[id]
    }
  }

  const getCommentTop = (id: string) => {
    const element = commentRefs.current[id]
    if (!element) return null
    const rect = element.getBoundingClientRect()
    return rect.top + window.scrollY
  }

  const scrollBackToOrigin = () => {
    if (!replyOrigin) return
    window.scrollTo({ top: replyOrigin.top, behavior: 'smooth' })
  }

  const expandPathToComment = (comment: CommentDto) => {
    if (!comment.parentId) return
    setExpandedIds((prev) => {
      const next = new Set(prev)
      let current: string | null = comment.parentId ?? null
      while (current) {
        next.add(current)
        current = parentLookup.get(current) ?? null
      }
      return next
    })
  }

  useEffect(() => {
    if (!pendingScrollToCommentId) return

    let frame: number

    const tryScroll = () => {
      const el = commentRefs.current[pendingScrollToCommentId]
      if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'center' })
        setPendingScrollToCommentId(null)
        return
      }
      frame = window.requestAnimationFrame(tryScroll)
    }

    tryScroll()

    return () => {
      if (frame) window.cancelAnimationFrame(frame)
    }
  }, [pendingScrollToCommentId, commentFeed.items])

  const toggleExpand = (id: string) => {
    setExpandedIds((prev) => {
      const next = new Set(prev)
      if (next.has(id)) {
        next.delete(id)
      } else {
        next.add(id)
      }
      return next
    })
  }

  const renderComment = (node: CommentNode, depth = 0): JSX.Element => (
    <CommentNode
      key={node.id}
      node={node}
      depth={depth}
      registerRef={registerCommentRef}
      expanded={expandedIds.has(node.id)}
      onToggleExpand={() => toggleExpand(node.id)}
      renderChild={renderComment}
      onReply={(target) => {
        const top = getCommentTop(node.id) ?? window.scrollY
        setReplyOrigin({ commentId: node.id, top })
        setReplyTarget(target)
        scrollToForm()
      }}
    />
  )

  return (
    <div className="min-h-screen bg-slate-900/90 text-slate-50">
      <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-12">
        <header className="flex flex-col gap-2 rounded-2xl bg-white/5 p-6 shadow-soft backdrop-blur">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-sm uppercase tracking-[0.28em] text-slate-300">SPA Comments</p>
              <h1 className="text-3xl font-semibold text-white">Страница комментариев</h1>
            </div>
          </div>
        </header>

        <section className="flex flex-col gap-4 rounded-2xl border border-white/10 bg-white/5 p-6 shadow-soft backdrop-blur">
          <div ref={formRef} className="space-y-3">
            {replyTarget && (
              <div className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-brand-500/30 bg-brand-500/10 px-4 py-3 text-sm text-brand-50">
                <div className="space-y-1">
                  <p className="font-semibold">Ответ на комментарий пользователя {replyTarget.userName}</p>
                  <p className="text-brand-100/80">“{replyTarget.text.length > 120 ? `${replyTarget.text.slice(0, 117)}…` : replyTarget.text}”</p>
                </div>
                <button
                  type="button"
                  onClick={() => {
                    scrollBackToOrigin()
                    setReplyTarget(null)
                    setReplyOrigin(null)
                  }}
                  className="rounded-full border border-white/20 px-3 py-2 text-xs font-medium text-white transition hover:border-white/40"
                >
                  Отменить
                </button>
              </div>
            )}

            <CommentForm
              heading="Новый комментарий"
              parentId={replyTarget?.id ?? null}
              onSubmitted={handleCommentAdded}
              onCancel={
                replyTarget
                  ? () => {
                      scrollBackToOrigin()
                      setReplyTarget(null)
                      setReplyOrigin(null)
                    }
                  : undefined
              }
            />
          </div>

          <div className="rounded-2xl border border-white/10 bg-slate-950/40 p-4">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <h2 className="text-2xl font-semibold text-white">Лента комментариев</h2>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => goToPage(commentFeed.page - 1)}
                  disabled={commentFeed.page <= 1 || commentFeed.status === 'loading'}
                  className="rounded-full border border-white/10 bg-white/10 px-3 py-2 text-sm text-white transition hover:bg-white/20 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  Назад
                </button>
                <div className="rounded-full bg-white/10 px-4 py-2 text-sm text-slate-200">
                  Стр. {commentFeed.page} / {totalPages}
                </div>
                <button
                  type="button"
                  onClick={() => goToPage(commentFeed.page + 1)}
                  disabled={commentFeed.page >= totalPages || commentFeed.status === 'loading'}
                  className="rounded-full border border-white/10 bg-white/10 px-3 py-2 text-sm text-white transition hover:bg-white/20 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  Вперёд
                </button>
              </div>
            </div>

            <div className="mt-6 space-y-4">
              {commentFeed.status === 'loading' && (
                <div className="rounded-xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-slate-300">
                  Обновляем ленту…
                </div>
              )}

              {commentFeed.status === 'failed' && commentFeed.error && (
                <div className="rounded-xl border border-rose-400/30 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">
                  {commentFeed.error}
                </div>
              )}

              {commentFeed.status === 'succeeded' && commentFeed.items.length === 0 && (
                <div className="rounded-xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-slate-300">
                  Пока нет комментариев.
                </div>
              )}

              {commentTree.map((node) => (
                <div key={node.id} className="rounded-2xl border border-white/10 bg-white/5 p-4 shadow-sm">
                  {renderComment(node)}
                </div>
              ))}
            </div>
          </div>
        </section>
      </div>
    </div>
  )
}

interface CommentNodeProps {
  node: CommentNode
  depth: number
  registerRef: (id: string, el: HTMLDivElement | null) => void
  onReply: (target: { id: string; userName: string; text: string }) => void
  expanded: boolean
  onToggleExpand: () => void
  renderChild: (node: CommentNode, depth: number) => JSX.Element
}

const getAttachmentInfo = (contentType?: string) => {
  if (contentType?.startsWith('image/')) return { icon: '📎', label: 'Изображение' }
  if (contentType?.startsWith('text/')) return { icon: '📄', label: 'Текстовый файл' }
  return { icon: '📁', label: 'Файл' }
}

const declension = (count: number) => {
  const mod10 = count % 10
  const mod100 = count % 100

  if (mod10 === 1 && mod100 !== 11) return 'ответ'
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return 'ответа'
  return 'ответов'
}

const countDescendants = (node: CommentNode): number =>
  node.children.reduce((acc, child) => acc + 1 + countDescendants(child), 0)

const CommentNode = ({ node, depth, registerRef, onReply, expanded, onToggleExpand, renderChild }: CommentNodeProps) => {
  const shortText = node.text.trim()
  const totalReplies = useMemo(() => countDescendants(node), [node])

  return (
    <div ref={(el) => registerRef(node.id, el)} className="space-y-3" style={{ marginLeft: depth * 18 }}>
      <div className="flex flex-wrap items-start gap-3">
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white/10 text-sm font-semibold text-white">
            {node.userName.charAt(0).toUpperCase()}
          </div>
          <div className="space-y-1">
            <div className="flex flex-wrap items-center gap-2">
              <p className="text-base font-semibold text-white">{node.userName}</p>
              {node.email && <p className="text-xs text-slate-400">{node.email}</p>}
              {node.homePage && (
                <a
                  href={node.homePage}
                  target="_blank"
                  rel="noreferrer"
                  className="text-xs text-brand-200 underline decoration-dashed underline-offset-4"
                >
                  {node.homePage}
                </a>
              )}
            </div>
            <p className="text-xs text-slate-400">{formatDate(node.createdAt)}</p>
          </div>
        </div>
      </div>

      <p className="whitespace-pre-line text-sm leading-relaxed text-slate-100">{node.text}</p>

      <div className="flex flex-wrap items-center gap-2 text-xs">
        {node.attachments.map((attachment) => {
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

        <button
          type="button"
          onClick={() => onReply({ id: node.id, userName: node.userName, text: shortText })}
          className="inline-flex items-center gap-2 rounded-full border border-transparent px-2 py-1.5 text-slate-200 transition hover:border-brand-400/40 hover:text-white"
        >
          <span className="text-base leading-none">↩︎</span>
          <span className="font-medium">Ответить</span>
        </button>
      </div>

      {totalReplies > 0 && (
        <button
          type="button"
          onClick={onToggleExpand}
          className="inline-flex items-center gap-2 rounded-full border border-white/5 bg-white/0 px-2 py-1 text-xs font-medium text-slate-200 transition hover:border-white/15 hover:bg-white/5 hover:text-white"
          aria-label={expanded ? 'Свернуть ветку' : 'Развернуть ветку'}
        >
          <span className="text-[10px] text-slate-400">{expanded ? '▼' : '►'}</span>
          <span>({totalReplies} {declension(totalReplies)})</span>
        </button>
      )}

      {expanded && node.children.length > 0 && (
        <div className="space-y-3 border-l border-white/10 pl-4">
          {node.children.map((child) => (
            <div key={child.id}>{renderChild(child, depth + 1)}</div>
          ))}
        </div>
      )}
    </div>
  )
}

export default App

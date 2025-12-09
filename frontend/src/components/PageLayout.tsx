import type { ReactNode } from 'react'
import { Link, useLocation } from 'react-router-dom'

interface PageLayoutProps {
  title: string
  subtitle?: string
  children: ReactNode
  actions?: ReactNode
}

export function PageLayout({ title, subtitle, children, actions }: PageLayoutProps) {
  const location = useLocation()
  const isHome = location.pathname === '/'

  return (
    <div className="min-h-screen bg-slate-900/90 text-slate-50">
      <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-12">
        <header className="flex flex-col gap-2 rounded-2xl bg-white/5 p-6 shadow-soft backdrop-blur">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div className="space-y-1">
              <p className="text-sm uppercase tracking-[0.28em] text-slate-300">SPA Comments</p>
              <h1 className="text-3xl font-semibold text-white">{title}</h1>
              {subtitle && <p className="text-sm text-slate-300">{subtitle}</p>}
            </div>
            <div className="flex flex-wrap items-center gap-2">
              {!isHome && (
                <Link
                  to="/"
                  className="rounded-full border border-white/10 bg-white/10 px-4 py-2 text-sm text-white transition hover:bg-white/20"
                >
                  На главную
                </Link>
              )}
              {actions}
            </div>
          </div>
        </header>

        {children}
      </div>
    </div>
  )
}

export default PageLayout

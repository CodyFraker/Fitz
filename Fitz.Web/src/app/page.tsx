import Link from 'next/link'
import { Sparkles, LogIn } from 'lucide-react'

export default function HomePage() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-4">
      <div className="z-10 max-w-5xl w-full items-center justify-center font-mono text-sm">
        <div className="flex items-center justify-center gap-2 mb-8">
          <Sparkles className="h-8 w-8" />
          <h1 className="text-4xl font-bold text-center">Fitz Bot</h1>
        </div>
        <div className="flex flex-col gap-4 items-center">
          <Link
            href="/login"
            className="px-6 py-3 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors flex items-center gap-2"
          >
            <LogIn className="h-5 w-5" />
            Login
          </Link>
        </div>
      </div>
    </main>
  )
}

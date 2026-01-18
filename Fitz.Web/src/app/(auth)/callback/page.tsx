'use client'

import { useEffect, useState, Suspense, useRef } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { apiClient } from '@/lib/api/client'
import { useAuth } from '@/contexts/AuthContext'
import { Loader2, AlertCircle, LogIn } from 'lucide-react'

const DISCORD_REDIRECT_URI = process.env.NEXT_PUBLIC_DISCORD_REDIRECT_URI || 'http://localhost:5173/callback'

function CallbackContent() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { login } = useAuth()
  const [error, setError] = useState<string | null>(null)
  const hasProcessed = useRef(false)

  useEffect(() => {
    const code = searchParams.get('code')
    if (!code) {
      setError('No authorization code received')
      return
    }

    if (hasProcessed.current) {
      return
    }

    hasProcessed.current = true

    const exchangeToken = async () => {
      try {
        const response = await apiClient.post('/api/auth/exchange-token', {
          code,
          redirectUri: DISCORD_REDIRECT_URI,
        })

        if (response.success && response.data) {
          await login(response.data.accessToken)
          router.push('/account')
        } else {
          const errorMessage = response.message || 'Failed to authenticate'
          if (process.env.NODE_ENV === 'development') {
            console.error('Token exchange failed:', response)
          }
          setError(errorMessage)
        }
      } catch (err) {
        let errorMessage = 'An error occurred during authentication'
        
        if (err instanceof Error) {
          errorMessage = err.message || errorMessage
        }
        
        if (process.env.NODE_ENV === 'development') {
          console.error('Token exchange error:', err)
        }
        
        setError(errorMessage)
      }
    }

    exchangeToken()
  }, [searchParams, login, router])

  if (error) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <div className="text-center">
          <AlertCircle className="h-8 w-8 text-destructive mx-auto mb-4" />
          <h1 className="text-2xl font-bold mb-4">Authentication Error</h1>
          <p className="text-muted-foreground mb-4">{error}</p>
          <a href="/login" className="text-primary hover:underline flex items-center justify-center gap-2">
            <LogIn className="h-4 w-4" />
            Try again
          </a>
        </div>
      </main>
    )
  }

  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-4">
      <div className="text-center">
        <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
        <p>Completing authentication...</p>
      </div>
    </main>
  )
}

export default function CallbackPage() {
  return (
    <Suspense fallback={
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <div className="text-center">
          <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
          <p>Loading...</p>
        </div>
      </main>
    }>
      <CallbackContent />
    </Suspense>
  )
}

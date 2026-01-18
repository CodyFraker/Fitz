'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/contexts/AuthContext'
import { MessageCircle } from 'lucide-react'

const DISCORD_CLIENT_ID = process.env.NEXT_PUBLIC_DISCORD_CLIENT_ID || ''
const DISCORD_REDIRECT_URI = process.env.NEXT_PUBLIC_DISCORD_REDIRECT_URI || 'http://localhost:5173/callback'

export default function LoginPage() {
  const router = useRouter()
  const { isAuthenticated } = useAuth()

  useEffect(() => {
    if (isAuthenticated) {
      router.push('/account')
    }
  }, [isAuthenticated, router])

  const handleLogin = () => {
    if (!DISCORD_CLIENT_ID) {
      alert('Discord Client ID is not configured. Please set NEXT_PUBLIC_DISCORD_CLIENT_ID in your .env.local file.')
      return
    }
    
    if (process.env.NODE_ENV === 'development') {
      console.log('Discord OAuth Configuration:', {
        clientId: DISCORD_CLIENT_ID ? `${DISCORD_CLIENT_ID.substring(0, 10)}...` : 'missing',
        redirectUri: DISCORD_REDIRECT_URI,
      })
    }
    
    const discordAuthUrl = `https://discord.com/api/oauth2/authorize?client_id=${DISCORD_CLIENT_ID}&redirect_uri=${encodeURIComponent(DISCORD_REDIRECT_URI)}&response_type=code&scope=identify`
    window.location.href = discordAuthUrl
  }

  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-4">
      <div className="z-10 max-w-md w-full">
        <div className="flex items-center justify-center gap-2 mb-8">
          <MessageCircle className="h-6 w-6" />
          <h1 className="text-3xl font-bold text-center">Login with Discord</h1>
        </div>
        <button
          onClick={handleLogin}
          className="w-full px-6 py-3 bg-[#5865F2] text-white rounded-lg hover:bg-[#4752C4] transition-colors font-medium flex items-center justify-center gap-2"
        >
          <MessageCircle className="h-5 w-5" />
          Continue with Discord
        </button>
      </div>
    </main>
  )
}

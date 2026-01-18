'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import { AccountResponse } from '@/types/api'
import { useState } from 'react'
import { User, Loader2, AlertCircle, Coins, Shield } from 'lucide-react'
import { LotterySettingsCard } from '@/components/account/LotterySettingsCard'

export default function AccountPage() {
  const router = useRouter()
  const { user, isAuthenticated, isLoading } = useAuth()
  const [account, setAccount] = useState<AccountResponse | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/login')
    }
  }, [isLoading, isAuthenticated, router])

  const fetchAccount = async () => {
    if (!user) return
    
    try {
      const response = await apiClient.get<AccountResponse>(`/api/account/${user.id.toString()}`)
      if (response.success && response.data) {
        setAccount(response.data)
      }
    } catch (error) {
      console.error('Failed to fetch account:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (user) {
      fetchAccount()
    }
  }, [user])

  if (isLoading || loading) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <Loader2 className="h-6 w-6 animate-spin" />
        <p className="mt-2">Loading...</p>
      </main>
    )
  }

  if (!account) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <AlertCircle className="h-6 w-6 text-destructive" />
        <p className="mt-2">Failed to load account</p>
      </main>
    )
  }

  return (
    <main className="flex min-h-screen flex-col p-4 max-w-4xl mx-auto">
      <div className="flex items-center gap-2 mb-6">
        <User className="h-6 w-6" />
        <h1 className="text-3xl font-bold">Account</h1>
      </div>
      <div className="space-y-4">
        <div className="p-4 border rounded-lg">
          <div className="flex items-center gap-2 mb-2">
            <User className="h-5 w-5" />
            <h2 className="text-xl font-semibold">Profile</h2>
          </div>
          <p>Username: {account.username || 'N/A'}</p>
          <p>ID: {account.id.toString()}</p>
        </div>
        <div className="p-4 border rounded-lg">
          <div className="flex items-center gap-2 mb-2">
            <Coins className="h-5 w-5" />
            <h2 className="text-xl font-semibold">Balance</h2>
          </div>
          <p>Beer: {account.beer}</p>
          <p>Lifetime Beer: {account.lifetimeBeer}</p>
          <div className="flex items-center gap-2">
            <Shield className="h-4 w-4" />
            <p>Safe Balance: {account.safeBalance}</p>
          </div>
        </div>
        <LotterySettingsCard account={account} onUpdate={fetchAccount} />
      </div>
    </main>
  )
}

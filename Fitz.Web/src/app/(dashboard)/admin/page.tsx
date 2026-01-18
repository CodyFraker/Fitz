'use client'

import { useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { useRouter } from 'next/navigation'
import { useEffect } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { AdminAccountManagement } from '@/components/admin/AdminAccountManagement'
import { AdminPollModeration } from '@/components/admin/AdminPollModeration'
import { AdminLotteryManagement } from '@/components/admin/AdminLotteryManagement'
import { AdminBotPuppeting } from '@/components/admin/AdminBotPuppeting'
import { AdminFavorabilityManagement } from '@/components/admin/AdminFavorabilityManagement'
import { AdminFavorabilitySettings } from '@/components/admin/AdminFavorabilitySettings'
import { Shield, Loader2, User, BarChart3, Ticket, MessageSquare, User as UserIcon, Heart } from 'lucide-react'

export default function AdminPage() {
  const { isAuthenticated, isAdmin, isLoading } = useAuth()
  const router = useRouter()
  const [activeTab, setActiveTab] = useState<'accounts' | 'polls' | 'lottery' | 'bot' | 'favorability'>('accounts')

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/login')
    } else if (!isLoading && !isAdmin) {
      router.push('/')
    }
  }, [isLoading, isAuthenticated, isAdmin, router])

  if (isLoading) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <Loader2 className="h-6 w-6 animate-spin" />
        <p className="mt-2">Loading...</p>
      </main>
    )
  }

  if (!isAdmin) {
    return null
  }

  return (
    <main className="flex min-h-screen flex-col p-4 max-w-7xl mx-auto">
      <div className="mb-6">
        <div className="flex items-center gap-2 mb-2">
          <Shield className="h-6 w-6" />
          <h1 className="text-3xl font-bold">Admin Dashboard</h1>
        </div>
        <p className="text-muted-foreground">Manage accounts, polls, lotteries, and bot actions</p>
      </div>

      <div className="flex gap-2 mb-6 border-b">
        <Button
          variant={activeTab === 'accounts' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('accounts')}
          className="flex items-center gap-2"
        >
          <UserIcon className="h-4 w-4" />
          Account Management
        </Button>
        <Button
          variant={activeTab === 'polls' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('polls')}
          className="flex items-center gap-2"
        >
          <BarChart3 className="h-4 w-4" />
          Poll Moderation
        </Button>
        <Button
          variant={activeTab === 'lottery' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('lottery')}
          className="flex items-center gap-2"
        >
          <Ticket className="h-4 w-4" />
          Lottery Management
        </Button>
        <Button
          variant={activeTab === 'bot' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('bot')}
          className="flex items-center gap-2"
        >
          <MessageSquare className="h-4 w-4" />
          Bot Puppeting
        </Button>
        <Button
          variant={activeTab === 'favorability' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('favorability')}
          className="flex items-center gap-2"
        >
          <Heart className="h-4 w-4" />
          Favorability
        </Button>
      </div>

      <div className="mt-4">
        {activeTab === 'accounts' && <AdminAccountManagement />}
        {activeTab === 'polls' && <AdminPollModeration />}
        {activeTab === 'lottery' && <AdminLotteryManagement />}
        {activeTab === 'bot' && <AdminBotPuppeting />}
        {activeTab === 'favorability' && (
          <div className="space-y-4">
            <AdminFavorabilityManagement />
            <AdminFavorabilitySettings />
          </div>
        )}
      </div>
    </main>
  )
}

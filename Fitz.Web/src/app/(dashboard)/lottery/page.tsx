'use client'

import { useEffect, useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import { CurrentLotteryResponse, AccountResponse } from '@/types/api'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { LotteryStatisticsChart } from '@/components/lottery/LotteryStatisticsChart'
import { LotteryHistoryTable } from '@/components/lottery/LotteryHistoryTable'
import { LotterySettingsCard } from '@/components/account/LotterySettingsCard'
import { Ticket, Loader2, AlertCircle, Trophy, Users, Beer } from 'lucide-react'

export default function LotteryPage() {
  const { user } = useAuth()
  const [lottery, setLottery] = useState<CurrentLotteryResponse | null>(null)
  const [account, setAccount] = useState<AccountResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [accountLoading, setAccountLoading] = useState(false)

  const fetchData = async () => {
    setLoading(true)
    try {
      const response = await apiClient.getCurrentLottery()
      if (response.success && response.data) {
        setLottery(response.data)
      }
    } catch (error) {
      console.error('Failed to fetch lottery:', error)
    } finally {
      setLoading(false)
    }
  }

  const fetchAccount = async () => {
    if (!user) return
    
    setAccountLoading(true)
    try {
      const response = await apiClient.get<AccountResponse>(`/api/account/${user.id.toString()}`)
      if (response.success && response.data) {
        setAccount(response.data)
      }
    } catch (error) {
      console.error('Failed to fetch account:', error)
    } finally {
      setAccountLoading(false)
    }
  }

  useEffect(() => {
    fetchData()
    fetchAccount()
  }, [user])

  if (loading) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <Loader2 className="h-6 w-6 animate-spin" />
        <p className="mt-2">Loading...</p>
      </main>
    )
  }

  if (!lottery) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <AlertCircle className="h-6 w-6 text-destructive" />
        <p className="mt-2">No active lottery</p>
      </main>
    )
  }

  const formatOdds = (odds: number) => {
    if (odds === 0) return 'N/A'
    const oneIn = Math.round(1 / (odds / 100))
    return `1 in ${oneIn.toLocaleString()} (${odds.toFixed(4)}%)`
  }

  return (
    <main className="flex min-h-screen flex-col p-4 max-w-6xl mx-auto">
      <div className="mb-6">
        <div className="flex items-center gap-2">
          <Ticket className="h-6 w-6" />
          <h1 className="text-3xl font-bold">Lottery</h1>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Beer className="h-5 w-5" />
              <CardTitle>Current Prize Pool</CardTitle>
            </div>
            <CardDescription>Total beer in the pool</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">🍺 {lottery.pool ?? 0}</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Trophy className="h-5 w-5" />
              <CardTitle>Odds of Winning</CardTitle>
            </div>
            <CardDescription>Your chance to win</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{formatOdds(lottery.odds)}</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              <CardTitle>Total Participants</CardTitle>
            </div>
            <CardDescription>Number of players</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">{lottery.totalParticipants}</p>
          </CardContent>
        </Card>
      </div>

      <div className="space-y-6">
        {account && (
          <LotterySettingsCard account={account} onUpdate={fetchAccount} />
        )}
        <LotteryStatisticsChart />
        <LotteryHistoryTable />
      </div>
    </main>
  )
}

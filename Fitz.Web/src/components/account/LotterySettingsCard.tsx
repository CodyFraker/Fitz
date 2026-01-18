'use client'

import { useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import { AccountResponse } from '@/types/api'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Input } from '@/components/ui/input'
import { Switch } from '@/components/ui/switch'
import { Ticket, Shield, Loader2, AlertCircle, CheckCircle2 } from 'lucide-react'

interface LotterySettingsCardProps {
  account: AccountResponse
  onUpdate?: () => void
}

export function LotterySettingsCard({ account, onUpdate }: LotterySettingsCardProps) {
  const { user } = useAuth()
  const [subscribeToLottery, setSubscribeToLottery] = useState(account.subscribeToLottery)
  const [safeBalance, setSafeBalance] = useState(account.safeBalance.toString())
  const [subscribeTickets, setSubscribeTickets] = useState(account.subscribeTickets.toString())
  
  const [loadingSubscribe, setLoadingSubscribe] = useState(false)
  const [loadingSafeBalance, setLoadingSafeBalance] = useState(false)
  const [loadingTickets, setLoadingTickets] = useState(false)
  
  const [errorSubscribe, setErrorSubscribe] = useState<string | null>(null)
  const [errorSafeBalance, setErrorSafeBalance] = useState<string | null>(null)
  const [errorTickets, setErrorTickets] = useState<string | null>(null)
  
  const [successSubscribe, setSuccessSubscribe] = useState(false)
  const [successSafeBalance, setSuccessSafeBalance] = useState(false)
  const [successTickets, setSuccessTickets] = useState(false)

  const handleSubscribeChange = async (checked: boolean) => {
    if (!user) return

    setLoadingSubscribe(true)
    setErrorSubscribe(null)
    setSuccessSubscribe(false)

    try {
      const response = await apiClient.setLotterySubscribe(user.id, checked)
      
      if (response.success) {
        setSubscribeToLottery(checked)
        setSuccessSubscribe(true)
        setTimeout(() => setSuccessSubscribe(false), 2000)
        onUpdate?.()
      } else {
        setErrorSubscribe(response.message || 'Failed to update subscription')
        setSubscribeToLottery(!checked)
      }
    } catch (err) {
      setErrorSubscribe(err instanceof Error ? err.message : 'Failed to update subscription')
      setSubscribeToLottery(!checked)
    } finally {
      setLoadingSubscribe(false)
    }
  }

  const handleSafeBalanceChange = async () => {
    if (!user) return

    const value = parseInt(safeBalance, 10)
    if (isNaN(value) || value < 0) {
      setErrorSafeBalance('Safe balance must be a non-negative number')
      return
    }

    setLoadingSafeBalance(true)
    setErrorSafeBalance(null)
    setSuccessSafeBalance(false)

    try {
      const response = await apiClient.setSafeBalance(user.id, value)
      
      if (response.success) {
        setSuccessSafeBalance(true)
        setTimeout(() => setSuccessSafeBalance(false), 2000)
        onUpdate?.()
      } else {
        setErrorSafeBalance(response.message || 'Failed to update safe balance')
        setSafeBalance(account.safeBalance.toString())
      }
    } catch (err) {
      setErrorSafeBalance(err instanceof Error ? err.message : 'Failed to update safe balance')
      setSafeBalance(account.safeBalance.toString())
    } finally {
      setLoadingSafeBalance(false)
    }
  }

  const handleTicketsChange = async () => {
    if (!user) return

    const value = parseInt(subscribeTickets, 10)
    if (isNaN(value) || value < 0) {
      setErrorTickets('Ticket amount must be a non-negative number')
      return
    }

    setLoadingTickets(true)
    setErrorTickets(null)
    setSuccessTickets(false)

    try {
      const response = await apiClient.setTicketAmount(user.id, value)
      
      if (response.success) {
        setSuccessTickets(true)
        setTimeout(() => setSuccessTickets(false), 2000)
        onUpdate?.()
      } else {
        setErrorTickets(response.message || 'Failed to update ticket amount')
        setSubscribeTickets(account.subscribeTickets.toString())
      }
    } catch (err) {
      setErrorTickets(err instanceof Error ? err.message : 'Failed to update ticket amount')
      setSubscribeTickets(account.subscribeTickets.toString())
    } finally {
      setLoadingTickets(false)
    }
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <Ticket className="h-5 w-5" />
          <CardTitle>Lottery Subscription</CardTitle>
        </div>
        <CardDescription>
          Configure your automatic lottery subscription settings
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label htmlFor="subscribe" className="flex items-center gap-2">
                <Ticket className="h-4 w-4" />
                Auto-enroll in Lottery
              </Label>
              <p className="text-sm text-muted-foreground">
                Automatically buy tickets for each lottery drawing
              </p>
            </div>
            <div className="flex items-center gap-2">
              {loadingSubscribe && <Loader2 className="h-4 w-4 animate-spin" />}
              {successSubscribe && <CheckCircle2 className="h-4 w-4 text-green-500" />}
              <Switch
                id="subscribe"
                checked={subscribeToLottery}
                onCheckedChange={handleSubscribeChange}
                disabled={loadingSubscribe}
              />
            </div>
          </div>
          {errorSubscribe && (
            <div className="flex items-center gap-2 text-sm text-destructive">
              <AlertCircle className="h-4 w-4" />
              <p>{errorSubscribe}</p>
            </div>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="safe-balance" className="flex items-center gap-2">
            <Shield className="h-4 w-4" />
            Minimal Balance Required
          </Label>
          <p className="text-sm text-muted-foreground">
            The minimum balance you want to keep before auto-buying stops
          </p>
          <div className="flex items-center gap-2">
            <Input
              id="safe-balance"
              type="number"
              value={safeBalance}
              onChange={(e) => setSafeBalance(e.target.value)}
              onBlur={handleSafeBalanceChange}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.currentTarget.blur()
                }
              }}
              min="0"
              disabled={loadingSafeBalance}
              className="max-w-[200px]"
            />
            {loadingSafeBalance && <Loader2 className="h-4 w-4 animate-spin" />}
            {successSafeBalance && <CheckCircle2 className="h-4 w-4 text-green-500" />}
          </div>
          {errorSafeBalance && (
            <div className="flex items-center gap-2 text-sm text-destructive">
              <AlertCircle className="h-4 w-4" />
              <p>{errorSafeBalance}</p>
            </div>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="ticket-amount" className="flex items-center gap-2">
            <Ticket className="h-4 w-4" />
            Tickets to Buy Automatically
          </Label>
          <p className="text-sm text-muted-foreground">
            Number of tickets to purchase each lottery when auto-enroll is active
          </p>
          <div className="flex items-center gap-2">
            <Input
              id="ticket-amount"
              type="number"
              value={subscribeTickets}
              onChange={(e) => setSubscribeTickets(e.target.value)}
              onBlur={handleTicketsChange}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.currentTarget.blur()
                }
              }}
              min="0"
              disabled={loadingTickets}
              className="max-w-[200px]"
            />
            {loadingTickets && <Loader2 className="h-4 w-4 animate-spin" />}
            {successTickets && <CheckCircle2 className="h-4 w-4 text-green-500" />}
          </div>
          {errorTickets && (
            <div className="flex items-center gap-2 text-sm text-destructive">
              <AlertCircle className="h-4 w-4" />
              <p>{errorTickets}</p>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  )
}

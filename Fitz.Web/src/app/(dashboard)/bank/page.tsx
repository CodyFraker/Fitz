'use client'

import { useEffect, useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import { BalanceResponse, TransactionResponse, TransactionsResponse } from '@/types/api'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { TransferBeerDialog } from '@/components/bank/TransferBeerDialog'
import { BeerOverTimeChart } from '@/components/bank/BeerOverTimeChart'
import { BalancesTable } from '@/components/bank/BalancesTable'
import { TransactionsTable } from '@/components/bank/TransactionsTable'
import { Refrigerator, Loader2, AlertCircle, Send, Beer, TrendingUp } from 'lucide-react'

export default function BankPage() {
  const { user } = useAuth()
  const [balance, setBalance] = useState<BalanceResponse | null>(null)
  const [transactions, setTransactions] = useState<TransactionResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [transferDialogOpen, setTransferDialogOpen] = useState(false)

  const fetchData = async () => {
    if (!user) return

    setLoading(true)
    try {
      const [balanceResponse, transactionsResponse] = await Promise.all([
        apiClient.get<BalanceResponse>(`/api/bank/balance/${user.id.toString()}`),
        apiClient.get<TransactionsResponse>(`/api/bank/transactions/${user.id.toString()}`),
      ])

      if (balanceResponse.success && balanceResponse.data) {
        setBalance(balanceResponse.data)
      }

      if (transactionsResponse.success && transactionsResponse.data) {
        setTransactions(transactionsResponse.data.transactions || [])
      }
    } catch (error) {
      console.error('Failed to fetch data:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchData()
  }, [user])

  if (loading) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <Loader2 className="h-6 w-6 animate-spin" />
        <p className="mt-2">Loading...</p>
      </main>
    )
  }

  if (!balance) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <AlertCircle className="h-6 w-6 text-destructive" />
        <p className="mt-2">Failed to load balance</p>
      </main>
    )
  }

  return (
    <main className="flex min-h-screen flex-col p-4 max-w-6xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-2">
          <Refrigerator className="h-6 w-6" />
          <h1 className="text-3xl font-bold">Fridge</h1>
        </div>
        <Button onClick={() => setTransferDialogOpen(true)} className="flex items-center gap-2">
          <Send className="h-5 w-5" />
          Transfer Beer
        </Button>
      </div>

      <div className="space-y-6">
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Beer className="h-5 w-5" />
              <CardTitle>Current Balance</CardTitle>
            </div>
            <CardDescription>Your current beer balance in the fridge</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              <p className="text-3xl font-bold">🍺 {balance.beer}</p>
              <p className="text-muted-foreground">Lifetime: {balance.lifetimeBeer}</p>
            </div>
          </CardContent>
        </Card>

        <BeerOverTimeChart transactions={transactions} currentBalance={balance.beer} />

        {user && <TransactionsTable userId={user.id} />}

        <BalancesTable />
      </div>

      <TransferBeerDialog
        open={transferDialogOpen}
        onOpenChange={setTransferDialogOpen}
        onSuccess={fetchData}
        currentBalance={balance.beer}
      />
    </main>
  )
}

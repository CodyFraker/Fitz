'use client'

import { useEffect, useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import { TransactionResponse } from '@/types/api'
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

interface BeerOverTimeChartProps {
  transactions: TransactionResponse[]
  currentBalance: number
}

export function BeerOverTimeChart({ transactions, currentBalance }: BeerOverTimeChartProps) {
  const { user } = useAuth()
  const [chartData, setChartData] = useState<Array<{ date: string; balance: number }>>([])

  useEffect(() => {
    if (!user) {
      setChartData([])
      return
    }

    const userId = user.id
    const data: Array<{ date: string; balance: number }> = []

    if (!Array.isArray(transactions)) {
      setChartData([])
      return
    }

    const userTransactions = transactions.filter(
      (t) => t.sender === userId || t.recipient === userId
    )

    if (userTransactions.length === 0) {
      data.push({
        date: new Date().toLocaleDateString(),
        balance: currentBalance,
      })
      setChartData(data)
      return
    }

    const sortedTransactions = [...userTransactions].sort(
      (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
    )

    let runningBalance = currentBalance

    for (let i = sortedTransactions.length - 1; i >= 0; i--) {
      const transaction = sortedTransactions[i]
      if (transaction.recipient === userId) {
        runningBalance -= transaction.amount
      } else if (transaction.sender === userId) {
        runningBalance += transaction.amount
      }
    }

    sortedTransactions.forEach((transaction) => {
      if (transaction.recipient === userId) {
        runningBalance += transaction.amount
      } else if (transaction.sender === userId) {
        runningBalance -= transaction.amount
      }

      const date = new Date(transaction.timestamp)
      data.push({
        date: date.toLocaleDateString(),
        balance: runningBalance,
      })
    })

    data.push({
      date: new Date().toLocaleDateString(),
      balance: currentBalance,
    })

    setChartData(data)
  }, [transactions, currentBalance, user])

  if (chartData.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Beer Over Time</CardTitle>
          <CardDescription>Your beer balance history</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">No transaction data available</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Beer Over Time</CardTitle>
        <CardDescription>Your beer balance history</CardDescription>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis
              dataKey="date"
              tick={{ fontSize: 12 }}
              angle={-45}
              textAnchor="end"
              height={80}
            />
            <YAxis tick={{ fontSize: 12 }} />
            <Tooltip
              formatter={(value) => value !== undefined ? [`${value} 🍺`, 'Balance'] : ['', '']}
              labelStyle={{ color: '#000' }}
            />
            <Line
              type="monotone"
              dataKey="balance"
              stroke="hsl(var(--primary))"
              strokeWidth={2}
              dot={{ r: 4 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}

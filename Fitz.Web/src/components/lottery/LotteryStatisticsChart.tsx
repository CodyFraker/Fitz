'use client'

import { useEffect, useState } from 'react'
import { apiClient } from '@/lib/api/client'
import { LotteryStatisticsPoint } from '@/types/api'
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

export function LotteryStatisticsChart() {
  const [chartData, setChartData] = useState<Array<{ date: string; prizePool: number; totalTickets: number }>>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchStatistics = async () => {
      try {
        const response = await apiClient.getLotteryStatistics()
        if (response.success && response.data?.dataPoints) {
          const dataPoints: LotteryStatisticsPoint[] = response.data.dataPoints
          const formattedData = dataPoints.map((point) => ({
            date: new Date(point.date).toLocaleDateString('en-US', {
              year: 'numeric',
              month: 'short',
              day: 'numeric',
            }),
            prizePool: point.prizePool,
            totalTickets: point.totalTickets,
          }))
          setChartData(formattedData)
        }
      } catch (error) {
        console.error('Failed to fetch lottery statistics:', error)
      } finally {
        setLoading(false)
      }
    }

    fetchStatistics()
  }, [])

  if (loading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Lottery Statistics</CardTitle>
          <CardDescription>Prize pool and tickets over time</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Loading...</p>
        </CardContent>
      </Card>
    )
  }

  if (chartData.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Lottery Statistics</CardTitle>
          <CardDescription>Prize pool and tickets over time</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">No lottery data available</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Lottery Statistics</CardTitle>
        <CardDescription>Prize pool and tickets over time</CardDescription>
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
            <YAxis yAxisId="left" tick={{ fontSize: 12 }} />
            <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 12 }} />
            <Tooltip
              formatter={(value: number, name: string) => {
                if (name === 'prizePool') {
                  return [`${value} 🍺`, 'Prize Pool']
                }
                return [value, 'Total Tickets']
              }}
              labelStyle={{ color: '#000' }}
            />
            <Legend />
            <Line
              yAxisId="left"
              type="monotone"
              dataKey="prizePool"
              stroke="hsl(var(--primary))"
              strokeWidth={2}
              dot={{ r: 4 }}
              name="Prize Pool"
            />
            <Line
              yAxisId="right"
              type="monotone"
              dataKey="totalTickets"
              stroke="#8884d8"
              strokeWidth={2}
              dot={{ r: 4 }}
              name="Total Tickets"
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}

'use client'

import { useState, useEffect } from 'react'
import { apiClient } from '@/lib/api/client'
import { LotteryHistoryResponse } from '@/types/api'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Ticket, Loader2, AlertCircle, ChevronLeft, ChevronRight, Calendar, Beer, Trophy, Users, Hash } from 'lucide-react'

const ITEMS_PER_PAGE = 10

export function LotteryHistoryTable() {
  const [history, setHistory] = useState<LotteryHistoryResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [currentPage, setCurrentPage] = useState(0)
  const [error, setError] = useState<string | null>(null)

  const fetchHistory = async (skip: number) => {
    setLoading(true)
    setError(null)
    try {
      const response = await apiClient.getLotteryHistory(skip, ITEMS_PER_PAGE)

      if (response.success && response.data) {
        setHistory(response.data)
      } else {
        setError('Failed to load lottery history')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load lottery history')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchHistory(currentPage * ITEMS_PER_PAGE)
  }, [currentPage])

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }

  if (loading && !history) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Ticket className="h-5 w-5" />
            <CardTitle>Lottery History</CardTitle>
          </div>
          <CardDescription>Past lottery drawings</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2">
            <Loader2 className="h-4 w-4 animate-spin" />
            <p className="text-sm text-muted-foreground">Loading...</p>
          </div>
        </CardContent>
      </Card>
    )
  }

  if (error) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Ticket className="h-5 w-5" />
            <CardTitle>Lottery History</CardTitle>
          </div>
          <CardDescription>Past lottery drawings</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2 text-sm text-destructive">
            <AlertCircle className="h-4 w-4" />
            <p>{error}</p>
          </div>
        </CardContent>
      </Card>
    )
  }

  const totalPages = history ? Math.ceil(history.totalCount / ITEMS_PER_PAGE) : 0

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <Ticket className="h-5 w-5" />
          <CardTitle>Lottery History</CardTitle>
        </div>
        <CardDescription>
          Showing {history?.lotteries.length || 0} of {history?.totalCount || 0} lotteries
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="flex items-center gap-1">
                  <Hash className="h-4 w-4" />
                  Lottery ID
                </TableHead>
                <TableHead className="flex items-center gap-1">
                  <Calendar className="h-4 w-4" />
                  Start Date
                </TableHead>
                <TableHead className="flex items-center gap-1">
                  <Calendar className="h-4 w-4" />
                  End Date
                </TableHead>
                <TableHead className="flex items-center gap-1">
                  <Beer className="h-4 w-4" />
                  Prize Pool
                </TableHead>
                <TableHead className="flex items-center gap-1">
                  <Trophy className="h-4 w-4" />
                  Winning Ticket
                </TableHead>
                <TableHead>Total Tickets</TableHead>
                <TableHead className="flex items-center gap-1">
                  <Users className="h-4 w-4" />
                  Total Participants
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {history?.lotteries.map((lottery) => (
                <TableRow key={lottery.id}>
                  <TableCell className="font-medium">#{lottery.id}</TableCell>
                  <TableCell>{formatDate(lottery.startDate)}</TableCell>
                  <TableCell>{formatDate(lottery.endDate)}</TableCell>
                  <TableCell>🍺 {lottery.pool ?? 0}</TableCell>
                  <TableCell>{lottery.winningTicket ?? '-'}</TableCell>
                  <TableCell>{lottery.totalTickets}</TableCell>
                  <TableCell>{lottery.totalParticipants}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
        <div className="flex items-center justify-between mt-4">
          <Button
            variant="outline"
            onClick={() => setCurrentPage((prev) => Math.max(0, prev - 1))}
            disabled={currentPage === 0 || loading}
            className="flex items-center gap-2"
          >
            <ChevronLeft className="h-4 w-4" />
            Previous
          </Button>
          <span className="text-sm text-muted-foreground">
            Page {currentPage + 1} of {totalPages || 1}
          </span>
          <Button
            variant="outline"
            onClick={() => setCurrentPage((prev) => prev + 1)}
            disabled={currentPage >= totalPages - 1 || loading}
            className="flex items-center gap-2"
          >
            Next
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}

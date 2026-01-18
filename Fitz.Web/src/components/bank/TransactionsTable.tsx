'use client'

import { useEffect, useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import { TransactionsResponse, TransactionResponse } from '@/types/api'
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
import { Loader2, AlertCircle, ChevronLeft, ChevronRight, ArrowUpRight, ArrowDownLeft, Clock, Tag } from 'lucide-react'

const FITZ_USER_ID = '746797148263415989'
const ITEMS_PER_PAGE = 10

const reasonLabels: Record<string, string> = {
  AccountCreationBonus: 'Account Creation',
  Bonus: 'Bonus',
  Donated: 'Donated',
  Lotto: 'Lottery Ticket',
  LottoWin: 'Lottery Win',
  Rename: 'Rename',
  MusicPlay: 'Music Play',
  MusicSkip: 'Music Skip',
  HappyHour: 'Happy Hour',
  PollSubmitted: 'Poll Submitted',
  PollApproved: 'Poll Approved',
  PollDeclined: 'Poll Declined',
  PollVote: 'Poll Vote',
  PollCreatorTip: 'Poll Creator Tip',
}

function formatTimestamp(timestamp: string): string {
  const date = new Date(timestamp)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 1) return 'Just now'
  if (diffMins < 60) return `${diffMins} minute${diffMins !== 1 ? 's' : ''} ago`
  if (diffHours < 24) return `${diffHours} hour${diffHours !== 1 ? 's' : ''} ago`
  if (diffDays < 7) return `${diffDays} day${diffDays !== 1 ? 's' : ''} ago`
  return date.toLocaleDateString()
}

function formatUserId(userId: string): string {
  if (userId === FITZ_USER_ID) {
    return 'Fitz'
  }
  return `User ${userId.slice(-6)}`
}

interface TransactionsTableProps {
  userId: string
}

export function TransactionsTable({ userId }: TransactionsTableProps) {
  const [transactions, setTransactions] = useState<TransactionsResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [currentPage, setCurrentPage] = useState(0)
  const [error, setError] = useState<string | null>(null)

  const fetchTransactions = async (skip: number) => {
    setLoading(true)
    setError(null)
    try {
      const response = await apiClient.get<TransactionsResponse>(
        `/api/bank/transactions/${userId}?skip=${skip}&take=${ITEMS_PER_PAGE}`
      )
      if (response.success && response.data) {
        setTransactions(response.data)
      } else {
        setError('Failed to load transactions')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load transactions')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchTransactions(currentPage * ITEMS_PER_PAGE)
  }, [currentPage, userId])

  const totalPages = transactions ? Math.ceil(transactions.totalCount / ITEMS_PER_PAGE) : 0

  const getTransactionDirection = (transaction: TransactionResponse) => {
    if (transaction.sender === userId) {
      return {
        type: 'sent' as const,
        otherParty: transaction.recipient,
        icon: ArrowUpRight,
        label: 'Sent to',
      }
    }
    return {
      type: 'received' as const,
      otherParty: transaction.sender,
      icon: ArrowDownLeft,
      label: 'Received from',
    }
  }

  if (loading && !transactions) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Clock className="h-5 w-5" />
            <CardTitle>Transaction History</CardTitle>
          </div>
          <CardDescription>View your recent transactions</CardDescription>
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
            <Clock className="h-5 w-5" />
            <CardTitle>Transaction History</CardTitle>
          </div>
          <CardDescription>View your recent transactions</CardDescription>
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

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <Clock className="h-5 w-5" />
          <CardTitle>Transaction History</CardTitle>
        </div>
        <CardDescription>
          Showing {transactions?.transactions.length || 0} of {transactions?.totalCount || 0} transactions
        </CardDescription>
      </CardHeader>
      <CardContent>
        {transactions && transactions.transactions.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">
            <p>No transactions found</p>
          </div>
        ) : (
          <>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="flex items-center gap-1">
                    <Clock className="h-4 w-4" />
                    Date
                  </TableHead>
                  <TableHead className="flex items-center gap-1">
                    <Tag className="h-4 w-4" />
                    Type
                  </TableHead>
                  <TableHead>Direction</TableHead>
                  <TableHead className="text-right">Amount</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {transactions?.transactions.map((transaction) => {
                  const direction = getTransactionDirection(transaction)
                  const DirectionIcon = direction.icon
                  return (
                    <TableRow key={transaction.id}>
                      <TableCell className="text-sm text-muted-foreground">
                        {formatTimestamp(transaction.timestamp)}
                      </TableCell>
                      <TableCell>
                        <span className="text-sm">
                          {reasonLabels[transaction.reason] || transaction.reason}
                        </span>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <DirectionIcon
                            className={`h-4 w-4 ${
                              direction.type === 'sent' ? 'text-destructive' : 'text-green-600'
                            }`}
                          />
                          <span className="text-sm">
                            {direction.label} {formatUserId(direction.otherParty)}
                          </span>
                        </div>
                      </TableCell>
                      <TableCell
                        className={`text-right font-medium ${
                          direction.type === 'sent' ? 'text-destructive' : 'text-green-600'
                        }`}
                      >
                        {direction.type === 'sent' ? '-' : '+'}🍺 {transaction.amount}
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
            {totalPages > 1 && (
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
            )}
          </>
        )}
      </CardContent>
    </Card>
  )
}

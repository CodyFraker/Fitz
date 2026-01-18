'use client'

import { useEffect, useState } from 'react'
import { apiClient } from '@/lib/api/client'
import { BalancesResponse, AccountBalanceResponse } from '@/types/api'
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
import { Refrigerator, Loader2, AlertCircle, ChevronLeft, ChevronRight, Beer, Trophy, User } from 'lucide-react'

const FITZ_USER_ID = '746797148263415989'
const ITEMS_PER_PAGE = 10

export function BalancesTable() {
  const [balances, setBalances] = useState<BalancesResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [currentPage, setCurrentPage] = useState(0)
  const [error, setError] = useState<string | null>(null)

  const fetchBalances = async (skip: number) => {
    setLoading(true)
    setError(null)
    try {
      const response = await apiClient.get<BalancesResponse>(
        `/api/bank/balances?skip=${skip}&take=${ITEMS_PER_PAGE}`
      )
      if (response.success && response.data) {
        setBalances(response.data)
      } else {
        setError('Failed to load balances')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load balances')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchBalances(currentPage * ITEMS_PER_PAGE)
  }, [currentPage])

  const totalPages = balances ? Math.ceil(balances.totalCount / ITEMS_PER_PAGE) : 0

  const formatUsername = (account: AccountBalanceResponse) => {
    if (account.id === FITZ_USER_ID) {
      return 'Fitz'
    }
    return account.username || `User ${account.id}`
  }

  if (loading && !balances) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Refrigerator className="h-5 w-5" />
            <CardTitle>All Fridge Balances</CardTitle>
          </div>
          <CardDescription>View all user balances in the fridge</CardDescription>
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
            <Refrigerator className="h-5 w-5" />
            <CardTitle>All Fridge Balances</CardTitle>
          </div>
          <CardDescription>View all user balances in the fridge</CardDescription>
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
          <Refrigerator className="h-5 w-5" />
          <CardTitle>All Fridge Balances</CardTitle>
        </div>
        <CardDescription>
          Showing {balances?.accounts.length || 0} of {balances?.totalCount || 0} users
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="flex items-center gap-1">
                <Trophy className="h-4 w-4" />
                Rank
              </TableHead>
              <TableHead className="flex items-center gap-1">
                <User className="h-4 w-4" />
                Username
              </TableHead>
              <TableHead className="text-right flex items-center justify-end gap-1">
                <Beer className="h-4 w-4" />
                Beer
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {balances?.accounts.map((account, index) => (
              <TableRow key={account.id}>
                <TableCell>{currentPage * ITEMS_PER_PAGE + index + 1}</TableCell>
                <TableCell className="font-medium">{formatUsername(account)}</TableCell>
                <TableCell className="text-right">🍺 {account.beer}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
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

'use client'

import { useState, useEffect } from 'react'
import { apiClient } from '@/lib/api/client'
import { PollsResponse, PollResponse, PollType, PollStatus } from '@/types/api'
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
import { BarChart3, Loader2, AlertCircle, ArrowUpDown, ChevronLeft, ChevronRight, HelpCircle, Calendar, TrendingUp } from 'lucide-react'

const ITEMS_PER_PAGE = 10

const pollTypeLabels: Record<PollType, string> = {
  [PollType.Number]: 'Number',
  [PollType.YesOrNo]: 'Yes/No',
  [PollType.Color]: 'Color',
  [PollType.ThisOrThat]: 'This or That',
  [PollType.HotTake]: 'Hot Take',
}

type SortField = 'totalVotes' | 'submittedOn' | 'question'
type SortOrder = 'asc' | 'desc'

export function AllPollsTable() {
  const [polls, setPolls] = useState<PollsResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [currentPage, setCurrentPage] = useState(0)
  const [sortBy, setSortBy] = useState<SortField>('totalVotes')
  const [sortOrder, setSortOrder] = useState<SortOrder>('desc')
  const [error, setError] = useState<string | null>(null)

  const fetchPolls = async (skip: number, sortField: SortField, order: SortOrder) => {
    setLoading(true)
    setError(null)
    try {
      const response = await apiClient.getPollsWithDetails({
        status: PollStatus.Approved,
        skip,
        take: ITEMS_PER_PAGE,
        sortBy: sortField,
        sortOrder: order,
      })

      if (response.success && response.data) {
        setPolls(response.data)
      } else {
        setError('Failed to load polls')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load polls')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchPolls(currentPage * ITEMS_PER_PAGE, sortBy, sortOrder)
  }, [currentPage, sortBy, sortOrder])

  const handleSort = (field: SortField) => {
    if (sortBy === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')
    } else {
      setSortBy(field)
      setSortOrder('desc')
    }
    setCurrentPage(0)
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }

  const SortIcon = ({ field }: { field: SortField }) => {
    if (sortBy !== field) return <ArrowUpDown className="h-4 w-4 text-muted-foreground" />
    return sortOrder === 'asc' ? <ArrowUpDown className="h-4 w-4" /> : <ArrowUpDown className="h-4 w-4 rotate-180" />
  }

  if (loading && !polls) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <BarChart3 className="h-5 w-5" />
            <CardTitle>All Polls</CardTitle>
          </div>
          <CardDescription>Browse all approved polls</CardDescription>
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
            <BarChart3 className="h-5 w-5" />
            <CardTitle>All Polls</CardTitle>
          </div>
          <CardDescription>Browse all approved polls</CardDescription>
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

  const totalPages = polls ? Math.ceil(polls.totalCount / ITEMS_PER_PAGE) : 0

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <BarChart3 className="h-5 w-5" />
          <CardTitle>All Polls</CardTitle>
        </div>
        <CardDescription>
          Showing {polls?.polls.length || 0} of {polls?.totalCount || 0} polls
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>
                  <button
                    onClick={() => handleSort('question')}
                    className="flex items-center gap-1 hover:text-foreground"
                  >
                    <HelpCircle className="h-4 w-4" />
                    Question
                    <SortIcon field="question" />
                  </button>
                </TableHead>
                <TableHead>Type</TableHead>
                <TableHead>
                  <button
                    onClick={() => handleSort('totalVotes')}
                    className="flex items-center gap-1 hover:text-foreground"
                  >
                    <TrendingUp className="h-4 w-4" />
                    Votes
                    <SortIcon field="totalVotes" />
                  </button>
                </TableHead>
                <TableHead>
                  <button
                    onClick={() => handleSort('submittedOn')}
                    className="flex items-center gap-1 hover:text-foreground"
                  >
                    <Calendar className="h-4 w-4" />
                    Submitted
                    <SortIcon field="submittedOn" />
                  </button>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {polls?.polls.map((poll) => (
                <TableRow key={poll.id}>
                  <TableCell className="font-medium">{poll.question}</TableCell>
                  <TableCell>{pollTypeLabels[poll.type]}</TableCell>
                  <TableCell>{poll.totalVotes}</TableCell>
                  <TableCell>{formatDate(poll.submittedOn)}</TableCell>
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

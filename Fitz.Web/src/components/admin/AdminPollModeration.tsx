'use client'

import { useState, useEffect } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { apiClient } from '@/lib/api/client'
import { PollResponse, PollStatus } from '@/types/api'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { BarChart3, Trash2, RefreshCw, CheckCircle2, XCircle, Clock, Hash, HelpCircle, Calendar, Loader2 } from 'lucide-react'

export function AdminPollModeration() {
  const [polls, setPolls] = useState<PollResponse[]>([])
  const [loading, setLoading] = useState(false)
  const [pollId, setPollId] = useState('')

  const fetchPolls = async () => {
    setLoading(true)
    try {
      const response = await apiClient.getPolls()
      if (response.success && response.data) {
        setPolls(response.data)
      }
    } catch (error) {
      console.error('Failed to fetch polls:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchPolls()
  }, [])

  const handleDelete = async (id: number) => {
    if (!confirm(`Are you sure you want to delete poll ${id}?`)) return
    try {
      const response = await apiClient.adminDeletePoll(id)
      if (response.success) {
        alert('Poll deleted successfully')
        fetchPolls()
      } else {
        alert(`Failed to delete poll: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to delete poll:', error)
      alert(`Failed to delete poll: ${error.message}`)
    }
  }

  const handleEvaluate = async (id: number, status: PollStatus) => {
    try {
      const response = await apiClient.adminEvaluatePoll(id, status)
      if (response.success) {
        alert(`Poll ${status === PollStatus.Approved ? 'approved' : 'declined'} successfully`)
        fetchPolls()
      } else {
        alert(`Failed to evaluate poll: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to evaluate poll:', error)
      alert(`Failed to evaluate poll: ${error.message}`)
    }
  }

  const handleDeleteById = async () => {
    if (!pollId) return
    const id = parseInt(pollId)
    if (isNaN(id)) {
      alert('Invalid poll ID')
      return
    }
    await handleDelete(id)
    setPollId('')
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <BarChart3 className="h-5 w-5" />
          <CardTitle>Poll Moderation</CardTitle>
        </div>
        <CardDescription>Approve, decline, or delete polls</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex gap-2">
          <Input
            placeholder="Poll ID to delete"
            value={pollId}
            onChange={(e) => setPollId(e.target.value)}
            type="number"
          />
          <Button onClick={handleDeleteById} variant="destructive" className="flex items-center gap-2">
            <Trash2 className="h-4 w-4" />
            Delete by ID
          </Button>
          <Button onClick={fetchPolls} variant="outline" className="flex items-center gap-2">
            <RefreshCw className="h-4 w-4" />
            Refresh
          </Button>
        </div>

        {loading ? (
          <div className="flex items-center gap-2">
            <Loader2 className="h-4 w-4 animate-spin" />
            <p>Loading polls...</p>
          </div>
        ) : (
          <div className="border rounded-lg">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="flex items-center gap-1">
                    <Hash className="h-4 w-4" />
                    ID
                  </TableHead>
                  <TableHead className="flex items-center gap-1">
                    <HelpCircle className="h-4 w-4" />
                    Question
                  </TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="flex items-center gap-1">
                    <Calendar className="h-4 w-4" />
                    Submitted
                  </TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {polls.map((poll) => (
                  <TableRow key={poll.id}>
                    <TableCell>{poll.id}</TableCell>
                    <TableCell className="max-w-xs truncate">{poll.question}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        {poll.status === PollStatus.Pending && <Clock className="h-4 w-4 text-yellow-600" />}
                        {poll.status === PollStatus.Approved && <CheckCircle2 className="h-4 w-4 text-green-600" />}
                        {poll.status === PollStatus.Declined && <XCircle className="h-4 w-4 text-red-600" />}
                        {poll.status === PollStatus.Pending && 'Pending'}
                        {poll.status === PollStatus.Approved && 'Approved'}
                        {poll.status === PollStatus.Declined && 'Declined'}
                      </div>
                    </TableCell>
                    <TableCell>{new Date(poll.submittedOn).toLocaleDateString()}</TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        {poll.status === PollStatus.Pending && (
                          <>
                            <Button
                              size="sm"
                              onClick={() => handleEvaluate(poll.id, PollStatus.Approved)}
                              className="flex items-center gap-1"
                            >
                              <CheckCircle2 className="h-4 w-4" />
                              Approve
                            </Button>
                            <Button
                              size="sm"
                              variant="secondary"
                              onClick={() => handleEvaluate(poll.id, PollStatus.Declined)}
                              className="flex items-center gap-1"
                            >
                              <XCircle className="h-4 w-4" />
                              Decline
                            </Button>
                          </>
                        )}
                        <Button
                          size="sm"
                          variant="destructive"
                          onClick={() => handleDelete(poll.id)}
                          className="flex items-center gap-1"
                        >
                          <Trash2 className="h-4 w-4" />
                          Delete
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </CardContent>
    </Card>
  )
}

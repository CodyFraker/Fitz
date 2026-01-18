'use client'

import { useState, useEffect } from 'react'
import { apiClient } from '@/lib/api/client'
import { PollResponse } from '@/types/api'
import { PollCard } from './PollCard'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { BarChart3, Loader2, AlertCircle, Inbox, User } from 'lucide-react'

export function MyPollsSection() {
  const [polls, setPolls] = useState<PollResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const fetchPolls = async () => {
      setLoading(true)
      setError(null)
      try {
        const response = await apiClient.getUserPolls()
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

    fetchPolls()
  }, [])

  if (loading) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <BarChart3 className="h-5 w-5" />
            <CardTitle>My Polls</CardTitle>
          </div>
          <CardDescription>Polls you have created</CardDescription>
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
            <CardTitle>My Polls</CardTitle>
          </div>
          <CardDescription>Polls you have created</CardDescription>
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

  if (polls.length === 0) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <BarChart3 className="h-5 w-5" />
            <CardTitle>My Polls</CardTitle>
          </div>
          <CardDescription>Polls you have created</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-4">
            <Inbox className="h-8 w-8 text-muted-foreground" />
            <p className="text-sm text-muted-foreground mt-2">
              You haven't created any polls yet. Create your first poll to get started!
            </p>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="space-y-4">
      <div>
        <div className="flex items-center gap-2 mb-2">
          <User className="h-6 w-6" />
          <h2 className="text-2xl font-bold">My Polls</h2>
        </div>
        <p className="text-muted-foreground">
          {polls.length} poll{polls.length !== 1 ? 's' : ''} created
        </p>
      </div>
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {polls.map((poll) => (
          <PollCard key={poll.id} poll={poll} showVoteBreakdown={true} />
        ))}
      </div>
    </div>
  )
}

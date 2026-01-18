'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import { RenameResponse, RenameStatus } from '@/types/api'
import { RenameQueueCard } from './RenameQueueCard'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Edit, Loader2, AlertCircle, Inbox } from 'lucide-react'

export function MyRenamesSection() {
  const { user } = useAuth()
  const [renames, setRenames] = useState<RenameResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!user) {
      setLoading(false)
      return
    }

    const fetchRenames = async () => {
      setLoading(true)
      setError(null)
      try {
        const response = await apiClient.getRenamesByUser(user.id)
        if (response.success && response.data) {
          const sortedRenames = [...response.data].sort((a, b) => {
            if (a.status === RenameStatus.Active && b.status !== RenameStatus.Active) return -1
            if (a.status !== RenameStatus.Active && b.status === RenameStatus.Active) return 1
            if (a.expiration && b.expiration) {
              return new Date(a.expiration).getTime() - new Date(b.expiration).getTime()
            }
            return 0
          })
          setRenames(sortedRenames)
        } else {
          setError('Failed to load renames')
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load renames')
      } finally {
        setLoading(false)
      }
    }

    fetchRenames()
  }, [user])

  if (loading) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Edit className="h-5 w-5" />
            <CardTitle>My Renames</CardTitle>
          </div>
          <CardDescription>Your rename queue</CardDescription>
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
            <Edit className="h-5 w-5" />
            <CardTitle>My Renames</CardTitle>
          </div>
          <CardDescription>Your rename queue</CardDescription>
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

  const activeRenames = renames.filter((r) => r.status === RenameStatus.Active)
  const pendingRenames = renames.filter((r) => r.status === RenameStatus.Pending)
  const otherRenames = renames.filter(
    (r) => r.status !== RenameStatus.Active && r.status !== RenameStatus.Pending
  )

  if (renames.length === 0) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Edit className="h-5 w-5" />
            <CardTitle>My Renames</CardTitle>
          </div>
          <CardDescription>Your rename queue</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-4">
            <Inbox className="h-8 w-8 text-muted-foreground" />
            <p className="text-sm text-muted-foreground mt-2">
              You don&apos;t have any rename requests yet. Create one to get started!
            </p>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <div className="flex items-center gap-2 mb-2">
          <Edit className="h-6 w-6" />
          <h2 className="text-2xl font-bold">My Renames</h2>
        </div>
        <p className="text-muted-foreground">
          {renames.length} rename{renames.length !== 1 ? 's' : ''} in queue
        </p>
      </div>

      {activeRenames.length > 0 && (
        <div className="space-y-3">
          <h3 className="text-lg font-semibold text-green-700">Active</h3>
          <div className="space-y-3">
            {activeRenames.map((rename) => (
              <RenameQueueCard key={rename.id} rename={rename} />
            ))}
          </div>
        </div>
      )}

      {pendingRenames.length > 0 && (
        <div className="space-y-3">
          <h3 className="text-lg font-semibold text-yellow-700">Pending</h3>
          <div className="space-y-3">
            {pendingRenames.map((rename) => (
              <RenameQueueCard key={rename.id} rename={rename} />
            ))}
          </div>
        </div>
      )}

      {otherRenames.length > 0 && (
        <div className="space-y-3">
          <h3 className="text-lg font-semibold">Other</h3>
          <div className="space-y-3">
            {otherRenames.map((rename) => (
              <RenameQueueCard key={rename.id} rename={rename} />
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

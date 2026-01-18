'use client'

import { useState, useEffect } from 'react'
import { apiClient } from '@/lib/api/client'
import { RenameResponse, RenameStatus } from '@/types/api'
import { RenameQueueCard } from './RenameQueueCard'
import { UserAutocomplete } from '@/components/ui/user-autocomplete'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Edit, Loader2, AlertCircle, Filter, X } from 'lucide-react'

const renameStatusLabels: Record<RenameStatus, string> = {
  [RenameStatus.Unknown]: 'Unknown',
  [RenameStatus.Pending]: 'Pending',
  [RenameStatus.Active]: 'Active',
  [RenameStatus.Expired]: 'Expired',
  [RenameStatus.BoughtOut]: 'Bought Out',
  [RenameStatus.Permanent]: 'Permanent',
}

export function AllRenamesTable() {
  const [renames, setRenames] = useState<RenameResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [statusFilter, setStatusFilter] = useState<RenameStatus | 'all'>('all')
  const [userFilter, setUserFilter] = useState<string>('')

  useEffect(() => {
    const fetchRenames = async () => {
      setLoading(true)
      setError(null)
      try {
        const statusParam = statusFilter !== 'all' ? statusFilter : undefined
        const response = await apiClient.getRenames(statusParam)
        if (response.success && response.data) {
          let filtered = response.data as RenameResponse[]
          if (userFilter) {
            filtered = filtered.filter(
              (r) =>
                r.affectedUserId === userFilter || r.requestedUserId === userFilter
            )
          }
          setRenames(filtered)
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
  }, [statusFilter, userFilter])

  const clearFilters = () => {
    setStatusFilter('all')
    setUserFilter('')
  }

  if (loading && renames.length === 0) {
    return (
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Edit className="h-5 w-5" />
            <CardTitle>All Renames</CardTitle>
          </div>
          <CardDescription>Browse all rename requests</CardDescription>
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
            <CardTitle>All Renames</CardTitle>
          </div>
          <CardDescription>Browse all rename requests</CardDescription>
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

  const hasFilters = statusFilter !== 'all' || userFilter !== ''

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <Edit className="h-5 w-5" />
          <CardTitle>All Renames</CardTitle>
        </div>
        <CardDescription>
          Showing {renames.length} rename{renames.length !== 1 ? 's' : ''}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="space-y-4 mb-4">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <label className="text-sm font-medium mb-2 block">Filter by User</label>
              <UserAutocomplete
                value={userFilter}
                onChange={setUserFilter}
                placeholder="Search by user ID..."
                className="w-full"
              />
            </div>
            <div className="sm:w-48">
              <label className="text-sm font-medium mb-2 block">Filter by Status</label>
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value as RenameStatus | 'all')}
                className="w-full px-3 py-2 border rounded-md bg-background"
              >
                <option value="all">All Statuses</option>
                {Object.entries(renameStatusLabels).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </div>
          </div>
          {hasFilters && (
            <Button
              variant="outline"
              size="sm"
              onClick={clearFilters}
              className="flex items-center gap-2"
            >
              <X className="h-4 w-4" />
              Clear Filters
            </Button>
          )}
        </div>

        {renames.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-8">
            <Filter className="h-8 w-8 text-muted-foreground" />
            <p className="text-sm text-muted-foreground mt-2">
              {hasFilters ? 'No renames match your filters' : 'No rename requests found'}
            </p>
          </div>
        ) : (
          <>
            <div className="hidden md:block overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Old Name</TableHead>
                    <TableHead>New Name</TableHead>
                    <TableHead>Affected User</TableHead>
                    <TableHead>Requested By</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Cost</TableHead>
                    <TableHead>Expiration</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {renames.map((rename) => (
                    <TableRow key={rename.id}>
                      <TableCell className="font-medium">
                        {rename.oldName || 'N/A'}
                      </TableCell>
                      <TableCell className="font-medium">{rename.newName}</TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {rename.affectedUserId}
                      </TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {rename.requestedUserId}
                      </TableCell>
                      <TableCell>
                        <span className="px-2 py-1 rounded text-xs font-medium bg-secondary">
                          {renameStatusLabels[rename.status]}
                        </span>
                      </TableCell>
                      <TableCell>{rename.cost} 🍺</TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {rename.expiration
                          ? new Date(rename.expiration).toLocaleDateString()
                          : 'N/A'}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>

            <div className="md:hidden space-y-3">
              {renames.map((rename) => (
                <RenameQueueCard key={rename.id} rename={rename} showRequestedBy={true} />
              ))}
            </div>
          </>
        )}
      </CardContent>
    </Card>
  )
}

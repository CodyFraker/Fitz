'use client'

import { useState, useEffect } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { UserAutocomplete } from '@/components/ui/user-autocomplete'
import { apiClient } from '@/lib/api/client'
import { RenameResponse, RenameStatus } from '@/types/api'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Edit, RefreshCw, Loader2, AlertCircle, Hash, Filter, X } from 'lucide-react'

const renameStatusLabels: Record<RenameStatus, string> = {
  [RenameStatus.Unknown]: 'Unknown',
  [RenameStatus.Pending]: 'Pending',
  [RenameStatus.Active]: 'Active',
  [RenameStatus.Expired]: 'Expired',
  [RenameStatus.BoughtOut]: 'Bought Out',
  [RenameStatus.Permanent]: 'Permanent',
}

export function AdminRenameManagement() {
  const [renames, setRenames] = useState<RenameResponse[]>([])
  const [loading, setLoading] = useState(false)
  const [statusFilter, setStatusFilter] = useState<RenameStatus | 'all'>('all')
  const [userFilter, setUserFilter] = useState('')
  const [updatingId, setUpdatingId] = useState<number | null>(null)

  const fetchRenames = async () => {
    setLoading(true)
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
      }
    } catch (error) {
      console.error('Failed to fetch renames:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchRenames()
  }, [statusFilter, userFilter])

  const handleStatusUpdate = async (id: number, status: RenameStatus) => {
    if (!confirm(`Are you sure you want to update rename ${id} status to ${renameStatusLabels[status]}?`)) {
      return
    }

    setUpdatingId(id)
    try {
      const response = await apiClient.updateRenameStatus(id, status)
      if (response.success) {
        alert(`Rename ${id} status updated successfully`)
        fetchRenames()
      } else {
        alert(`Failed to update status: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to update rename status:', error)
      alert(`Failed to update status: ${error.message}`)
    } finally {
      setUpdatingId(null)
    }
  }

  const clearFilters = () => {
    setStatusFilter('all')
    setUserFilter('')
  }

  const hasFilters = statusFilter !== 'all' || userFilter !== ''

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <Edit className="h-5 w-5" />
          <CardTitle>Rename Management</CardTitle>
        </div>
        <CardDescription>Update rename statuses and manage rename requests</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
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

        <div className="flex gap-2">
          {hasFilters && (
            <Button
              variant="outline"
              onClick={clearFilters}
              className="flex items-center gap-2"
            >
              <X className="h-4 w-4" />
              Clear Filters
            </Button>
          )}
          <Button onClick={fetchRenames} variant="outline" className="flex items-center gap-2">
            <RefreshCw className="h-4 w-4" />
            Refresh
          </Button>
        </div>

        {loading ? (
          <div className="flex items-center gap-2">
            <Loader2 className="h-4 w-4 animate-spin" />
            <p>Loading renames...</p>
          </div>
        ) : (
          <div className="border rounded-lg overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="flex items-center gap-1">
                    <Hash className="h-4 w-4" />
                    ID
                  </TableHead>
                  <TableHead>Old Name</TableHead>
                  <TableHead>New Name</TableHead>
                  <TableHead>Affected User</TableHead>
                  <TableHead>Requested By</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Cost</TableHead>
                  <TableHead>Expiration</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {renames.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={9} className="text-center py-8 text-muted-foreground">
                      {hasFilters ? 'No renames match your filters' : 'No renames found'}
                    </TableCell>
                  </TableRow>
                ) : (
                  renames.map((rename) => (
                    <TableRow key={rename.id}>
                      <TableCell>{rename.id}</TableCell>
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
                        <select
                          value={rename.status}
                          onChange={(e) =>
                            handleStatusUpdate(rename.id, parseInt(e.target.value) as RenameStatus)
                          }
                          disabled={updatingId === rename.id}
                          className="px-2 py-1 border rounded bg-background text-sm"
                        >
                          {Object.entries(renameStatusLabels).map(([value, label]) => (
                            <option key={value} value={value}>
                              {label}
                            </option>
                          ))}
                        </select>
                        {updatingId === rename.id && (
                          <Loader2 className="h-3 w-3 ml-2 inline animate-spin" />
                        )}
                      </TableCell>
                      <TableCell>{rename.cost} 🍺</TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {rename.expiration
                          ? new Date(rename.expiration).toLocaleDateString()
                          : 'N/A'}
                      </TableCell>
                      <TableCell>
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => handleStatusUpdate(rename.id, rename.status)}
                          disabled={updatingId === rename.id}
                          className="flex items-center gap-1"
                        >
                          {updatingId === rename.id ? (
                            <>
                              <Loader2 className="h-3 w-3 animate-spin" />
                              Updating...
                            </>
                          ) : (
                            'Update'
                          )}
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
        )}
      </CardContent>
    </Card>
  )
}

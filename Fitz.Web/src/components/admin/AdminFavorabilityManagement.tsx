'use client'

import { useState, useEffect } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { UserAutocomplete } from '@/components/ui/user-autocomplete'
import { apiClient } from '@/lib/api/client'
import { User, Search, Edit, Save, X, Loader2, Beer, AlertTriangle, CheckCircle, ArrowUpDown } from 'lucide-react'

interface UserFavorability {
  userId: string
  username: string
  beer: number
  botBeer: number
  beerRatio: number
  favorability: number
  canUseCommands: boolean
}

export function AdminFavorabilityManagement() {
  const [users, setUsers] = useState<UserFavorability[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')
  const [skip, setSkip] = useState(0)
  const [take] = useState(20)
  const [sortBy, setSortBy] = useState<string>('id')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')
  const [editingUserId, setEditingUserId] = useState<string | null>(null)
  const [editFavorability, setEditFavorability] = useState('')
  const [selectedUsers, setSelectedUsers] = useState<Set<string>>(new Set())
  const [bulkFavorability, setBulkFavorability] = useState('')
  const [saving, setSaving] = useState(false)

  const fetchUsers = async () => {
    setLoading(true)
    try {
      const response = await apiClient.getUsersWithFavorability({
        query: searchQuery || undefined,
        skip,
        take,
        sortBy: sortBy === 'id' ? undefined : sortBy,
        sortOrder,
      })
      if (response.success && response.data) {
        setUsers(response.data.users.map((u: any) => ({
          userId: u.userId.toString(),
          username: u.username,
          beer: u.beer,
          botBeer: u.botBeer,
          beerRatio: u.beerRatio,
          favorability: u.favorability,
          canUseCommands: u.canUseCommands,
        })))
        setTotalCount(response.data.totalCount)
      }
    } catch (error) {
      console.error('Failed to fetch users:', error)
      alert('Failed to fetch users')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchUsers()
  }, [skip, sortBy, sortOrder])

  const handleSort = (column: string) => {
    if (sortBy === column) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')
    } else {
      setSortBy(column)
      setSortOrder('asc')
    }
  }

  const handleEdit = (user: UserFavorability) => {
    setEditingUserId(user.userId)
    setEditFavorability(user.favorability.toString())
  }

  const handleSave = async (userId: string) => {
    const favorability = parseInt(editFavorability)
    if (isNaN(favorability) || favorability < 0 || favorability > 100) {
      alert('Favorability must be between 0 and 100')
      return
    }

    setSaving(true)
    try {
      const response = await apiClient.adminUpdateFavorability(userId, favorability)
      if (response.success) {
        alert('Favorability updated successfully')
        setEditingUserId(null)
        fetchUsers()
      } else {
        alert(`Failed to update favorability: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to update favorability:', error)
      alert(`Failed to update favorability: ${error.message}`)
    } finally {
      setSaving(false)
    }
  }

  const handleBulkUpdate = async () => {
    if (selectedUsers.size === 0) {
      alert('Please select at least one user')
      return
    }

    const favorability = parseInt(bulkFavorability)
    if (isNaN(favorability) || favorability < 0 || favorability > 100) {
      alert('Favorability must be between 0 and 100')
      return
    }

    if (!confirm(`Update favorability to ${favorability} for ${selectedUsers.size} user(s)?`)) {
      return
    }

    setSaving(true)
    try {
      const response = await apiClient.adminBulkUpdateFavorability(
        Array.from(selectedUsers),
        favorability
      )
      if (response.success) {
        alert(response.message || 'Bulk update completed')
        setSelectedUsers(new Set())
        setBulkFavorability('')
        fetchUsers()
      } else {
        alert(`Failed to update favorability: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to bulk update favorability:', error)
      alert(`Failed to bulk update favorability: ${error.message}`)
    } finally {
      setSaving(false)
    }
  }

  const getFavorabilityColor = (favorability: number) => {
    if (favorability === 0) return 'text-red-600 font-bold'
    if (favorability < 25) return 'text-red-500'
    if (favorability < 50) return 'text-yellow-500'
    return 'text-green-500'
  }

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <User className="h-5 w-5" />
            <CardTitle>User Favorability Management</CardTitle>
          </div>
          <CardDescription>Manage user favorability, view beer ratios, and update favorability in bulk</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-2">
            <div className="flex-1">
              <UserAutocomplete
                value={searchQuery}
                onChange={setSearchQuery}
                onKeyDown={(e) => e.key === 'Enter' && fetchUsers()}
                placeholder="Search by username or enter User ID"
                disabled={loading}
              />
            </div>
            <Button onClick={fetchUsers} disabled={loading} className="flex items-center gap-2">
              {loading ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Loading...
                </>
              ) : (
                <>
                  <Search className="h-4 w-4" />
                  Search
                </>
              )}
            </Button>
          </div>

          {selectedUsers.size > 0 && (
            <Card className="bg-muted">
              <CardContent className="pt-6">
                <div className="flex items-center gap-4">
                  <div className="flex-1">
                    <Label>Bulk Update Favorability</Label>
                    <div className="flex gap-2 mt-2">
                      <Input
                        type="number"
                        min="0"
                        max="100"
                        value={bulkFavorability}
                        onChange={(e) => setBulkFavorability(e.target.value)}
                        placeholder="0-100"
                        className="w-32"
                      />
                      <Button
                        onClick={handleBulkUpdate}
                        disabled={saving || !bulkFavorability}
                        className="flex items-center gap-2"
                      >
                        {saving ? (
                          <>
                            <Loader2 className="h-4 w-4 animate-spin" />
                            Updating...
                          </>
                        ) : (
                          <>
                            <Save className="h-4 w-4" />
                            Update {selectedUsers.size} User(s)
                          </>
                        )}
                      </Button>
                      <Button
                        variant="ghost"
                        onClick={() => {
                          setSelectedUsers(new Set())
                          setBulkFavorability('')
                        }}
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <input
                      type="checkbox"
                      checked={selectedUsers.size === users.length && users.length > 0}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setSelectedUsers(new Set(users.map((u) => u.userId)))
                        } else {
                          setSelectedUsers(new Set())
                        }
                      }}
                    />
                  </TableHead>
                  <TableHead>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleSort('username')}
                      className="flex items-center gap-1"
                    >
                      Username
                      <ArrowUpDown className="h-3 w-3" />
                    </Button>
                  </TableHead>
                  <TableHead>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleSort('beer')}
                      className="flex items-center gap-1"
                    >
                      <Beer className="h-4 w-4" />
                      Beer
                      <ArrowUpDown className="h-3 w-3" />
                    </Button>
                  </TableHead>
                  <TableHead>Beer Ratio</TableHead>
                  <TableHead>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleSort('favorability')}
                      className="flex items-center gap-1"
                    >
                      Favorability
                      <ArrowUpDown className="h-3 w-3" />
                    </Button>
                  </TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center">
                      <Loader2 className="h-6 w-6 animate-spin mx-auto" />
                    </TableCell>
                  </TableRow>
                ) : users.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center text-muted-foreground">
                      No users found
                    </TableCell>
                  </TableRow>
                ) : (
                  users.map((user) => (
                    <TableRow key={user.userId}>
                      <TableCell>
                        <input
                          type="checkbox"
                          checked={selectedUsers.has(user.userId)}
                          onChange={(e) => {
                            const newSelected = new Set(selectedUsers)
                            if (e.target.checked) {
                              newSelected.add(user.userId)
                            } else {
                              newSelected.delete(user.userId)
                            }
                            setSelectedUsers(newSelected)
                          }}
                        />
                      </TableCell>
                      <TableCell className="font-medium">{user.username}</TableCell>
                      <TableCell>{user.beer.toLocaleString()}</TableCell>
                      <TableCell>
                        <span className={user.beerRatio >= 2.0 ? 'text-orange-600 font-semibold' : ''}>
                          {user.beerRatio.toFixed(2)}x
                        </span>
                      </TableCell>
                      <TableCell>
                        <span className={getFavorabilityColor(user.favorability)}>
                          {user.favorability}
                        </span>
                      </TableCell>
                      <TableCell>
                        {user.canUseCommands ? (
                          <span className="flex items-center gap-1 text-green-600">
                            <CheckCircle className="h-4 w-4" />
                            Can Use
                          </span>
                        ) : (
                          <span className="flex items-center gap-1 text-red-600">
                            <AlertTriangle className="h-4 w-4" />
                            Blocked
                          </span>
                        )}
                      </TableCell>
                      <TableCell>
                        {editingUserId === user.userId ? (
                          <div className="flex items-center gap-2">
                            <Input
                              type="number"
                              min="0"
                              max="100"
                              value={editFavorability}
                              onChange={(e) => setEditFavorability(e.target.value)}
                              className="w-20"
                            />
                            <Button
                              size="sm"
                              onClick={() => handleSave(user.userId)}
                              disabled={saving}
                            >
                              <Save className="h-4 w-4" />
                            </Button>
                            <Button
                              size="sm"
                              variant="ghost"
                              onClick={() => setEditingUserId(null)}
                            >
                              <X className="h-4 w-4" />
                            </Button>
                          </div>
                        ) : (
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => handleEdit(user)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>

          <div className="flex items-center justify-between">
            <div className="text-sm text-muted-foreground">
              Showing {skip + 1}-{Math.min(skip + take, totalCount)} of {totalCount} users
              </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                onClick={() => setSkip(Math.max(0, skip - take))}
                disabled={skip === 0 || loading}
              >
                Previous
              </Button>
              <Button
                variant="outline"
                onClick={() => setSkip(skip + take)}
                disabled={skip + take >= totalCount || loading}
              >
                Next
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

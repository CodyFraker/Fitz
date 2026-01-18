'use client'

import { useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { UserAutocomplete } from '@/components/ui/user-autocomplete'
import { apiClient } from '@/lib/api/client'
import { AccountResponse } from '@/types/api'
import { User, Search, Edit, Save, X, Loader2, Beer, Shield, Ticket, Calendar } from 'lucide-react'

export function AdminAccountManagement() {
  const [userId, setUserId] = useState('')
  const [account, setAccount] = useState<AccountResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [editing, setEditing] = useState(false)
  const [formData, setFormData] = useState({
    beer: '',
    lifetimeBeer: '',
    safeBalance: '',
    favorability: '',
    subscribeToLottery: '',
    subscribeTickets: '',
    deactivated: '',
  })

  const fetchAccount = async () => {
    if (!userId) return
    setLoading(true)
    try {
      const response = await apiClient.getAccount(userId)
      if (response.success && response.data) {
        setAccount(response.data)
        setFormData({
          beer: response.data.beer?.toString() || '',
          lifetimeBeer: response.data.lifetimeBeer?.toString() || '',
          safeBalance: response.data.safeBalance?.toString() || '',
          favorability: response.data.favorability?.toString() || '',
          subscribeToLottery: response.data.subscribeToLottery?.toString() || '',
          subscribeTickets: response.data.subscribeTickets?.toString() || '',
          deactivated: response.data.deactivated?.toString() || '',
        })
      }
    } catch (error) {
      console.error('Failed to fetch account:', error)
      alert('Failed to fetch account')
    } finally {
      setLoading(false)
    }
  }

  const handleSave = async () => {
    if (!userId || !account) return
    setLoading(true)
    try {
      const request: any = { userId }
      if (formData.beer) request.beer = parseInt(formData.beer)
      if (formData.lifetimeBeer) request.lifetimeBeer = parseInt(formData.lifetimeBeer)
      if (formData.safeBalance) request.safeBalance = parseInt(formData.safeBalance)
      if (formData.favorability) request.favorability = parseInt(formData.favorability)
      if (formData.subscribeToLottery) request.subscribeToLottery = formData.subscribeToLottery === 'true'
      if (formData.subscribeTickets) request.subscribeTickets = parseInt(formData.subscribeTickets)
      if (formData.deactivated) request.deactivated = formData.deactivated === 'true'

      const response = await apiClient.adminModifyAccount(userId, request)
      if (response.success) {
        alert('Account updated successfully')
        await fetchAccount()
        setEditing(false)
      } else {
        alert(`Failed to update account: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to update account:', error)
      alert(`Failed to update account: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <User className="h-5 w-5" />
          <CardTitle>Account Management</CardTitle>
        </div>
        <CardDescription>Modify user accounts, beer balances, and settings</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex gap-2">
          <div className="flex-1">
            <UserAutocomplete
              value={userId}
              onChange={setUserId}
              onKeyDown={(e) => e.key === 'Enter' && fetchAccount()}
              placeholder="Search by username or enter User ID"
              disabled={loading}
            />
          </div>
          <Button onClick={fetchAccount} disabled={loading || !userId} className="flex items-center gap-2">
            {loading ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                Loading...
              </>
            ) : (
              <>
                <Search className="h-4 w-4" />
                Load Account
              </>
            )}
          </Button>
        </div>

        {account && (
          <div className="space-y-4 border-t pt-4">
            <div className="flex justify-between items-center">
              <div className="flex items-center gap-2">
                <User className="h-5 w-5" />
                <div>
                  <p className="font-semibold">{account.username || 'Unknown'}</p>
                  <p className="text-sm text-muted-foreground">ID: {account.id}</p>
                </div>
              </div>
              <Button onClick={() => setEditing(!editing)} variant={editing ? 'secondary' : 'default'} className="flex items-center gap-2">
                {editing ? (
                  <>
                    <X className="h-4 w-4" />
                    Cancel
                  </>
                ) : (
                  <>
                    <Edit className="h-4 w-4" />
                    Edit
                  </>
                )}
              </Button>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label className="flex items-center gap-2">
                  <Beer className="h-4 w-4" />
                  Beer
                </Label>
                <Input
                  value={editing ? formData.beer : account.beer}
                  onChange={(e) => setFormData({ ...formData, beer: e.target.value })}
                  disabled={!editing}
                  type="number"
                />
              </div>
              <div>
                <Label className="flex items-center gap-2">
                  <Beer className="h-4 w-4" />
                  Lifetime Beer
                </Label>
                <Input
                  value={editing ? formData.lifetimeBeer : account.lifetimeBeer}
                  onChange={(e) => setFormData({ ...formData, lifetimeBeer: e.target.value })}
                  disabled={!editing}
                  type="number"
                />
              </div>
              <div>
                <Label className="flex items-center gap-2">
                  <Shield className="h-4 w-4" />
                  Safe Balance
                </Label>
                <Input
                  value={editing ? formData.safeBalance : account.safeBalance}
                  onChange={(e) => setFormData({ ...formData, safeBalance: e.target.value })}
                  disabled={!editing}
                  type="number"
                />
              </div>
              <div>
                <Label>Favorability</Label>
                <Input
                  value={editing ? formData.favorability : account.favorability}
                  onChange={(e) => setFormData({ ...formData, favorability: e.target.value })}
                  disabled={!editing}
                  type="number"
                />
              </div>
              <div>
                <Label className="flex items-center gap-2">
                  <Ticket className="h-4 w-4" />
                  Subscribe to Lottery
                </Label>
                <Input
                  value={editing ? formData.subscribeToLottery : account.subscribeToLottery.toString()}
                  onChange={(e) => setFormData({ ...formData, subscribeToLottery: e.target.value })}
                  disabled={!editing}
                  placeholder="true or false"
                />
              </div>
              <div>
                <Label className="flex items-center gap-2">
                  <Ticket className="h-4 w-4" />
                  Subscribe Tickets
                </Label>
                <Input
                  value={editing ? formData.subscribeTickets : account.subscribeTickets}
                  onChange={(e) => setFormData({ ...formData, subscribeTickets: e.target.value })}
                  disabled={!editing}
                  type="number"
                />
              </div>
              <div>
                <Label>Deactivated</Label>
                <Input
                  value={editing ? formData.deactivated : account.deactivated.toString()}
                  onChange={(e) => setFormData({ ...formData, deactivated: e.target.value })}
                  disabled={!editing}
                  placeholder="true or false"
                />
              </div>
            </div>

            {editing && (
              <Button onClick={handleSave} disabled={loading} className="w-full flex items-center gap-2">
                {loading ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Save className="h-4 w-4" />
                    Save Changes
                  </>
                )}
              </Button>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

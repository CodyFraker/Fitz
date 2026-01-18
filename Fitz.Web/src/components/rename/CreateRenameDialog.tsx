'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import {
  CreateRenameRequest,
  RenameResponse,
  RenameStatus,
  AccountResponse,
  RenameCostResponse,
} from '@/types/api'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { UserAutocomplete } from '@/components/ui/user-autocomplete'
import { RenameQueueCard } from './RenameQueueCard'
import { Plus, X, Loader2, AlertCircle, Coins, Calendar, CheckCircle2 } from 'lucide-react'

interface CreateRenameDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess?: () => void
}

export function CreateRenameDialog({
  open,
  onOpenChange,
  onSuccess,
}: CreateRenameDialogProps) {
  const { user } = useAuth()
  const [affectedUserId, setAffectedUserId] = useState('')
  const [newName, setNewName] = useState('')
  const [days, setDays] = useState(1)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [cost, setCost] = useState<number | null>(null)
  const [calculatingCost, setCalculatingCost] = useState(false)
  const [account, setAccount] = useState<AccountResponse | null>(null)
  const [existingRenames, setExistingRenames] = useState<RenameResponse[]>([])
  const [loadingQueue, setLoadingQueue] = useState(false)
  const [step, setStep] = useState<'form' | 'confirm'>('form')

  useEffect(() => {
    if (open) {
      resetForm()
      if (user) {
        fetchAccount()
      }
    }
  }, [open, user])

  useEffect(() => {
    if (affectedUserId && newName && days >= 1 && days <= 365 && user) {
      const timer = setTimeout(() => {
        calculateCost()
      }, 500)
      return () => clearTimeout(timer)
    } else {
      setCost(null)
    }
  }, [affectedUserId, newName, days, user])

  useEffect(() => {
    if (affectedUserId && user) {
      fetchExistingRenames()
    } else {
      setExistingRenames([])
    }
  }, [affectedUserId, user])

  const resetForm = () => {
    setAffectedUserId('')
    setNewName('')
    setDays(1)
    setError(null)
    setCost(null)
    setExistingRenames([])
    setStep('form')
  }

  const fetchAccount = async () => {
    if (!user) return
    try {
      const response = await apiClient.getAccount(user.id)
      if (response.success && response.data) {
        setAccount(response.data)
      }
    } catch (err) {
      console.error('Failed to fetch account:', err)
    }
  }

  const fetchExistingRenames = async () => {
    if (!affectedUserId) return
    setLoadingQueue(true)
    try {
      const allRenamesResponse = await apiClient.getRenames()
      if (allRenamesResponse.success && allRenamesResponse.data) {
        const filtered = (allRenamesResponse.data as RenameResponse[]).filter(
          (r) =>
            r.affectedUserId === affectedUserId &&
            (r.status === RenameStatus.Active || r.status === RenameStatus.Pending)
        )
        const sorted = filtered.sort((a, b) => {
          if (a.expiration && b.expiration) {
            return new Date(a.expiration).getTime() - new Date(b.expiration).getTime()
          }
          return 0
        })
        setExistingRenames(sorted)
      }
    } catch (err) {
      console.error('Failed to fetch existing renames:', err)
    } finally {
      setLoadingQueue(false)
    }
  }

  const calculateCost = async () => {
    if (!affectedUserId || !newName.trim() || !user || days < 1 || days > 365) {
      setCost(null)
      return
    }

    setCalculatingCost(true)
    try {
      const response = await apiClient.calculateRenameCost({
        affectedUserId,
        requestedUserId: user.id,
        days,
        newName: newName.trim(),
      })
      if (response.success && response.data) {
        setCost((response.data as RenameCostResponse).cost)
      } else {
        setCost(null)
      }
    } catch (err) {
      console.error('Failed to calculate cost:', err)
      setCost(null)
    } finally {
      setCalculatingCost(false)
    }
  }

  const getBuyoutCost = (): number => {
    const existingCost = existingRenames.reduce((sum, r) => sum + r.cost, 0)
    return existingCost + (cost || 0)
  }

  const canBuyout = (): boolean => {
    if (!account || !cost) return false
    return account.beer >= getBuyoutCost()
  }

  const canCreatePending = (): boolean => {
    if (!account || !cost) return false
    return account.beer >= cost
  }

  const handleCreateActive = async () => {
    if (!user || !affectedUserId || !newName.trim() || !cost) return

    setLoading(true)
    setError(null)

    try {
      const startDate = new Date()
      const expiration = new Date(startDate)
      expiration.setDate(expiration.getDate() + days)

      const request: CreateRenameRequest = {
        newName: newName.trim(),
        affectedUserId,
        requestedUserId: user.id,
        days,
        startDate: startDate.toISOString(),
        expiration: expiration.toISOString(),
        status: RenameStatus.Active,
      }

      const response = await apiClient.createRename(request)
      if (response.success) {
        onSuccess?.()
        onOpenChange(false)
        resetForm()
      } else {
        setError(response.message || 'Failed to create rename')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create rename')
    } finally {
      setLoading(false)
    }
  }

  const handleCreatePending = async () => {
    if (!user || !affectedUserId || !newName.trim() || !cost) return

    setLoading(true)
    setError(null)

    try {
      let startDate: Date
      let expiration: Date

      if (existingRenames.length > 0 && existingRenames[0].expiration) {
        startDate = new Date(existingRenames[0].expiration)
        expiration = new Date(startDate)
        expiration.setDate(expiration.getDate() + days)
      } else {
        startDate = new Date()
        expiration = new Date(startDate)
        expiration.setDate(expiration.getDate() + days)
      }

      const request: CreateRenameRequest = {
        newName: newName.trim(),
        affectedUserId,
        requestedUserId: user.id,
        days,
        startDate: startDate.toISOString(),
        expiration: expiration.toISOString(),
        status: RenameStatus.Pending,
      }

      const response = await apiClient.createRename(request)
      if (response.success) {
        onSuccess?.()
        onOpenChange(false)
        resetForm()
      } else {
        setError(response.message || 'Failed to create rename')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create rename')
    } finally {
      setLoading(false)
    }
  }

  const handleBuyoutAndCreate = async () => {
    if (!user || !affectedUserId || !newName.trim() || !cost) return

    setLoading(true)
    setError(null)

    try {
      const buyoutResponse = await apiClient.buyoutRenames(affectedUserId)
      if (!buyoutResponse.success) {
        setError(buyoutResponse.message || 'Failed to buyout renames')
        setLoading(false)
        return
      }

      const startDate = new Date()
      const expiration = new Date(startDate)
      expiration.setDate(expiration.getDate() + days)

      const request: CreateRenameRequest = {
        newName: newName.trim(),
        affectedUserId,
        requestedUserId: user.id,
        days,
        startDate: startDate.toISOString(),
        expiration: expiration.toISOString(),
        status: RenameStatus.Active,
      }

      const createResponse = await apiClient.createRename(request)
      if (createResponse.success) {
        onSuccess?.()
        onOpenChange(false)
        resetForm()
      } else {
        setError(createResponse.message || 'Failed to create rename')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to buyout and create rename')
    } finally {
      setLoading(false)
    }
  }

  const isValid = affectedUserId && newName.trim().length > 0 && newName.trim().length <= 32 && days >= 1 && days <= 365

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <div className="flex items-center gap-2">
            <Plus className="h-5 w-5" />
            <DialogTitle>Create Rename Request</DialogTitle>
          </div>
          <DialogDescription>
            Create a rename request for another user. The first item in the queue is currently active.
          </DialogDescription>
        </DialogHeader>

        {step === 'form' ? (
          <form
            onSubmit={(e) => {
              e.preventDefault()
              if (isValid && cost && existingRenames.length === 0) {
                handleCreateActive()
              } else if (isValid && cost) {
                setStep('confirm')
              }
            }}
          >
            <div className="space-y-6 py-4">
              <div className="space-y-2">
                <Label htmlFor="affectedUser">User to Rename</Label>
                <UserAutocomplete
                  value={affectedUserId}
                  onChange={setAffectedUserId}
                  placeholder="Select user..."
                  className="w-full"
                />
                <p className="text-xs text-muted-foreground">
                  The user whose nickname will be changed
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="newName">
                  New Name <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="newName"
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  placeholder="Enter new nickname"
                  maxLength={32}
                  disabled={loading}
                  required
                />
                <p className="text-xs text-muted-foreground">
                  {newName.length}/32 characters (max length for Discord nicknames)
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="days">
                  Duration (Days) <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="days"
                  type="number"
                  min={1}
                  max={365}
                  value={days}
                  onChange={(e) => setDays(parseInt(e.target.value) || 1)}
                  disabled={loading}
                  required
                />
                <p className="text-xs text-muted-foreground">Between 1 and 365 days</p>
              </div>

              {calculatingCost && (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Loader2 className="h-4 w-4 animate-spin" />
                  <span>Calculating cost...</span>
                </div>
              )}

              {cost !== null && account && (
                <div className="p-4 border rounded-lg space-y-2">
                  <div className="flex items-center gap-2">
                    <Coins className="h-5 w-5" />
                    <span className="font-semibold">Cost: {cost} 🍺</span>
                  </div>
                  <div className="flex items-center gap-2 text-sm">
                    <span>Your balance: {account.beer} 🍺</span>
                    {account.beer < cost && (
                      <span className="text-destructive">(Insufficient funds)</span>
                    )}
                  </div>
                </div>
              )}

              {loadingQueue ? (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Loader2 className="h-4 w-4 animate-spin" />
                  <span>Loading existing renames...</span>
                </div>
              ) : existingRenames.length > 0 ? (
                <div className="space-y-3">
                  <div className="flex items-center gap-2">
                    <AlertCircle className="h-5 w-5 text-yellow-600" />
                    <span className="font-semibold">
                      {affectedUserId} already has {existingRenames.length} rename{existingRenames.length !== 1 ? 's' : ''} in queue
                    </span>
                  </div>
                  <div className="space-y-2 max-h-64 overflow-y-auto">
                    {existingRenames.map((rename) => (
                      <RenameQueueCard key={rename.id} rename={rename} />
                    ))}
                  </div>
                  {cost && (
                    <div className="p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
                      <div className="text-sm">
                        <div className="font-semibold mb-1">Buyout Cost:</div>
                        <div className="text-lg">
                          {getBuyoutCost()} 🍺 (includes {existingRenames.length} existing rename{existingRenames.length !== 1 ? 's' : ''} + new rename)
                        </div>
                        {account && (
                          <div className="mt-1 text-xs">
                            Your balance: {account.beer} 🍺
                            {!canBuyout() && (
                              <span className="text-destructive ml-2">(Insufficient funds for buyout)</span>
                            )}
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              ) : affectedUserId && (
                <div className="p-3 bg-green-50 border border-green-200 rounded-lg">
                  <div className="flex items-center gap-2 text-sm text-green-800">
                    <CheckCircle2 className="h-4 w-4" />
                    <span>No existing renames. Your rename will be active immediately.</span>
                  </div>
                </div>
              )}

              {error && (
                <div className="flex items-center gap-2 text-sm text-destructive">
                  <AlertCircle className="h-4 w-4" />
                  <p>{error}</p>
                </div>
              )}
            </div>
            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => {
                  resetForm()
                  onOpenChange(false)
                }}
                disabled={loading}
              >
                <X className="h-4 w-4 mr-2" />
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={loading || !isValid || !cost || (account ? account.beer < cost : false)}
              >
                {loading ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Creating...
                  </>
                ) : (
                  <>
                    <Plus className="h-4 w-4 mr-2" />
                    Continue
                  </>
                )}
              </Button>
            </DialogFooter>
          </form>
        ) : (
          <div className="space-y-6 py-4">
            <div className="space-y-4">
              <div className="p-4 border rounded-lg space-y-2">
                <div className="font-semibold">Rename Details</div>
                <div className="text-sm space-y-1">
                  <div>User: {affectedUserId}</div>
                  <div>New Name: {newName}</div>
                  <div>Duration: {days} day{days !== 1 ? 's' : ''}</div>
                  <div>Cost: {cost} 🍺</div>
                </div>
              </div>

              {existingRenames.length > 0 && (
                <div className="space-y-3">
                  <div className="font-semibold">Existing Queue</div>
                  <div className="space-y-2 max-h-64 overflow-y-auto">
                    {existingRenames.map((rename) => (
                      <RenameQueueCard key={rename.id} rename={rename} />
                    ))}
                  </div>
                </div>
              )}

              {error && (
                <div className="flex items-center gap-2 text-sm text-destructive">
                  <AlertCircle className="h-4 w-4" />
                  <p>{error}</p>
                </div>
              )}
            </div>

            <DialogFooter className="flex-col sm:flex-row gap-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => setStep('form')}
                disabled={loading}
                className="w-full sm:w-auto"
              >
                <X className="h-4 w-4 mr-2" />
                Back
              </Button>
              {existingRenames.length > 0 && canCreatePending() && (
                <Button
                  type="button"
                  variant="outline"
                  onClick={handleCreatePending}
                  disabled={loading}
                  className="w-full sm:w-auto"
                >
                  {loading ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Creating...
                    </>
                  ) : (
                    <>
                      <Calendar className="h-4 w-4 mr-2" />
                      Create as Pending
                    </>
                  )}
                </Button>
              )}
              {existingRenames.length > 0 && canBuyout() && (
                <Button
                  type="button"
                  onClick={handleBuyoutAndCreate}
                  disabled={loading}
                  className="w-full sm:w-auto"
                >
                  {loading ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Processing...
                    </>
                  ) : (
                    <>
                      <Coins className="h-4 w-4 mr-2" />
                      Buyout & Create ({getBuyoutCost()} 🍺)
                    </>
                  )}
                </Button>
              )}
              {existingRenames.length === 0 && (
                <Button
                  type="button"
                  onClick={handleCreateActive}
                  disabled={loading || !cost || (account ? account.beer < cost : false)}
                >
                  {loading ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Creating...
                    </>
                  ) : (
                    <>
                      <Plus className="h-4 w-4 mr-2" />
                      Create Rename ({cost} 🍺)
                    </>
                  )}
                </Button>
              )}
            </DialogFooter>
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}

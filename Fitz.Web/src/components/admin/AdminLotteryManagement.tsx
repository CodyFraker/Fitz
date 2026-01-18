'use client'

import { useState, useEffect } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { apiClient } from '@/lib/api/client'
import { CurrentLotteryResponse } from '@/types/api'
import { Ticket, Loader2, Beer, Users, Calendar, Plus, X, AlertCircle, Clock, Edit, Play, ShoppingCart } from 'lucide-react'

export function AdminLotteryManagement() {
  const [currentLottery, setCurrentLottery] = useState<CurrentLotteryResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [creating, setCreating] = useState(false)
  const [formData, setFormData] = useState({
    startDate: '',
    endDate: '',
    pool: '0',
  })
  const [extendingEndDate, setExtendingEndDate] = useState(false)
  const [newEndDate, setNewEndDate] = useState('')
  const [modifyingPool, setModifyingPool] = useState(false)
  const [newPool, setNewPool] = useState('')
  const [buyingFitzTickets, setBuyingFitzTickets] = useState(false)
  const [fitzTicketsCount, setFitzTicketsCount] = useState('')

  const fetchCurrentLottery = async () => {
    setLoading(true)
    try {
      const response = await apiClient.getCurrentLottery()
      if (response.success && response.data) {
        setCurrentLottery(response.data)
      }
    } catch (error) {
      console.error('Failed to fetch current lottery:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchCurrentLottery()
  }, [])

  const handleCreateLottery = async () => {
    setLoading(true)
    try {
      const request: any = {
        pool: parseInt(formData.pool) || 0,
      }
      if (formData.startDate) {
        request.startDate = new Date(formData.startDate).toISOString()
      }
      if (formData.endDate) {
        request.endDate = new Date(formData.endDate).toISOString()
      }

      const response = await apiClient.adminCreateLottery(request)
      if (response.success) {
        alert('Lottery created successfully')
        setFormData({ startDate: '', endDate: '', pool: '0' })
        setCreating(false)
        fetchCurrentLottery()
      } else {
        alert(`Failed to create lottery: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to create lottery:', error)
      alert(`Failed to create lottery: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  const handleCancelLottery = async () => {
    if (!confirm('Are you sure you want to cancel the current lottery?')) return
    setLoading(true)
    try {
      const response = await apiClient.adminCancelLottery()
      if (response.success) {
        alert('Lottery cancelled successfully')
        fetchCurrentLottery()
      } else {
        alert(`Failed to cancel lottery: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to cancel lottery:', error)
      alert(`Failed to cancel lottery: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  const handleExtendEndDate = async () => {
    if (!newEndDate) {
      alert('Please enter a new end date')
      return
    }
    setLoading(true)
    try {
      const response = await apiClient.adminExtendLotteryEndDate(new Date(newEndDate).toISOString())
      if (response.success) {
        alert('Lottery end date extended successfully')
        setExtendingEndDate(false)
        setNewEndDate('')
        fetchCurrentLottery()
      } else {
        alert(`Failed to extend end date: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to extend end date:', error)
      alert(`Failed to extend end date: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  const handleModifyPool = async () => {
    const poolValue = parseInt(newPool)
    if (isNaN(poolValue) || poolValue < 0) {
      alert('Please enter a valid pool amount (0 or greater)')
      return
    }
    setLoading(true)
    try {
      const response = await apiClient.adminModifyLotteryPool(poolValue)
      if (response.success) {
        alert('Prize pool modified successfully')
        setModifyingPool(false)
        setNewPool('')
        fetchCurrentLottery()
      } else {
        alert(`Failed to modify pool: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to modify pool:', error)
      alert(`Failed to modify pool: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  const handleEndLottery = async () => {
    if (!confirm('Are you sure you want to end the current lottery? This will determine winners and end the lottery.')) return
    setLoading(true)
    try {
      const response = await apiClient.adminEndLottery()
      if (response.success) {
        alert('Lottery ended successfully and winners have been determined')
        fetchCurrentLottery()
      } else {
        alert(`Failed to end lottery: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to end lottery:', error)
      alert(`Failed to end lottery: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  const handleBuyFitzTickets = async () => {
    const ticketsValue = parseInt(fitzTicketsCount)
    if (isNaN(ticketsValue) || ticketsValue < 1) {
      alert('Please enter a valid number of tickets (1 or greater)')
      return
    }
    setLoading(true)
    try {
      const response = await apiClient.adminBuyFitzTickets(ticketsValue)
      if (response.success) {
        alert('Fitz tickets purchased successfully')
        setBuyingFitzTickets(false)
        setFitzTicketsCount('')
        fetchCurrentLottery()
      } else {
        alert(`Failed to buy Fitz tickets: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to buy Fitz tickets:', error)
      alert(`Failed to buy Fitz tickets: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Ticket className="h-5 w-5" />
            <CardTitle>Current Lottery</CardTitle>
          </div>
          <CardDescription>View and manage the active lottery</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {loading ? (
            <div className="flex items-center gap-2">
              <Loader2 className="h-4 w-4 animate-spin" />
              <p>Loading...</p>
            </div>
          ) : currentLottery ? (
            <div className="space-y-2">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="flex items-center gap-2">
                    <Beer className="h-4 w-4" />
                    Pool
                  </Label>
                  <p className="text-lg font-semibold">{currentLottery.pool ?? 0}</p>
                </div>
                <div>
                  <Label className="flex items-center gap-2">
                    <Ticket className="h-4 w-4" />
                    Total Tickets
                  </Label>
                  <p className="text-lg font-semibold">{currentLottery.totalTickets}</p>
                </div>
                <div>
                  <Label className="flex items-center gap-2">
                    <Users className="h-4 w-4" />
                    Participants
                  </Label>
                  <p className="text-lg font-semibold">{currentLottery.totalParticipants}</p>
                </div>
                <div>
                  <Label className="flex items-center gap-2">
                    <Calendar className="h-4 w-4" />
                    End Date
                  </Label>
                  <p className="text-lg font-semibold">
                    {new Date(currentLottery.endDate).toLocaleString()}
                  </p>
                </div>
              </div>
              <div className="space-y-4 pt-4 border-t">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {!extendingEndDate ? (
                    <Button onClick={() => setExtendingEndDate(true)} variant="outline" disabled={loading} className="flex items-center gap-2">
                      <Clock className="h-4 w-4" />
                      Extend End Date
                    </Button>
                  ) : (
                    <div className="space-y-2">
                      <Label>New End Date</Label>
                      <div className="flex gap-2">
                        <Input
                          type="datetime-local"
                          value={newEndDate}
                          onChange={(e) => setNewEndDate(e.target.value)}
                          disabled={loading}
                        />
                        <Button onClick={handleExtendEndDate} disabled={loading} size="sm" className="flex items-center gap-2">
                          {loading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Clock className="h-4 w-4" />}
                          Save
                        </Button>
                        <Button
                          onClick={() => {
                            setExtendingEndDate(false)
                            setNewEndDate('')
                          }}
                          variant="secondary"
                          size="sm"
                          disabled={loading}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  )}
                  {!modifyingPool ? (
                    <Button onClick={() => setModifyingPool(true)} variant="outline" disabled={loading} className="flex items-center gap-2">
                      <Edit className="h-4 w-4" />
                      Modify Prize Pool
                    </Button>
                  ) : (
                    <div className="space-y-2">
                      <Label>New Prize Pool</Label>
                      <div className="flex gap-2">
                        <Input
                          type="number"
                          value={newPool}
                          onChange={(e) => setNewPool(e.target.value)}
                          placeholder="Enter pool amount"
                          disabled={loading}
                        />
                        <Button onClick={handleModifyPool} disabled={loading} size="sm" className="flex items-center gap-2">
                          {loading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Edit className="h-4 w-4" />}
                          Save
                        </Button>
                        <Button
                          onClick={() => {
                            setModifyingPool(false)
                            setNewPool('')
                          }}
                          variant="secondary"
                          size="sm"
                          disabled={loading}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  )}
                  {!buyingFitzTickets ? (
                    <Button onClick={() => setBuyingFitzTickets(true)} variant="outline" disabled={loading} className="flex items-center gap-2">
                      <ShoppingCart className="h-4 w-4" />
                      Buy Fitz Tickets
                    </Button>
                  ) : (
                    <div className="space-y-2">
                      <Label>Number of Tickets</Label>
                      <div className="flex gap-2">
                        <Input
                          type="number"
                          value={fitzTicketsCount}
                          onChange={(e) => setFitzTicketsCount(e.target.value)}
                          placeholder="Enter ticket count"
                          disabled={loading}
                          min="1"
                        />
                        <Button onClick={handleBuyFitzTickets} disabled={loading} size="sm" className="flex items-center gap-2">
                          {loading ? <Loader2 className="h-4 w-4 animate-spin" /> : <ShoppingCart className="h-4 w-4" />}
                          Buy
                        </Button>
                        <Button
                          onClick={() => {
                            setBuyingFitzTickets(false)
                            setFitzTicketsCount('')
                          }}
                          variant="secondary"
                          size="sm"
                          disabled={loading}
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  )}
                  <Button onClick={handleEndLottery} variant="default" disabled={loading} className="flex items-center gap-2">
                    <Play className="h-4 w-4" />
                    End Lottery
                  </Button>
                </div>
                <Button onClick={handleCancelLottery} variant="destructive" disabled={loading} className="flex items-center gap-2 w-full">
                  <X className="h-4 w-4" />
                  Cancel Lottery
                </Button>
              </div>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <AlertCircle className="h-4 w-4 text-muted-foreground" />
              <p>No active lottery</p>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Plus className="h-5 w-5" />
            <CardTitle>Create New Lottery</CardTitle>
          </div>
          <CardDescription>Start a new lottery drawing</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {!creating ? (
            <Button onClick={() => setCreating(true)} className="flex items-center gap-2">
              <Plus className="h-4 w-4" />
              Create New Lottery
            </Button>
          ) : (
            <div className="space-y-4">
              <div>
                <Label className="flex items-center gap-2">
                  <Calendar className="h-4 w-4" />
                  Start Date (optional)
                </Label>
                <Input
                  type="datetime-local"
                  value={formData.startDate}
                  onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                />
              </div>
              <div>
                <Label className="flex items-center gap-2">
                  <Calendar className="h-4 w-4" />
                  End Date (optional)
                </Label>
                <Input
                  type="datetime-local"
                  value={formData.endDate}
                  onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                />
              </div>
              <div>
                <Label className="flex items-center gap-2">
                  <Beer className="h-4 w-4" />
                  Initial Pool
                </Label>
                <Input
                  type="number"
                  value={formData.pool}
                  onChange={(e) => setFormData({ ...formData, pool: e.target.value })}
                />
              </div>
              <div className="flex gap-2">
                <Button onClick={handleCreateLottery} disabled={loading} className="flex items-center gap-2">
                  {loading ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Creating...
                    </>
                  ) : (
                    <>
                      <Plus className="h-4 w-4" />
                      Create Lottery
                    </>
                  )}
                </Button>
                <Button
                  variant="secondary"
                  onClick={() => {
                    setCreating(false)
                    setFormData({ startDate: '', endDate: '', pool: '0' })
                  }}
                  className="flex items-center gap-2"
                >
                  <X className="h-4 w-4" />
                  Cancel
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

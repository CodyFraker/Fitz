'use client'

import { useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
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
import { Send, X, Loader2, Beer, User, AlertCircle } from 'lucide-react'

const FITZ_USER_ID = '746797148263415989'

interface TransferBeerDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess?: () => void
  currentBalance: number
}

export function TransferBeerDialog({
  open,
  onOpenChange,
  onSuccess,
  currentBalance,
}: TransferBeerDialogProps) {
  const { user } = useAuth()
  const [recipientId, setRecipientId] = useState('')
  const [amount, setAmount] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    if (!user) {
      setError('You must be logged in to transfer beer')
      return
    }

    const amountNum = parseInt(amount, 10)
    if (isNaN(amountNum) || amountNum <= 0) {
      setError('Amount must be a positive number')
      return
    }

    if (amountNum > currentBalance) {
      setError('Insufficient balance')
      return
    }

    if (!recipientId.trim()) {
      setError('Recipient ID is required')
      return
    }

    setLoading(true)
    try {
      const response = await apiClient.post('/api/bank/transfer', {
        senderId: user.id,
        recipientId: recipientId.trim(),
        amount: amountNum,
      })

      if (response.success) {
        setRecipientId('')
        setAmount('')
        onOpenChange(false)
        onSuccess?.()
      } else {
        setError(response.message || 'Failed to transfer beer')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to transfer beer')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <div className="flex items-center gap-2">
            <Send className="h-5 w-5" />
            <DialogTitle>Transfer Beer</DialogTitle>
          </div>
          <DialogDescription>
            Send beer from your fridge to another user. You can send to Fitz by using ID: {FITZ_USER_ID}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="recipient" className="flex items-center gap-2">
                <User className="h-4 w-4" />
                Recipient ID
              </Label>
              <Input
                id="recipient"
                value={recipientId}
                onChange={(e) => setRecipientId(e.target.value)}
                placeholder={FITZ_USER_ID}
                disabled={loading}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="amount" className="flex items-center gap-2">
                <Beer className="h-4 w-4" />
                Amount
              </Label>
              <Input
                id="amount"
                type="number"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                placeholder="Enter amount"
                min="1"
                max={currentBalance}
                disabled={loading}
              />
              <p className="text-sm text-muted-foreground flex items-center gap-1">
                <Beer className="h-4 w-4" />
                Current balance: {currentBalance} 🍺
              </p>
            </div>
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
              onClick={() => onOpenChange(false)}
              disabled={loading}
              className="flex items-center gap-2"
            >
              <X className="h-4 w-4" />
              Cancel
            </Button>
            <Button type="submit" disabled={loading} className="flex items-center gap-2">
              {loading ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Transferring...
                </>
              ) : (
                <>
                  <Send className="h-4 w-4" />
                  Transfer
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

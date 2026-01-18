'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import { PollType, CreatePollRequest, PollOptionRequest, SettingsResponse } from '@/types/api'
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
import { PollTypeSelector } from './PollTypeSelector'
import { Plus, X, Loader2, AlertCircle, Beer, HelpCircle } from 'lucide-react'

interface CreatePollDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess?: () => void
}

const numberEmojis = ['1️⃣', '2️⃣', '3️⃣', '4️⃣', '5️⃣', '6️⃣', '7️⃣', '8️⃣', '9️⃣', '🔟']
const colorEmojis = ['🔵', '🟢', '🟠', '🟣', '🔴', '🟡', '🟤', '⚫', '⚪']

export function CreatePollDialog({
  open,
  onOpenChange,
  onSuccess,
}: CreatePollDialogProps) {
  const { user } = useAuth()
  const [pollType, setPollType] = useState<PollType | null>(null)
  const [question, setQuestion] = useState('')
  const [optionsText, setOptionsText] = useState('')
  const [thisOption, setThisOption] = useState('')
  const [thatOption, setThatOption] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [settings, setSettings] = useState<SettingsResponse | null>(null)

  useEffect(() => {
    if (open) {
      const fetchSettings = async () => {
        try {
          const response = await apiClient.getSettings()
          if (response.success && response.data) {
            setSettings(response.data)
          }
        } catch (err) {
          console.error('Failed to fetch settings:', err)
        }
      }
      fetchSettings()
    }
  }, [open])

  const resetForm = () => {
    setPollType(null)
    setQuestion('')
    setOptionsText('')
    setThisOption('')
    setThatOption('')
    setError(null)
  }

  const generateOptions = (): PollOptionRequest[] => {
    if (!pollType) return []

    switch (pollType) {
      case PollType.Number: {
        const options = optionsText
          .split(',')
          .map((opt) => opt.trim())
          .filter((opt) => opt.length > 0)
        if (options.length < 2 || options.length > 10) {
          throw new Error('Number polls require between 2 and 10 options')
        }
        return options.map((answer, index) => ({
          answer,
          emojiName: numberEmojis[index],
          emojiId: 0,
        }))
      }

      case PollType.Color: {
        const options = optionsText
          .split(',')
          .map((opt) => opt.trim())
          .filter((opt) => opt.length > 0)
        if (options.length < 1 || options.length > 9) {
          throw new Error('Color polls require between 1 and 9 options')
        }
        return options.map((answer, index) => ({
          answer,
          emojiName: colorEmojis[index],
          emojiId: 0,
        }))
      }

      case PollType.YesOrNo:
        return [
          { answer: 'Yes', emojiName: '✅', emojiId: 0 },
          { answer: 'No', emojiName: '❌', emojiId: 0 },
        ]

      case PollType.ThisOrThat:
        if (!thisOption.trim() || !thatOption.trim()) {
          throw new Error('Both "This" and "That" options are required')
        }
        return [
          { answer: thisOption.trim(), emojiName: '👈', emojiId: 0 },
          { answer: thatOption.trim(), emojiName: '👉', emojiId: 0 },
        ]

      case PollType.HotTake:
        return [
          { answer: 'Agree', emojiName: '🔥', emojiId: 0 },
          { answer: 'Shit Take', emojiName: '💩', emojiId: 0 },
        ]

      default:
        return []
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    if (!user) {
      setError('You must be logged in to create a poll')
      return
    }

    if (!pollType) {
      setError('Please select a poll type')
      return
    }

    if (!question.trim()) {
      setError('Question is required')
      return
    }

    if (question.length > 128) {
      setError('Question must be 128 characters or less')
      return
    }

    try {
      const options = generateOptions()

      setLoading(true)
      const request: CreatePollRequest = {
        accountId: user.id,
        messageId: '0',
        question: question.trim(),
        type: pollType,
        options,
      }

      const response = await apiClient.createPoll(request)

      if (response.success && response.data) {
        try {
          await apiClient.postPollToPending(response.data.id)
        } catch (postError) {
          console.error('Failed to post poll to Discord:', postError)
        }
        resetForm()
        onOpenChange(false)
        onSuccess?.()
      } else {
        setError(response.message || 'Failed to create poll')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create poll')
    } finally {
      setLoading(false)
    }
  }

  const renderFormFields = () => {
    if (!pollType) return null

    switch (pollType) {
      case PollType.Number:
        return (
          <div className="space-y-2">
            <Label htmlFor="options">Options (comma-separated, 2-10 options)</Label>
            <Input
              id="options"
              value={optionsText}
              onChange={(e) => setOptionsText(e.target.value)}
              placeholder="Option 1, Option 2, Option 3..."
              disabled={loading}
            />
            <p className="text-xs text-muted-foreground">
              Separate options with commas. Example: Pizza, Burgers, Tacos
            </p>
          </div>
        )

      case PollType.Color:
        return (
          <div className="space-y-2">
            <Label htmlFor="options">Options (comma-separated, 1-9 options)</Label>
            <Input
              id="options"
              value={optionsText}
              onChange={(e) => setOptionsText(e.target.value)}
              placeholder="Red, Blue, Green..."
              disabled={loading}
            />
            <p className="text-xs text-muted-foreground">
              Separate options with commas. Example: Red, Blue, Green
            </p>
          </div>
        )

      case PollType.ThisOrThat:
        return (
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="this">This</Label>
              <Input
                id="this"
                value={thisOption}
                onChange={(e) => setThisOption(e.target.value)}
                placeholder="First option"
                maxLength={25}
                disabled={loading}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="that">That</Label>
              <Input
                id="that"
                value={thatOption}
                onChange={(e) => setThatOption(e.target.value)}
                placeholder="Second option"
                maxLength={25}
                disabled={loading}
              />
            </div>
          </div>
        )

      case PollType.YesOrNo:
      case PollType.HotTake:
        return (
          <p className="text-sm text-muted-foreground">
            Options will be automatically generated for this poll type.
          </p>
        )

      default:
        return null
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <div className="flex items-center gap-2">
            <Plus className="h-5 w-5" />
            <DialogTitle>Create Poll</DialogTitle>
          </div>
          <DialogDescription>
            {settings && (
              <span className="flex items-center gap-1">
                <Beer className="h-4 w-4" />
                Creating a poll costs {settings.pollSubmittedPenalty} 🍺. You need at least{' '}
                {settings.pollSubmittedPenalty + settings.pollDeclinedPenalty} 🍺 to create a
                poll.
              </span>
            )}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="space-y-6 py-4">
            <PollTypeSelector value={pollType} onChange={setPollType} />

            <div className="space-y-2">
              <Label htmlFor="question" className="flex items-center gap-2">
                <HelpCircle className="h-4 w-4" />
                Question
              </Label>
              <Input
                id="question"
                value={question}
                onChange={(e) => setQuestion(e.target.value)}
                placeholder="Enter your poll question"
                maxLength={128}
                disabled={loading}
                required
              />
              <p className="text-xs text-muted-foreground">
                {question.length}/128 characters
              </p>
            </div>

            {renderFormFields()}

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
              className="flex items-center gap-2"
            >
              <X className="h-4 w-4" />
              Cancel
            </Button>
            <Button type="submit" disabled={loading || !pollType || !question.trim()} className="flex items-center gap-2">
              {loading ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Creating...
                </>
              ) : (
                <>
                  <Plus className="h-4 w-4" />
                  Create Poll
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

'use client'

import { useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { apiClient } from '@/lib/api/client'
import { MessageSquare, Send, Hash, Loader2 } from 'lucide-react'

export function AdminBotPuppeting() {
  const [channelId, setChannelId] = useState('')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSendMessage = async () => {
    if (!channelId || !message) {
      alert('Please provide both channel ID and message')
      return
    }

    setLoading(true)
    try {
      const response = await apiClient.adminSendMessage({
        channelId: channelId,
        message: message,
      })
      if (response.success) {
        alert('Message sent successfully')
        setMessage('')
      } else {
        alert(`Failed to send message: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to send message:', error)
      alert(`Failed to send message: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <MessageSquare className="h-5 w-5" />
          <CardTitle>Bot Puppeting</CardTitle>
        </div>
        <CardDescription>Send messages as the bot to any Discord channel</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <Label className="flex items-center gap-2">
            <Hash className="h-4 w-4" />
            Channel ID
          </Label>
          <Input
            placeholder="Discord Channel ID"
            value={channelId}
            onChange={(e) => setChannelId(e.target.value)}
          />
        </div>
        <div>
          <Label className="flex items-center gap-2">
            <MessageSquare className="h-4 w-4" />
            Message
          </Label>
          <Input
            placeholder="Message to send"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSendMessage()}
          />
        </div>
        <Button onClick={handleSendMessage} disabled={loading || !channelId || !message} className="flex items-center gap-2">
          {loading ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin" />
              Sending...
            </>
          ) : (
            <>
              <Send className="h-4 w-4" />
              Send Message
            </>
          )}
        </Button>
        <p className="text-sm text-muted-foreground">
          Enter the Discord channel ID and message you want the bot to send.
        </p>
      </CardContent>
    </Card>
  )
}

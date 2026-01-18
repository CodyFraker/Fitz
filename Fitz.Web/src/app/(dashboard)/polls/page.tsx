'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { CreatePollDialog } from '@/components/polls/CreatePollDialog'
import { MyPollsSection } from '@/components/polls/MyPollsSection'
import { AllPollsTable } from '@/components/polls/AllPollsTable'
import { BarChart3, Plus } from 'lucide-react'

export default function PollsPage() {
  const [createDialogOpen, setCreateDialogOpen] = useState(false)
  const [refreshKey, setRefreshKey] = useState(0)

  const handlePollCreated = () => {
    setRefreshKey((prev) => prev + 1)
  }

  return (
    <main className="flex min-h-screen flex-col p-4 max-w-6xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-2">
          <BarChart3 className="h-6 w-6" />
          <h1 className="text-3xl font-bold">Polls</h1>
        </div>
        <Button onClick={() => setCreateDialogOpen(true)} className="flex items-center gap-2">
          <Plus className="h-5 w-5" />
          Create Poll
        </Button>
      </div>

      <div className="space-y-8">
        <MyPollsSection key={`my-polls-${refreshKey}`} />
        <AllPollsTable key={`all-polls-${refreshKey}`} />
      </div>

      <CreatePollDialog
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
        onSuccess={handlePollCreated}
      />
    </main>
  )
}

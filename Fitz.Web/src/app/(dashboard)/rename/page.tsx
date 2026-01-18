'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { MyRenamesSection } from '@/components/rename/MyRenamesSection'
import { AllRenamesTable } from '@/components/rename/AllRenamesTable'
import { CreateRenameDialog } from '@/components/rename/CreateRenameDialog'
import { AdminRenameManagement } from '@/components/rename/AdminRenameManagement'
import { Edit, Plus, Loader2, Shield } from 'lucide-react'

export default function RenamePage() {
  const { isAuthenticated, isAdmin, isLoading } = useAuth()
  const router = useRouter()
  const [createDialogOpen, setCreateDialogOpen] = useState(false)
  const [refreshKey, setRefreshKey] = useState(0)
  const [activeTab, setActiveTab] = useState<'my' | 'all' | 'admin'>('my')

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/login')
    }
  }, [isLoading, isAuthenticated, router])

  useEffect(() => {
    if (!isAdmin && activeTab === 'admin') {
      setActiveTab('my')
    }
  }, [isAdmin, activeTab])

  const handleRenameCreated = () => {
    setRefreshKey((prev) => prev + 1)
  }

  if (isLoading) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center p-4">
        <Loader2 className="h-6 w-6 animate-spin" />
        <p className="mt-2">Loading...</p>
      </main>
    )
  }

  return (
    <main className="flex min-h-screen flex-col p-4 max-w-6xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-2">
          <Edit className="h-6 w-6" />
          <h1 className="text-3xl font-bold">Renames</h1>
        </div>
        <Button onClick={() => setCreateDialogOpen(true)} className="flex items-center gap-2">
          <Plus className="h-5 w-5" />
          Create Rename
        </Button>
      </div>

      <div className="flex gap-2 mb-6 border-b overflow-x-auto">
        <Button
          variant={activeTab === 'my' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('my')}
          className="flex items-center gap-2 whitespace-nowrap"
        >
          <Edit className="h-4 w-4" />
          My Renames
        </Button>
        <Button
          variant={activeTab === 'all' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('all')}
          className="flex items-center gap-2 whitespace-nowrap"
        >
          <Edit className="h-4 w-4" />
          All Renames
        </Button>
        {isAdmin && (
          <Button
            variant={activeTab === 'admin' ? 'default' : 'ghost'}
            onClick={() => setActiveTab('admin')}
            className="flex items-center gap-2 whitespace-nowrap"
          >
            <Shield className="h-4 w-4" />
            Admin Management
          </Button>
        )}
      </div>

      <div className="space-y-8">
        {activeTab === 'my' && (
          <MyRenamesSection key={`my-renames-${refreshKey}`} />
        )}
        {activeTab === 'all' && (
          <AllRenamesTable key={`all-renames-${refreshKey}`} />
        )}
        {activeTab === 'admin' && isAdmin && (
          <AdminRenameManagement key={`admin-renames-${refreshKey}`} />
        )}
      </div>

      <CreateRenameDialog
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
        onSuccess={handleRenameCreated}
      />
    </main>
  )
}

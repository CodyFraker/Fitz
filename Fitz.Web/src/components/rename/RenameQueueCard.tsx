'use client'

import { RenameResponse, RenameStatus } from '@/types/api'
import { Card, CardContent } from '@/components/ui/card'
import { ArrowRight, Calendar, Coins, User } from 'lucide-react'

interface RenameQueueCardProps {
  rename: RenameResponse
  showRequestedBy?: boolean
}

const renameStatusLabels: Record<RenameStatus, string> = {
  [RenameStatus.Unknown]: 'Unknown',
  [RenameStatus.Pending]: 'Pending',
  [RenameStatus.Active]: 'Active',
  [RenameStatus.Expired]: 'Expired',
  [RenameStatus.BoughtOut]: 'Bought Out',
  [RenameStatus.Permanent]: 'Permanent',
}

const getStatusColor = (status: RenameStatus): string => {
  switch (status) {
    case RenameStatus.Active:
      return 'bg-green-100 text-green-800 border-green-300'
    case RenameStatus.Pending:
      return 'bg-yellow-100 text-yellow-800 border-yellow-300'
    case RenameStatus.Expired:
      return 'bg-gray-100 text-gray-800 border-gray-300'
    case RenameStatus.BoughtOut:
      return 'bg-blue-100 text-blue-800 border-blue-300'
    case RenameStatus.Permanent:
      return 'bg-purple-100 text-purple-800 border-purple-300'
    default:
      return 'bg-gray-100 text-gray-800 border-gray-300'
  }
}

export function RenameQueueCard({ rename, showRequestedBy = false }: RenameQueueCardProps) {
  const formatDate = (dateString?: string) => {
    if (!dateString) return 'N/A'
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardContent className="p-4">
        <div className="space-y-3">
          <div className="flex items-center gap-2 flex-wrap">
            {rename.oldName && (
              <>
                <span className="font-semibold text-lg">{rename.oldName}</span>
                <ArrowRight className="h-4 w-4 text-muted-foreground flex-shrink-0" />
              </>
            )}
            <span className="font-semibold text-lg">{rename.newName}</span>
            <span
              className={`ml-auto px-2 py-1 rounded text-xs font-medium border ${getStatusColor(rename.status)}`}
            >
              {renameStatusLabels[rename.status]}
            </span>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-sm">
            <div className="flex items-center gap-2 text-muted-foreground">
              <Coins className="h-4 w-4" />
              <span>
                Cost: {rename.cost} 🍺
              </span>
            </div>
            {rename.days && (
              <div className="flex items-center gap-2 text-muted-foreground">
                <Calendar className="h-4 w-4" />
                <span>Duration: {rename.days} day{rename.days !== 1 ? 's' : ''}</span>
              </div>
            )}
          </div>

          {(rename.startDate || rename.expiration) && (
            <div className="space-y-1 text-xs text-muted-foreground">
              {rename.startDate && (
                <div className="flex items-center gap-1">
                  <Calendar className="h-3 w-3" />
                  <span>Starts: {formatDate(rename.startDate)}</span>
                </div>
              )}
              {rename.expiration && (
                <div className="flex items-center gap-1">
                  <Calendar className="h-3 w-3" />
                  <span>Expires: {formatDate(rename.expiration)}</span>
                </div>
              )}
            </div>
          )}

          {showRequestedBy && (
            <div className="flex items-center gap-2 text-xs text-muted-foreground pt-2 border-t">
              <User className="h-3 w-3" />
              <span>Requested by: {rename.requestedUserId}</span>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  )
}

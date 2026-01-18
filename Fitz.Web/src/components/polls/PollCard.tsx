'use client'

import { PollResponse, PollType, PollStatus } from '@/types/api'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { CheckCircle2, XCircle, Clock, BarChart3, Calendar } from 'lucide-react'

interface PollCardProps {
  poll: PollResponse
  showVoteBreakdown?: boolean
}

const pollTypeLabels: Record<PollType, string> = {
  [PollType.Number]: 'Number',
  [PollType.YesOrNo]: 'Yes/No',
  [PollType.Color]: 'Color',
  [PollType.ThisOrThat]: 'This or That',
  [PollType.HotTake]: 'Hot Take',
}

const pollStatusLabels: Record<PollStatus, string> = {
  [PollStatus.Pending]: 'Pending',
  [PollStatus.Approved]: 'Approved',
  [PollStatus.Declined]: 'Declined',
}

const getStatusColor = (status: PollStatus): string => {
  switch (status) {
    case PollStatus.Approved:
      return 'text-green-600'
    case PollStatus.Declined:
      return 'text-red-600'
    case PollStatus.Pending:
      return 'text-yellow-600'
    default:
      return 'text-muted-foreground'
  }
}

export function PollCard({ poll, showVoteBreakdown = true }: PollCardProps) {
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }

  const getVotePercentage = (optionId: number): number => {
    if (!poll.optionVoteCounts || poll.totalVotes === 0) return 0
    const votes = poll.optionVoteCounts[optionId] || 0
    return (votes / poll.totalVotes) * 100
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="flex-1">
            <CardTitle className="text-lg">{poll.question}</CardTitle>
            <CardDescription className="mt-1 flex items-center gap-2">
              <span className="inline-block mr-2">
                {pollTypeLabels[poll.type]}
              </span>
              {poll.status === PollStatus.Approved && <CheckCircle2 className="h-4 w-4 text-green-600" />}
              {poll.status === PollStatus.Declined && <XCircle className="h-4 w-4 text-red-600" />}
              {poll.status === PollStatus.Pending && <Clock className="h-4 w-4 text-yellow-600" />}
              <span className={`inline-block ${getStatusColor(poll.status)}`}>
                {pollStatusLabels[poll.status]}
              </span>
            </CardDescription>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground flex items-center gap-1">
              <BarChart3 className="h-4 w-4" />
              Total Votes
            </span>
            <span className="font-semibold">{poll.totalVotes}</span>
          </div>

          {showVoteBreakdown && poll.options && poll.options.length > 0 && (
            <div className="space-y-3 pt-2 border-t">
              <h4 className="text-sm font-medium flex items-center gap-1">
                <BarChart3 className="h-4 w-4" />
                Vote Breakdown
              </h4>
              {poll.options.map((option) => {
                const votes = poll.optionVoteCounts?.[option.id] || 0
                const percentage = getVotePercentage(option.id)

                return (
                  <div key={option.id} className="space-y-1">
                    <div className="flex items-center justify-between text-sm">
                      <span className="flex items-center gap-2">
                        <span>{option.emojiName}</span>
                        <span>{option.answer}</span>
                      </span>
                      <span className="text-muted-foreground">
                        {votes} ({percentage.toFixed(1)}%)
                      </span>
                    </div>
                    <div className="w-full bg-secondary rounded-full h-2">
                      <div
                        className="bg-primary h-2 rounded-full transition-all"
                        style={{ width: `${percentage}%` }}
                      />
                    </div>
                  </div>
                )
              })}
            </div>
          )}

          <div className="text-xs text-muted-foreground pt-2 border-t flex items-center gap-2">
            <Calendar className="h-3 w-3" />
            <span>Submitted: {formatDate(poll.submittedOn)}</span>
            {poll.evaluatedOn && (
              <span className="ml-2">
                • Evaluated: {formatDate(poll.evaluatedOn)}
              </span>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

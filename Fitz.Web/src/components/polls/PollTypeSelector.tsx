'use client'

import { PollType } from '@/types/api'
import { Label } from '@/components/ui/label'

interface PollTypeSelectorProps {
  value: PollType | null
  onChange: (type: PollType) => void
}

const pollTypes = [
  { value: PollType.Number, label: 'Number', description: '2-10 custom options with number emojis' },
  { value: PollType.YesOrNo, label: 'Yes/No', description: 'Simple yes or no question' },
  { value: PollType.Color, label: 'Color', description: '1-9 options with color circle emojis' },
  { value: PollType.ThisOrThat, label: 'This or That', description: 'Two choice comparison' },
  { value: PollType.HotTake, label: 'Hot Take', description: 'Agree or disagree with a hot take' },
]

export function PollTypeSelector({ value, onChange }: PollTypeSelectorProps) {
  return (
    <div className="space-y-3">
      <Label>Poll Type</Label>
      <div className="grid gap-3 md:grid-cols-2">
        {pollTypes.map((type) => (
          <button
            key={type.value}
            type="button"
            onClick={() => onChange(type.value)}
            className={`p-4 border rounded-lg text-left transition-all hover:border-primary ${
              value === type.value
                ? 'border-primary bg-primary/5'
                : 'border-border'
            }`}
          >
            <div className="font-medium">{type.label}</div>
            <div className="text-sm text-muted-foreground mt-1">
              {type.description}
            </div>
          </button>
        ))}
      </div>
    </div>
  )
}

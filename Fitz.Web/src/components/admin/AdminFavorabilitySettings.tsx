'use client'

import { useState, useEffect } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { apiClient } from '@/lib/api/client'
import { Settings, Save, Loader2, AlertCircle } from 'lucide-react'

export function AdminFavorabilitySettings() {
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [formData, setFormData] = useState({
    favorabilityBeerRatioThreshold: '',
    favorabilityLowThreshold: '',
    favorabilityBaseDropPercent: '',
    favorabilityDropMultiplier: '',
  })
  const [originalData, setOriginalData] = useState({
    favorabilityBeerRatioThreshold: 0,
    favorabilityLowThreshold: 0,
    favorabilityBaseDropPercent: 0,
    favorabilityDropMultiplier: 0,
  })

  const fetchSettings = async () => {
    setLoading(true)
    try {
      const response = await apiClient.getSettings()
      if (response.success && response.data) {
        const settings = response.data
        setFormData({
          favorabilityBeerRatioThreshold: settings.favorabilityBeerRatioThreshold?.toString() || '2.0',
          favorabilityLowThreshold: settings.favorabilityLowThreshold?.toString() || '10',
          favorabilityBaseDropPercent: settings.favorabilityBaseDropPercent?.toString() || '1.0',
          favorabilityDropMultiplier: settings.favorabilityDropMultiplier?.toString() || '1.5',
        })
        setOriginalData({
          favorabilityBeerRatioThreshold: settings.favorabilityBeerRatioThreshold || 2.0,
          favorabilityLowThreshold: settings.favorabilityLowThreshold || 10,
          favorabilityBaseDropPercent: settings.favorabilityBaseDropPercent || 1.0,
          favorabilityDropMultiplier: settings.favorabilityDropMultiplier || 1.5,
        })
      }
    } catch (error) {
      console.error('Failed to fetch settings:', error)
      alert('Failed to fetch favorability settings')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchSettings()
  }, [])

  const handleSave = async () => {
    const settings: any = {}
    let hasChanges = false

    const beerRatioThreshold = parseFloat(formData.favorabilityBeerRatioThreshold)
    if (!isNaN(beerRatioThreshold) && beerRatioThreshold !== originalData.favorabilityBeerRatioThreshold) {
      if (beerRatioThreshold < 0.1 || beerRatioThreshold > 100) {
        alert('Beer ratio threshold must be between 0.1 and 100')
        return
      }
      settings.favorabilityBeerRatioThreshold = beerRatioThreshold
      hasChanges = true
    }

    const lowThreshold = parseInt(formData.favorabilityLowThreshold)
    if (!isNaN(lowThreshold) && lowThreshold !== originalData.favorabilityLowThreshold) {
      if (lowThreshold < 0 || lowThreshold > 100) {
        alert('Low threshold must be between 0 and 100')
        return
      }
      settings.favorabilityLowThreshold = lowThreshold
      hasChanges = true
    }

    const baseDropPercent = parseFloat(formData.favorabilityBaseDropPercent)
    if (!isNaN(baseDropPercent) && baseDropPercent !== originalData.favorabilityBaseDropPercent) {
      if (baseDropPercent < 0 || baseDropPercent > 100) {
        alert('Base drop percent must be between 0 and 100')
        return
      }
      settings.favorabilityBaseDropPercent = baseDropPercent
      hasChanges = true
    }

    const dropMultiplier = parseFloat(formData.favorabilityDropMultiplier)
    if (!isNaN(dropMultiplier) && dropMultiplier !== originalData.favorabilityDropMultiplier) {
      if (dropMultiplier < 0.1 || dropMultiplier > 10) {
        alert('Drop multiplier must be between 0.1 and 10')
        return
      }
      settings.favorabilityDropMultiplier = dropMultiplier
      hasChanges = true
    }

    if (!hasChanges) {
      alert('No changes to save')
      return
    }

    setSaving(true)
    try {
      const response = await apiClient.adminUpdateFavorabilitySettings(settings)
      if (response.success) {
        alert('Favorability settings updated successfully')
        fetchSettings()
      } else {
        alert(`Failed to update settings: ${response.message}`)
      }
    } catch (error: any) {
      console.error('Failed to update settings:', error)
      alert(`Failed to update settings: ${error.message}`)
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-center py-8">
            <Loader2 className="h-6 w-6 animate-spin" />
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-2">
          <Settings className="h-5 w-5" />
          <CardTitle>Favorability Settings</CardTitle>
        </div>
        <CardDescription>Configure favorability thresholds, drop percentages, and multipliers</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label htmlFor="beerRatioThreshold">
              Beer Ratio Threshold
              <span className="text-muted-foreground text-xs ml-2">
                (When user has this ratio or more, costs increase)
              </span>
            </Label>
            <Input
              id="beerRatioThreshold"
              type="number"
              step="0.1"
              min="0.1"
              max="100"
              value={formData.favorabilityBeerRatioThreshold}
              onChange={(e) =>
                setFormData({ ...formData, favorabilityBeerRatioThreshold: e.target.value })
              }
            />
            <p className="text-xs text-muted-foreground">
              Default: 2.0 (user has 2x bot's beer)
            </p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="lowThreshold">
              Low Favorability Threshold
              <span className="text-muted-foreground text-xs ml-2">
                (Below this, users won't receive happy hour beer)
              </span>
            </Label>
            <Input
              id="lowThreshold"
              type="number"
              min="0"
              max="100"
              value={formData.favorabilityLowThreshold}
              onChange={(e) =>
                setFormData({ ...formData, favorabilityLowThreshold: e.target.value })
              }
            />
            <p className="text-xs text-muted-foreground">Default: 10</p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="baseDropPercent">
              Base Drop Percent
              <span className="text-muted-foreground text-xs ml-2">
                (Base favorability drop percentage per command)
              </span>
            </Label>
            <Input
              id="baseDropPercent"
              type="number"
              step="0.1"
              min="0"
              max="100"
              value={formData.favorabilityBaseDropPercent}
              onChange={(e) =>
                setFormData({ ...formData, favorabilityBaseDropPercent: e.target.value })
              }
            />
            <p className="text-xs text-muted-foreground">Default: 1.0%</p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="dropMultiplier">
              Drop Multiplier
              <span className="text-muted-foreground text-xs ml-2">
                (Multiplier for favorability drop when ratio exceeds threshold)
              </span>
            </Label>
            <Input
              id="dropMultiplier"
              type="number"
              step="0.1"
              min="0.1"
              max="10"
              value={formData.favorabilityDropMultiplier}
              onChange={(e) =>
                setFormData({ ...formData, favorabilityDropMultiplier: e.target.value })
              }
            />
            <p className="text-xs text-muted-foreground">Default: 1.5x</p>
          </div>
        </div>

        <div className="flex items-center gap-2 pt-4 border-t">
          <Button
            onClick={handleSave}
            disabled={saving}
            className="flex items-center gap-2"
          >
            {saving ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                Saving...
              </>
            ) : (
              <>
                <Save className="h-4 w-4" />
                Save Settings
              </>
            )}
          </Button>
          <Button
            variant="outline"
            onClick={fetchSettings}
            disabled={saving || loading}
          >
            Reset
          </Button>
        </div>

        <div className="bg-muted p-4 rounded-md">
          <div className="flex items-start gap-2">
            <AlertCircle className="h-5 w-5 text-yellow-600 mt-0.5" />
            <div className="text-sm">
              <p className="font-semibold mb-1">How Favorability Works:</p>
              <ul className="list-disc list-inside space-y-1 text-muted-foreground">
                <li>Users with beer ratio ≥ threshold face increased command costs</li>
                <li>Favorability drops after each command that costs beer</li>
                <li>Drop percentage increases based on beer ratio and multiplier</li>
                <li>Users with 0 favorability cannot use commands</li>
                <li>Users below low threshold won't receive happy hour beer</li>
              </ul>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

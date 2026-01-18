import { useState, useEffect } from 'react'
import { apiClient } from '@/lib/api/client'
import { ApiResponse } from '@/types/api'

interface UseApiOptions<T> {
  onSuccess?: (data: T) => void
  onError?: (error: Error) => void
  enabled?: boolean
}

export function useApi<T>(
  url: string | null,
  options: UseApiOptions<T> = {}
) {
  const { onSuccess, onError, enabled = true } = options
  const [data, setData] = useState<T | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<Error | null>(null)

  useEffect(() => {
    if (!url || !enabled) return

    const fetchData = async () => {
      setLoading(true)
      setError(null)
      try {
        const response = await apiClient.get<T>(url)
        if (response.success && response.data) {
          setData(response.data)
          onSuccess?.(response.data)
        } else {
          const err = new Error(response.message || 'Request failed')
          setError(err)
          onError?.(err)
        }
      } catch (err) {
        const error = err instanceof Error ? err : new Error('Unknown error')
        setError(error)
        onError?.(error)
      } finally {
        setLoading(false)
      }
    }

    fetchData()
  }, [url, enabled, onSuccess, onError])

  return { data, loading, error, refetch: () => {} }
}

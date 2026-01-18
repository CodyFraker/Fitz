import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiClient } from './client'

describe('ApiClient', () => {
  beforeEach(() => {
    if (typeof window !== 'undefined') {
      localStorage.clear()
    }
  })

  it('sets and retrieves token', () => {
    if (typeof window === 'undefined') return

    const token = 'test-token'
    apiClient.setToken(token)
    expect(localStorage.getItem('auth_token')).toBe(token)
  })

  it('handles missing token gracefully', () => {
    if (typeof window === 'undefined') return

    localStorage.removeItem('auth_token')
    const token = apiClient.getToken()
    expect(token).toBeNull()
  })
})

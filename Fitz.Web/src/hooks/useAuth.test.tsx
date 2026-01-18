import { describe, it, expect, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { AuthProvider, useAuth } from '@/contexts/AuthContext'
import { apiClient } from '@/lib/api/client'
import { server } from '@/mocks/server'
import { http, HttpResponse } from 'msw'

describe('useAuth', () => {
  beforeEach(() => {
    if (typeof window !== 'undefined') {
    localStorage.clear()
    }
  })

  it('provides authentication context', () => {
    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AuthProvider>{children}</AuthProvider>
    )

    const { result } = renderHook(() => useAuth(), { wrapper })

    expect(result.current).toBeDefined()
    expect(result.current.isLoading).toBe(true)
    expect(result.current.isAuthenticated).toBe(false)
    expect(result.current.user).toBeNull()
  })

  it('logs in user with token', async () => {
    server.use(
      http.get('http://localhost:5000/api/auth/me', () => {
        return HttpResponse.json({
          success: true,
          message: '',
          data: { id: '123', username: 'TestUser' },
        })
      })
    )

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AuthProvider>{children}</AuthProvider>
    )

    const { result } = renderHook(() => useAuth(), { wrapper })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })

    result.current.login('test-token')

    await waitFor(() => {
      expect(result.current.isAuthenticated).toBe(true)
    })
  })

  it('logs out user', () => {
    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AuthProvider>{children}</AuthProvider>
    )

    const { result } = renderHook(() => useAuth(), { wrapper })

    if (typeof window !== 'undefined') {
      localStorage.setItem('auth_token', 'test-token')
    }

    result.current.logout()

    expect(result.current.isAuthenticated).toBe(false)
    expect(result.current.user).toBeNull()
    if (typeof window !== 'undefined') {
      expect(localStorage.getItem('auth_token')).toBeNull()
    }
  })
})

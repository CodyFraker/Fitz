'use client'

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { apiClient } from '@/lib/api/client'
import { CurrentUserResponse } from '@/types/api'

interface AuthContextType {
  user: CurrentUserResponse | null
  isLoading: boolean
  isAuthenticated: boolean
  isAdmin: boolean
  login: (token: string) => Promise<void>
  logout: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUserResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const token = typeof window !== 'undefined' ? localStorage.getItem('auth_token') : null
    if (token) {
      apiClient.setToken(token)
      refreshUser().finally(() => setIsLoading(false))
    } else {
      setIsLoading(false)
    }
  }, [])

  const refreshUser = async () => {
    try {
      const response = await apiClient.get<CurrentUserResponse>('/api/auth/me')
      if (response.success && response.data) {
        if (process.env.NODE_ENV === 'development') {
          console.log('[AuthContext] User data:', response.data)
          console.log('[AuthContext] Is Admin:', response.data.isAdmin)
        }
        setUser(response.data)
      } else {
        logout()
      }
    } catch (error) {
      console.error('[AuthContext] Failed to refresh user:', error)
      logout()
    }
  }

  const login = async (token: string) => {
    if (process.env.NODE_ENV === 'development') {
      console.log('[AuthContext] Login called with token:', token ? `${token.substring(0, 10)}...` : 'null')
    }
    apiClient.setToken(token)
    await refreshUser()
  }

  const logout = () => {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('auth_token')
    }
    setUser(null)
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: !!user,
        isAdmin: user?.isAdmin ?? false,
        login,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

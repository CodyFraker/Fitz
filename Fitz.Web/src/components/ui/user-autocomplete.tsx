'use client'

import { useState, useEffect, useRef, useCallback } from 'react'
import { Input } from './input'
import { apiClient } from '@/lib/api/client'
import { UserResponse } from '@/types/api'
import { cn } from '@/lib/utils'

interface UserAutocompleteProps {
  value: string
  onChange: (value: string) => void
  onKeyDown?: (e: React.KeyboardEvent<HTMLInputElement>) => void
  placeholder?: string
  disabled?: boolean
  className?: string
}

export function UserAutocomplete({
  value,
  onChange,
  onKeyDown,
  placeholder = 'User ID (Discord ID)',
  disabled = false,
  className,
}: UserAutocompleteProps) {
  const [query, setQuery] = useState(value || '')
  const [suggestions, setSuggestions] = useState<UserResponse[]>([])
  const [isOpen, setIsOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const [selectedIndex, setSelectedIndex] = useState(-1)
  const containerRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const debounceTimerRef = useRef<NodeJS.Timeout>()

  const fetchUsers = useCallback(async (searchQuery: string) => {
    if (!searchQuery || searchQuery.trim().length < 2) {
      setSuggestions([])
      setIsOpen(false)
      return
    }

    setLoading(true)
    try {
      const response = await apiClient.getUsers({
        query: searchQuery,
        page: 1,
        pageSize: 5,
      })

      if (response.success && response.data?.users) {
        setSuggestions(response.data.users)
        setIsOpen(response.data.users.length > 0)
        setSelectedIndex(-1)
      } else {
        setSuggestions([])
        setIsOpen(false)
      }
    } catch (error) {
      console.error('Failed to fetch users:', error)
      setSuggestions([])
      setIsOpen(false)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current)
    }

    debounceTimerRef.current = setTimeout(() => {
      fetchUsers(query)
    }, 300)

    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current)
      }
    }
  }, [query, fetchUsers])

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [])

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value
    setQuery(newValue)
    onChange(newValue)
    if (newValue.length < 2) {
      setIsOpen(false)
    }
  }

  const handleSelectUser = (user: UserResponse) => {
    const userId = user.id
    setQuery(userId)
    onChange(userId)
    setIsOpen(false)
    setSelectedIndex(-1)
    inputRef.current?.blur()
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (!isOpen || suggestions.length === 0) {
      onKeyDown?.(e)
      return
    }

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault()
        setSelectedIndex((prev) => (prev < suggestions.length - 1 ? prev + 1 : prev))
        break
      case 'ArrowUp':
        e.preventDefault()
        setSelectedIndex((prev) => (prev > 0 ? prev - 1 : -1))
        break
      case 'Enter':
        e.preventDefault()
        if (selectedIndex >= 0 && selectedIndex < suggestions.length) {
          handleSelectUser(suggestions[selectedIndex])
        } else {
          onKeyDown?.(e)
        }
        break
      case 'Escape':
        e.preventDefault()
        setIsOpen(false)
        setSelectedIndex(-1)
        break
      default:
        onKeyDown?.(e)
    }
  }

  const displayValue = value

  return (
    <div ref={containerRef} className={cn('relative w-full', className)}>
      <Input
        ref={inputRef}
        placeholder={placeholder}
        value={displayValue}
        onChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onFocus={() => {
          if (suggestions.length > 0) {
            setIsOpen(true)
          }
        }}
        disabled={disabled}
        className="min-h-[44px]"
      />
      {isOpen && suggestions.length > 0 && (
        <div className="absolute z-50 w-full mt-1 bg-popover border border-border rounded-md shadow-lg max-h-[220px] overflow-y-auto overscroll-contain">
          {loading && (
            <div className="px-4 py-3 text-sm text-muted-foreground min-h-[44px] flex items-center">
              Loading...
            </div>
          )}
          {!loading && suggestions.map((user, index) => (
            <button
              key={user.id}
              type="button"
              onClick={() => handleSelectUser(user)}
              className={cn(
                'w-full text-left px-4 py-3 text-sm min-h-[44px] flex flex-col justify-center',
                'active:bg-accent active:text-accent-foreground',
                'hover:bg-accent hover:text-accent-foreground',
                'focus:bg-accent focus:text-accent-foreground focus:outline-none',
                'transition-colors touch-manipulation',
                index === selectedIndex && 'bg-accent text-accent-foreground'
              )}
            >
              <div className="font-medium truncate">{user.username || 'Unknown'}</div>
              <div className="text-xs text-muted-foreground truncate">ID: {user.id}</div>
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

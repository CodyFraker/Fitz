'use client'

import Link from 'next/link'
import { useAuth } from '@/contexts/AuthContext'
import { Button } from '@/components/ui/button'
import { User, Refrigerator, BarChart3, Shield, LogOut, LogIn, Tag, Ticket } from 'lucide-react'

export function Header() {
  const { user, logout, isAuthenticated, isAdmin } = useAuth()

  return (
    <header className="border-b">
      <div className="container mx-auto px-4 py-4 flex items-center justify-between">
        <Link href="/" className="text-xl font-bold">
          Fitz Bot
        </Link>
        <nav className="flex items-center gap-4">
          {isAuthenticated ? (
            <>
              <Link href="/account" className="text-sm hover:underline flex items-center gap-1">
                <User className="h-4 w-4" />
                Account
              </Link>
              <Link href="/bank" className="text-sm hover:underline flex items-center gap-1">
                <Refrigerator className="h-4 w-4" />
                Fridge
              </Link>
              <Link href="/polls" className="text-sm hover:underline flex items-center gap-1">
                <BarChart3 className="h-4 w-4" />
                Polls
              </Link>
              <Link href="/rename" className="text-sm hover:underline flex items-center gap-1">
                <Tag className="h-4 w-4" />
                Renames
              </Link>
              <Link href="/lottery" className="text-sm hover:underline flex items-center gap-1">
                <Ticket className="h-4 w-4" />
                Lottery
              </Link>
              {isAdmin && (
                <Link href="/admin" className="text-sm hover:underline text-red-600 font-semibold flex items-center gap-1">
                  <Shield className="h-4 w-4" />
                  Admin
                </Link>
              )}
              <span className="text-sm text-muted-foreground hidden sm:inline">{user?.username}</span>
              <Button variant="ghost" size="sm" onClick={logout} className="flex items-center gap-1">
                <LogOut className="h-4 w-4" />
                Logout
              </Button>
            </>
          ) : (
            <Link href="/login">
              <Button size="sm" className="flex items-center gap-1">
                <LogIn className="h-4 w-4" />
                Login
              </Button>
            </Link>
          )}
        </nav>
      </div>
    </header>
  )
}

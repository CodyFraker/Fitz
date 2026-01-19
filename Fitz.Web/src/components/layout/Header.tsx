'use client'

import { useState } from 'react'
import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { useAuth } from '@/contexts/AuthContext'
import { Button } from '@/components/ui/button'
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet'
import { User, Refrigerator, BarChart3, Shield, LogOut, LogIn, Tag, Ticket, Menu } from 'lucide-react'

function NavigationLinks({ onLinkClick }: { onLinkClick?: () => void }) {
  const { isAdmin } = useAuth()
  const pathname = usePathname()

  const links = [
    { href: '/account', icon: User, label: 'Account' },
    { href: '/bank', icon: Refrigerator, label: 'Fridge' },
    { href: '/polls', icon: BarChart3, label: 'Polls' },
    { href: '/rename', icon: Tag, label: 'Renames' },
    { href: '/lottery', icon: Ticket, label: 'Lottery' },
  ]

  if (isAdmin) {
    links.push({ href: '/admin', icon: Shield, label: 'Admin' })
  }

  return (
    <>
      {links.map((link) => {
        const Icon = link.icon
        const isActive = pathname === link.href
        return (
          <Link
            key={link.href}
            href={link.href}
            onClick={onLinkClick}
            className={`text-sm hover:underline flex items-center gap-2 p-2 rounded-md transition-colors ${
              isActive ? 'bg-accent' : ''
            } ${link.href === '/admin' ? 'text-red-600 font-semibold' : ''}`}
          >
            <Icon className="h-4 w-4" />
            {link.label}
          </Link>
        )
      })}
    </>
  )
}

export function Header() {
  const { user, logout, isAuthenticated, isAdmin } = useAuth()
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)

  const handleLinkClick = () => {
    setIsSidebarOpen(false)
  }

  return (
    <header className="border-b">
      <div className="container mx-auto px-4 py-4 flex items-center justify-between">
        <Link href="/" className="text-xl font-bold">
          Fitz Bot
        </Link>
        <nav className="flex items-center gap-4">
          {isAuthenticated ? (
            <>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setIsSidebarOpen(true)}
                className="md:hidden flex items-center gap-1"
              >
                <Menu className="h-5 w-5" />
              </Button>
              <div className="hidden md:flex items-center gap-4">
                <NavigationLinks />
              </div>
              <span className="text-sm text-muted-foreground hidden sm:inline">{user?.username}</span>
              <Button variant="ghost" size="sm" onClick={logout} className="flex items-center gap-1">
                <LogOut className="h-4 w-4" />
                <span className="hidden sm:inline">Logout</span>
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
      {isAuthenticated && (
        <Sheet open={isSidebarOpen} onOpenChange={setIsSidebarOpen}>
          <SheetContent className="w-72">
            <SheetHeader>
              <SheetTitle>Navigation</SheetTitle>
            </SheetHeader>
            <nav className="flex flex-col gap-2 mt-6">
              <NavigationLinks onLinkClick={handleLinkClick} />
              <div className="border-t pt-4 mt-4">
                <div className="px-2 py-2 text-sm text-muted-foreground">
                  {user?.username}
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    handleLinkClick()
                    logout()
                  }}
                  className="w-full justify-start flex items-center gap-2"
                >
                  <LogOut className="h-4 w-4" />
                  Logout
                </Button>
              </div>
            </nav>
          </SheetContent>
        </Sheet>
      )}
    </header>
  )
}

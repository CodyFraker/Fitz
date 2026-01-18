import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@/test-utils/test-utils'
import LoginPage from './page'

describe('LoginPage', () => {
  it('renders login button', () => {
    render(<LoginPage />)
    const loginButton = screen.getByRole('button', { name: /continue with discord/i })
    expect(loginButton).toBeInTheDocument()
  })

  it('redirects to Discord OAuth on button click', () => {
    const originalLocation = window.location
    delete (window as any).location
    window.location = { href: '' } as any

    render(<LoginPage />)
    const loginButton = screen.getByRole('button', { name: /continue with discord/i })
    loginButton.click()

    expect(window.location.href).toContain('discord.com/api/oauth2/authorize')

    window.location = originalLocation
  })
})

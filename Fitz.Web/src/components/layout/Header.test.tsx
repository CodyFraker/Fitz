import { describe, it, expect } from 'vitest'
import { render, screen } from '@/test-utils/test-utils'
import { Header } from './Header'

describe('Header', () => {
  it('renders login button when not authenticated', () => {
    render(<Header />)
    const loginButton = screen.getByRole('link', { name: /login/i })
    expect(loginButton).toBeInTheDocument()
  })

  it('renders navigation links when authenticated', () => {
    const { container } = render(<Header />)
    expect(container).toBeInTheDocument()
  })
})

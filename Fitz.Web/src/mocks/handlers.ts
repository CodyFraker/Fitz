import { http, HttpResponse } from 'msw'
import { mockUser, mockBalance, mockAccount } from '@/test-utils/mocks'

export const handlers = [
  http.post('http://localhost:5000/api/auth/exchange-token', () => {
    return HttpResponse.json({
      success: true,
      message: '',
      data: {
        accessToken: 'mock-access-token',
        tokenType: 'Bearer',
        expiresIn: 3600,
        refreshToken: 'mock-refresh-token',
        scope: 'identify',
      },
    })
  }),

  http.get('http://localhost:5000/api/auth/me', ({ request }) => {
    const authHeader = request.headers.get('Authorization')
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return HttpResponse.json(
        { success: false, message: 'Unauthorized' },
        { status: 401 }
      )
    }
    return HttpResponse.json({
      success: true,
      message: '',
      data: mockUser,
    })
  }),

  http.get('http://localhost:5000/api/bank/balance/:userId', ({ params, request }) => {
    const authHeader = request.headers.get('Authorization')
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return HttpResponse.json(
        { success: false, message: 'Unauthorized' },
        { status: 401 }
      )
    }
    return HttpResponse.json({
      success: true,
      message: '',
      data: mockBalance,
    })
  }),

  http.get('http://localhost:5000/api/account/:userId', ({ request }) => {
    const authHeader = request.headers.get('Authorization')
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return HttpResponse.json(
        { success: false, message: 'Unauthorized' },
        { status: 401 }
      )
    }
    return HttpResponse.json({
      success: true,
      message: '',
      data: mockAccount,
    })
  }),

  http.get('http://localhost:5000/api/polls', ({ request }) => {
    const authHeader = request.headers.get('Authorization')
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return HttpResponse.json(
        { success: false, message: 'Unauthorized' },
        { status: 401 }
      )
    }
    return HttpResponse.json({
      success: true,
      message: '',
      data: [],
    })
  }),
]

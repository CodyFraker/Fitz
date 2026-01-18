import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios'
import { ApiResponse } from '@/types/api'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001'

class ApiClient {
  private client: AxiosInstance

  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    this.setupInterceptors()
  }

  private setupInterceptors() {
    this.client.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        const token = this.getToken()
        if (token && config.headers) {
          config.headers.Authorization = `Bearer ${token}`
          if (process.env.NODE_ENV === 'development') {
            console.log('[API Client] Adding Authorization header for request to:', config.url)
          }
        } else {
          if (process.env.NODE_ENV === 'development') {
            console.warn('[API Client] No token found for request to:', config.url)
          }
        }
        return config
      },
      (error) => {
        return Promise.reject(error)
      }
    )

    this.client.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        if (error.response?.status === 401) {
          this.clearToken()
          if (typeof window !== 'undefined') {
            window.location.href = '/login'
          }
        }
        return Promise.reject(this.transformError(error))
      }
    )
  }

  getToken(): string | null {
    if (typeof window === 'undefined') return null
    return localStorage.getItem('auth_token')
  }

  private clearToken(): void {
    if (typeof window === 'undefined') return
    localStorage.removeItem('auth_token')
  }

  setToken(token: string): void {
    if (typeof window === 'undefined') return
    localStorage.setItem('auth_token', token)
    if (process.env.NODE_ENV === 'development') {
      console.log('[API Client] Token set in localStorage')
    }
  }

  private transformError(error: AxiosError): Error {
    if (error.response?.data) {
      const apiError = error.response.data as any
      if (apiError.message) {
        return new Error(apiError.message)
      }
      if (typeof apiError === 'string') {
        return new Error(apiError)
      }
    }
    return error instanceof Error ? error : new Error('An error occurred')
  }

  async get<T>(url: string, config?: any): Promise<ApiResponse<T>> {
    const response = await this.client.get<ApiResponse<T>>(url, config)
    return response.data
  }

  async post<T>(url: string, data?: any, config?: any): Promise<ApiResponse<T>> {
    const response = await this.client.post<ApiResponse<T>>(url, data, config)
    return response.data
  }

  async postRaw<T>(url: string, data?: any, config?: any): Promise<T> {
    const response = await this.client.post<T>(url, data, config)
    return response.data
  }

  async put<T>(url: string, data?: any, config?: any): Promise<ApiResponse<T>> {
    const response = await this.client.put<ApiResponse<T>>(url, data, config)
    return response.data
  }

  async patch<T>(url: string, data?: any, config?: any): Promise<ApiResponse<T>> {
    const response = await this.client.patch<ApiResponse<T>>(url, data, config)
    return response.data
  }

  async delete<T>(url: string, config?: any): Promise<ApiResponse<T>> {
    const response = await this.client.delete<ApiResponse<T>>(url, config)
    return response.data
  }

  async getPollsWithDetails(params: {
    status?: number
    userId?: string
    skip?: number
    take?: number
    sortBy?: string
    sortOrder?: string
  }): Promise<ApiResponse<any>> {
    const queryParams = new URLSearchParams()
    if (params.status !== undefined) queryParams.append('status', params.status.toString())
    if (params.userId) queryParams.append('userId', params.userId)
    if (params.skip !== undefined) queryParams.append('skip', params.skip.toString())
    if (params.take !== undefined) queryParams.append('take', params.take.toString())
    if (params.sortBy) queryParams.append('sortBy', params.sortBy)
    if (params.sortOrder) queryParams.append('sortOrder', params.sortOrder)

    const url = `/api/polls/with-details${queryParams.toString() ? `?${queryParams.toString()}` : ''}`
    return this.get(url)
  }

  async getUserPolls(): Promise<ApiResponse<any[]>> {
    return this.get('/api/polls/my-polls')
  }

  async createPoll(request: any): Promise<ApiResponse<any>> {
    return this.post('/api/polls', request)
  }

  async postPollToPending(pollId: number): Promise<ApiResponse<any>> {
    return this.post(`/api/polls/${pollId}/post-to-pending`)
  }

  async getSettings(): Promise<ApiResponse<any>> {
    return this.get('/api/settings')
  }

  async getCurrentLottery(): Promise<ApiResponse<any>> {
    return this.get('/api/lottery/current')
  }

  async getLotteryHistory(skip: number = 0, take: number = 10): Promise<ApiResponse<any>> {
    const queryParams = new URLSearchParams()
    queryParams.append('skip', skip.toString())
    queryParams.append('take', take.toString())
    const url = `/api/lottery/history?${queryParams.toString()}`
    return this.get(url)
  }

  async getLotteryStatistics(): Promise<ApiResponse<any>> {
    return this.get('/api/lottery/statistics')
  }

  async adminModifyAccount(userId: string, request: any): Promise<ApiResponse<any>> {
    return this.patch(`/api/admin/accounts/${userId}`, request)
  }

  async adminDeletePoll(pollId: number): Promise<ApiResponse<any>> {
    return this.delete(`/api/admin/polls/${pollId}`)
  }

  async adminEvaluatePoll(pollId: number, status: number): Promise<ApiResponse<any>> {
    return this.patch(`/api/polls/${pollId}/evaluate`, { status })
  }

  async adminCreateLottery(request: any): Promise<ApiResponse<any>> {
    return this.post('/api/admin/lottery', request)
  }

  async adminCancelLottery(): Promise<ApiResponse<any>> {
    return this.delete('/api/admin/lottery/current')
  }

  async adminExtendLotteryEndDate(endDate: string): Promise<ApiResponse<any>> {
    return this.patch('/api/admin/lottery/current/end-date', { endDate })
  }

  async adminModifyLotteryPool(pool: number): Promise<ApiResponse<any>> {
    return this.patch('/api/admin/lottery/current/pool', { pool })
  }

  async adminEndLottery(): Promise<ApiResponse<any>> {
    return this.post('/api/admin/lottery/current/end')
  }

  async adminBuyFitzTickets(tickets: number): Promise<ApiResponse<any>> {
    return this.post('/api/admin/lottery/current/fitz-tickets', { tickets })
  }

  async adminSendMessage(request: any): Promise<ApiResponse<any>> {
    return this.post('/api/admin/bot/send-message', request)
  }

  async getAccount(userId: string): Promise<ApiResponse<any>> {
    return this.get(`/api/account/${userId}`)
  }

  async getPolls(params?: { status?: number; userId?: string }): Promise<ApiResponse<any[]>> {
    const queryParams = new URLSearchParams()
    if (params?.status !== undefined) queryParams.append('status', params.status.toString())
    if (params?.userId) queryParams.append('userId', params.userId)
    const url = `/api/polls${queryParams.toString() ? `?${queryParams.toString()}` : ''}`
    return this.get(url)
  }

  async getUsers(params?: { query?: string; page?: number; pageSize?: number }): Promise<ApiResponse<any>> {
    const queryParams = new URLSearchParams()
    if (params?.query) queryParams.append('query', params.query)
    if (params?.page !== undefined) queryParams.append('page', params.page.toString())
    if (params?.pageSize !== undefined) queryParams.append('pageSize', params.pageSize.toString())
    const url = `/api/users${queryParams.toString() ? `?${queryParams.toString()}` : ''}`
    return this.get(url)
  }

  async getRenames(status?: number): Promise<ApiResponse<any[]>> {
    const queryParams = new URLSearchParams()
    if (status !== undefined) queryParams.append('status', status.toString())
    const url = `/api/rename${queryParams.toString() ? `?${queryParams.toString()}` : ''}`
    return this.get(url)
  }

  async getRenamesByUser(userId: string): Promise<ApiResponse<any[]>> {
    return this.get(`/api/rename/user/${userId}`)
  }

  async getRename(id: number): Promise<ApiResponse<any>> {
    return this.get(`/api/rename/${id}`)
  }

  async calculateRenameCost(request: any): Promise<ApiResponse<any>> {
    return this.post('/api/rename/calculate-cost', request)
  }

  async createRename(request: any): Promise<ApiResponse<any>> {
    return this.post('/api/rename', request)
  }

  async buyoutRenames(userId: string): Promise<ApiResponse<any>> {
    return this.post(`/api/rename/user/${userId}/buyout`)
  }

  async updateRenameStatus(id: number, status: number): Promise<ApiResponse<any>> {
    return this.patch(`/api/rename/${id}/status`, { status })
  }

  async setLotterySubscribe(userId: string, subscribe: boolean): Promise<ApiResponse<any>> {
    return this.post('/api/account/lottery-subscribe', {
      userId,
      subscribe,
    })
  }

  async setSafeBalance(userId: string, safeBalance: number): Promise<ApiResponse<any>> {
    return this.post('/api/account/safe-balance', {
      userId,
      safeBalance,
    })
  }

  async setTicketAmount(userId: string, amount: number): Promise<ApiResponse<any>> {
    return this.post('/api/account/ticket-amount', {
      userId,
      amount,
    })
  }

  async getUsersWithFavorability(params?: {
    query?: string
    skip?: number
    take?: number
    sortBy?: string
    sortOrder?: string
  }): Promise<ApiResponse<any>> {
    const queryParams = new URLSearchParams()
    if (params?.query) queryParams.append('query', params.query)
    if (params?.skip !== undefined) queryParams.append('skip', params.skip.toString())
    if (params?.take !== undefined) queryParams.append('take', params.take.toString())
    if (params?.sortBy) queryParams.append('sortBy', params.sortBy)
    if (params?.sortOrder) queryParams.append('sortOrder', params.sortOrder)
    const url = `/api/admin/favorability/users${queryParams.toString() ? `?${queryParams.toString()}` : ''}`
    return this.get(url)
  }

  async adminUpdateFavorability(userId: string, favorability: number): Promise<ApiResponse<any>> {
    return this.patch(`/api/admin/favorability/users/${userId}`, { favorability })
  }

  async adminBulkUpdateFavorability(userIds: string[], favorability: number): Promise<ApiResponse<any>> {
    return this.post('/api/admin/favorability/users/bulk', { userIds, favorability })
  }

  async adminUpdateFavorabilitySettings(settings: {
    favorabilityBeerRatioThreshold?: number
    favorabilityLowThreshold?: number
    favorabilityBaseDropPercent?: number
    favorabilityDropMultiplier?: number
  }): Promise<ApiResponse<any>> {
    const request: any = {}
    if (settings.favorabilityBeerRatioThreshold !== undefined) {
      request.favorabilityBeerRatioThreshold = settings.favorabilityBeerRatioThreshold
    }
    if (settings.favorabilityLowThreshold !== undefined) {
      request.favorabilityLowThreshold = settings.favorabilityLowThreshold
    }
    if (settings.favorabilityBaseDropPercent !== undefined) {
      request.favorabilityBaseDropPercent = settings.favorabilityBaseDropPercent
    }
    if (settings.favorabilityDropMultiplier !== undefined) {
      request.favorabilityDropMultiplier = settings.favorabilityDropMultiplier
    }
    return this.patch('/api/admin/favorability/settings', request)
  }
}

export const apiClient = new ApiClient()

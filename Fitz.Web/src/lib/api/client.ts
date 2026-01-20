import axios, { AxiosInstance, AxiosError, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { ApiResponse } from '@/types/api'
import {
  ApiClient as GeneratedApiClient,
  PollStatusEnum,
  RenameStatusEnum,
  AdminModifyLotteryPoolRequest,
  AdminBuyFitzTicketsRequest,
  SetLotterySubscribeRequest,
  SetSafeBalanceRequest,
  SetTicketAmountRequest,
  UpdateFavorabilityRequest,
  BulkUpdateFavorabilityRequest,
  UpdateFavorabilitySettingsRequest,
  AdminExtendLotteryEndDateRequest,
  UpdateRenameStatusRequestDto,
  EvaluatePollRequestDto,
} from './generated/src/lib/api/generated/api-client'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'

function numberToPollStatus(status: number): PollStatusEnum {
  switch (status) {
    case 1:
      return PollStatusEnum.Pending
    case 2:
      return PollStatusEnum.Approved
    case 3:
      return PollStatusEnum.Declined
    default:
      return PollStatusEnum.Pending
  }
}

function numberToRenameStatus(status: number): RenameStatusEnum {
  switch (status) {
    case 0:
      return RenameStatusEnum.Unknown
    case 1:
      return RenameStatusEnum.Pending
    case 2:
      return RenameStatusEnum.Active
    case 3:
      return RenameStatusEnum.Expired
    case 4:
      return RenameStatusEnum.BoughtOut
    case 5:
      return RenameStatusEnum.Permanent
    default:
      return RenameStatusEnum.Unknown
  }
}

class ApiClient {
  private generatedClient: GeneratedApiClient
  private axiosInstance: AxiosInstance
  private responseDataMap: Map<string, any> = new Map()
  private requestCounter: number = 0

  constructor() {
    this.axiosInstance = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    this.setupInterceptors()
    this.generatedClient = new GeneratedApiClient(API_BASE_URL, this.axiosInstance)
  }

  private setupInterceptors() {
    this.axiosInstance.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        const token = this.getToken()
        if (token && config.headers) {
          config.headers.Authorization = `Bearer ${token}`
        }
        const requestId = `${config.method}-${config.url}-${this.requestCounter++}`
        ;(config as any).__requestId = requestId
        return config
      },
      (error) => {
        return Promise.reject(error)
      }
    )

    this.axiosInstance.interceptors.response.use(
      (response: AxiosResponse) => {
        const requestId = (response.config as any).__requestId
        if (requestId) {
          this.responseDataMap.set(requestId, response.data)
          setTimeout(() => this.responseDataMap.delete(requestId), 1000)
        }
        return response
      },
      async (error: AxiosError) => {
        if (error.response?.status === 401) {
          this.clearToken()
          if (typeof window !== 'undefined') {
            window.location.href = '/login'
          }
        }
        return Promise.reject(error)
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
  }

  private async wrapGeneratedCall<T>(
    promise: Promise<void>
  ): Promise<ApiResponse<T>> {
    try {
      await promise
      const entries = Array.from(this.responseDataMap.entries())
      let data: any = null
      if (entries.length > 0) {
        data = entries[entries.length - 1][1]
        this.responseDataMap.delete(entries[entries.length - 1][0])
      }
      if (data && typeof data === 'object' && 'success' in data) {
        return data as ApiResponse<T>
      }
      return { success: true, message: '', data: data as T }
    } catch (error: any) {
      if (error.response?.data) {
        const errorData = error.response.data
        if (errorData && typeof errorData === 'object' && 'success' in errorData) {
          return errorData as ApiResponse<T>
        }
        return {
          success: false,
          message: errorData?.message || error.message || 'An error occurred',
          data: undefined,
        }
      }
      return {
        success: false,
        message: error.message || 'An error occurred',
        data: undefined,
      }
    }
  }

  async get<T>(url: string, config?: any): Promise<ApiResponse<T>> {
    const response = await this.axiosInstance.get<ApiResponse<T>>(url, config)
    return response.data
  }

  async post<T>(url: string, data?: any, config?: any): Promise<ApiResponse<T>> {
    const response = await this.axiosInstance.post<ApiResponse<T>>(url, data, config)
    return response.data
  }

  async postRaw<T>(url: string, data?: any, config?: any): Promise<T> {
    const response = await this.axiosInstance.post<T>(url, data, config)
    return response.data
  }

  async put<T>(url: string, data?: any, config?: any): Promise<ApiResponse<T>> {
    const response = await this.axiosInstance.put<ApiResponse<T>>(url, data, config)
    return response.data
  }

  async patch<T>(url: string, data?: any, config?: any): Promise<ApiResponse<T>> {
    const response = await this.axiosInstance.patch<ApiResponse<T>>(url, data, config)
    return response.data
  }

  async delete<T>(url: string, config?: any): Promise<ApiResponse<T>> {
    const response = await this.axiosInstance.delete<ApiResponse<T>>(url, config)
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
    return this.wrapGeneratedCall(
      this.generatedClient.withDetails(
        params.status !== undefined ? numberToPollStatus(params.status) : undefined,
        params.userId ? parseInt(params.userId) : undefined,
        params.skip,
        params.take,
        params.sortBy,
        params.sortOrder
      )
    )
  }

  async getUserPolls(): Promise<ApiResponse<any[]>> {
    return this.wrapGeneratedCall(this.generatedClient.myPolls())
  }

  async createPoll(request: any): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.pollsPOST(request))
  }

  async postPollToPending(pollId: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.postToPending(pollId))
  }

  async getSettings(): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.settingsGET())
  }

  async getCurrentLottery(): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.currentGET())
  }

  async getLotteryHistory(skip: number = 0, take: number = 10): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.history(skip, take))
  }

  async getLotteryStatistics(): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.statistics())
  }

  async adminModifyAccount(userId: string, request: any): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.accounts(parseInt(userId), request)
    )
  }

  async adminDeletePoll(pollId: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.pollsDELETE(pollId))
  }

  async adminEvaluatePoll(pollId: number, status: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.evaluate(pollId, new EvaluatePollRequestDto({ status: numberToPollStatus(status) }))
    )
  }

  async adminCreateLottery(request: any): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.lottery(request))
  }

  async adminCancelLottery(): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.currentDELETE())
  }

  async adminExtendLotteryEndDate(endDate: string): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.endDate(new AdminExtendLotteryEndDateRequest({ endDate: new Date(endDate) }))
    )
  }

  async adminModifyLotteryPool(pool: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.pool(new AdminModifyLotteryPoolRequest({ pool }))
    )
  }

  async adminEndLottery(): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.end())
  }

  async adminBuyFitzTickets(tickets: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.fitzTickets(new AdminBuyFitzTicketsRequest({ tickets }))
    )
  }

  async adminSendMessage(request: any): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.sendMessage(request))
  }

  async getAccount(userId: string): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.accountGET(parseInt(userId)))
  }

  async getPolls(params?: { status?: number; userId?: string }): Promise<ApiResponse<any[]>> {
    return this.wrapGeneratedCall(
      this.generatedClient.pollsGET(
        params?.status !== undefined ? numberToPollStatus(params.status) : undefined,
        params?.userId ? parseInt(params.userId) : undefined
      )
    )
  }

  async getUsers(params?: { query?: string; page?: number; pageSize?: number }): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.usersGET(params?.query, params?.page, params?.pageSize)
    )
  }

  async getRenames(status?: number): Promise<ApiResponse<any[]>> {
    return this.wrapGeneratedCall(
      this.generatedClient.renameGET(status !== undefined ? numberToRenameStatus(status) : undefined)
    )
  }

  async getRenamesByUser(userId: string): Promise<ApiResponse<any[]>> {
    return this.wrapGeneratedCall(this.generatedClient.user(parseInt(userId)))
  }

  async getRename(id: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.renameGET2(id))
  }

  async calculateRenameCost(request: any): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.calculateCost(request))
  }

  async createRename(request: any): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.renamePOST(request))
  }

  async buyoutRenames(userId: string): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(this.generatedClient.buyout(parseInt(userId)))
  }

  async updateRenameStatus(id: number, status: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.status(id, new UpdateRenameStatusRequestDto({ status: numberToRenameStatus(status) }))
    )
  }

  async setLotterySubscribe(userId: string, subscribe: boolean): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.lotterySubscribe(
        new SetLotterySubscribeRequest({ userId: parseInt(userId), subscribe })
      )
    )
  }

  async setSafeBalance(userId: string, safeBalance: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.safeBalance(new SetSafeBalanceRequest({ userId: parseInt(userId), safeBalance }))
    )
  }

  async setTicketAmount(userId: string, amount: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.ticketAmount(new SetTicketAmountRequest({ userId: parseInt(userId), amount }))
    )
  }

  async getUsersWithFavorability(params?: {
    query?: string
    skip?: number
    take?: number
    sortBy?: string
    sortOrder?: string
  }): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.usersGET2(
        params?.query,
        params?.skip,
        params?.take,
        params?.sortBy,
        params?.sortOrder
      )
    )
  }

  async adminUpdateFavorability(userId: string, favorability: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.usersPATCH(parseInt(userId), new UpdateFavorabilityRequest({ favorability }))
    )
  }

  async adminBulkUpdateFavorability(userIds: string[], favorability: number): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.bulk(
        new BulkUpdateFavorabilityRequest({ userIds: userIds.map((id) => parseInt(id)), favorability })
      )
    )
  }

  async adminUpdateFavorabilitySettings(settings: {
    favorabilityBeerRatioThreshold?: number
    favorabilityLowThreshold?: number
    favorabilityBaseDropPercent?: number
    favorabilityDropMultiplier?: number
  }): Promise<ApiResponse<any>> {
    return this.wrapGeneratedCall(
      this.generatedClient.settingsPATCH(new UpdateFavorabilitySettingsRequest(settings))
    )
  }
}

export const apiClient = new ApiClient()

import { CurrentUserResponse, BalanceResponse, AccountResponse } from '@/types/api'

export const mockUser: CurrentUserResponse = {
  id: '123456789',
  username: 'TestUser',
  isAdmin: false,
}

export const mockBalance: BalanceResponse = {
  beer: 1000,
  lifetimeBeer: 5000,
}

export const mockAccount: AccountResponse = {
  id: '123456789',
  username: 'TestUser',
  beer: 1000,
  lifetimeBeer: 5000,
  safeBalance: 500,
  favorability: 75,
  createdDate: '2024-01-01T00:00:00Z',
  subscribeToLottery: false,
  subscribeTickets: 0,
  deactivated: false,
}

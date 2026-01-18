export interface ApiResponse<T> {
  success: boolean
  message: string
  data?: T
}

export interface AuthTokenResponse {
  accessToken: string
  tokenType: string
  expiresIn: number
  refreshToken?: string
  scope: string
}

export interface CurrentUserResponse {
  id: string
  username: string
  isAdmin: boolean
}

export interface BalanceResponse {
  beer: number
  lifetimeBeer: number
}

export interface AccountResponse {
  id: string
  username?: string
  beer: number
  lifetimeBeer: number
  safeBalance: number
  favorability: number
  createdDate: string
  subscribeToLottery: boolean
  subscribeTickets: number
  deactivated: boolean
}

export interface AccountBalanceResponse {
  id: string
  username?: string
  beer: number
}

export interface BalancesResponse {
  accounts: AccountBalanceResponse[]
  totalCount: number
  skip: number
  take: number
}

export interface TransactionResponse {
  id: number
  sender: string
  recipient: string
  amount: number
  reason: string
  timestamp: string
}

export interface TransactionsResponse {
  transactions: TransactionResponse[]
  totalCount: number
  skip: number
  take: number
}

export interface TransferBeerRequest {
  senderId: string
  recipientId: string
  amount: number
}

export enum PollType {
  Number = 1,
  YesOrNo = 2,
  Color = 3,
  ThisOrThat = 4,
  HotTake = 5,
}

export enum PollStatus {
  Pending = 1,
  Approved = 2,
  Declined = 3,
}

export interface PollOptionResponse {
  id: number
  pollId: number
  answer: string
  emojiName: string
  emojiId?: number
}

export interface PollResponse {
  id: number
  accountId: string
  messageId: string
  question: string
  type: PollType
  status: PollStatus
  evaluatedOn?: string
  submittedOn: string
  options?: PollOptionResponse[]
  totalVotes: number
  optionVoteCounts?: Record<number, number>
}

export interface PollsResponse {
  polls: PollResponse[]
  totalCount: number
  skip: number
  take: number
}

export interface CreatePollRequest {
  accountId: string
  messageId: string
  question: string
  type: PollType
  options: PollOptionRequest[]
}

export interface PollOptionRequest {
  answer: string
  emojiName: string
  emojiId?: number
}

export interface SettingsResponse {
  pollSubmittedPenalty: number
  pollDeclinedPenalty: number
  maxPendingPolls: number
}

export interface CurrentLotteryResponse {
  id: number
  startDate: string
  endDate: string
  pool: number | null
  totalTickets: number
  totalParticipants: number
  odds: number
  winningTicket: number | null
}

export interface LotteryHistoryItem {
  id: number
  startDate: string
  endDate: string
  pool: number | null
  winningTicket: number | null
  totalTickets: number
  totalParticipants: number
}

export interface LotteryHistoryResponse {
  lotteries: LotteryHistoryItem[]
  totalCount: number
  skip: number
  take: number
}

export interface LotteryStatisticsPoint {
  date: string
  prizePool: number
  totalTickets: number
}

export interface LotteryStatisticsResponse {
  dataPoints: LotteryStatisticsPoint[]
}

export interface UserResponse {
  id: string
  username?: string
}

export interface UsersResponse {
  users: UserResponse[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export enum RenameStatus {
  Unknown = 0,
  Pending = 1,
  Active = 2,
  Expired = 3,
  BoughtOut = 4,
  Permanent = 5,
}

export interface RenameResponse {
  id: number
  oldName?: string
  newName: string
  affectedUserId: string
  requestedUserId: string
  days?: number
  cost: number
  notified: boolean
  status: RenameStatus
  startDate?: string
  expiration?: string
  timestamp: string
}

export interface CreateRenameRequest {
  newName: string
  affectedUserId: string
  requestedUserId: string
  days: number
  startDate?: string
  expiration?: string
  status?: RenameStatus
}

export interface CalculateRenameCostRequest {
  affectedUserId: string
  requestedUserId: string
  days: number
  newName: string
}

export interface RenameCostResponse {
  cost: number
}

export interface UpdateRenameStatusRequest {
  status: RenameStatus
}

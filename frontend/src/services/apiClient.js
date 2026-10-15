import axios from 'axios'
import { toast } from 'sonner'
import { API_BASE_URL } from '@/config/constants'
import { useAuthStore } from '@/store/auth-store'

/**
 * Hand-rolled API client for the YSSE Idea Box backend.
 *
 * - Reads the access token from the auth store and attaches it as a bearer token.
 * - Uses `withCredentials` so the HttpOnly guest-review cookie travels with requests.
 * - Unwraps the standard backend envelope:
 *     Success: { success:true, data, page?, pageSize?, total?, hasNextPage?, traceId }
 *     Error:   { success:false, error:{ message, code, details? }, traceId }
 * - Runs ONE 401 -> POST /auth/refresh -> retry cycle (queued/concurrent-safe),
 *   then logs the user out if refresh also fails.
 * - Surfaces errors centrally via sonner toasts; set request config
 *   `{ suppressToast: true }` to opt out.
 */

export class ApiError extends Error {
  constructor(message, { code, details, status, traceId } = {}) {
    super(message)
    this.name = 'ApiError'
    this.code = code
    this.details = details
    this.status = status
    this.traceId = traceId
  }

  getMessage() {
    return this.message
  }
}

const client = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
  timeout: 20000,
})

let isRefreshing = false
let waitQueue = []

function flushQueue(error, token = null) {
  waitQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error)
    else resolve(token)
  })
  waitQueue = []
}

const isEnvelope = (body) =>
  body && typeof body === 'object' && !Array.isArray(body) && 'success' in body

function buildApiError(body, status, traceId) {
  const err = body?.error || body || {}
  const message =
    err.message ||
    (typeof err === 'string' ? err : 'The server could not complete the request.')
  const details = Array.isArray(err.details) ? err.details : err.details
  if (Array.isArray(details) && details.length > 0 && typeof details[0] === 'string') {
    return new ApiError(`${message}: ${details.join('; ')}`, {
      code: err.code,
      details,
      status,
      traceId: body?.traceId || traceId,
    })
  }
  return new ApiError(message, {
    code: err.code,
    details,
    status,
    traceId: body?.traceId || traceId,
  })
}

/** Normalize any paged payload (envelope-style or PagedResult-style). */
function normalizePaged(payload) {
  if (!payload) return { items: [], page: 1, pageSize: 0, total: 0, hasNextPage: false }
  if (Array.isArray(payload)) {
    return { items: payload, page: 1, pageSize: payload.length, total: payload.length, hasNextPage: false }
  }
  if (payload.items !== undefined) {
    const items = Array.isArray(payload.items) ? payload.items : []
    return {
      items,
      page: payload.page ?? 1,
      pageSize: payload.pageSize ?? (Array.isArray(payload.items) ? payload.items.length : 0),
      total: payload.totalCount ?? payload.total ?? (Array.isArray(payload.items) ? payload.items.length : 0),
      hasNextPage: payload.hasNext ?? payload.hasNextPage ?? false,
      traceId: payload.traceId,
    }
  }
  return { items: [], page: 1, pageSize: 0, total: 0, hasNextPage: false }
}

function unwrapEnvelope(response) {
  const body = response?.data
  if (isEnvelope(body)) {
    if (body.success === false) throw buildApiError(body, response.status, body.traceId)
    return body.data === undefined ? null : body.data
  }
  return body
}

async function performRefresh() {
  const state = useAuthStore.getState()
  const refreshToken = state.refreshToken
  if (!refreshToken) throw new ApiError('Session expired. Please sign in again.', { status: 401 })
  // The response interceptor already unwraps the envelope for us.
  return client.post('/auth/refresh', { refreshToken })
}

function extractTokens(payload) {
  const tokens = payload && (payload.accessToken || payload.refreshToken) ? payload : payload?.data
  if (tokens && tokens.accessToken && tokens.refreshToken) {
    return { accessToken: tokens.accessToken, refreshToken: tokens.refreshToken, user: tokens.user }
  }
  return null
}

// Request interceptor: attach bearer token.
client.interceptors.request.use((config) => {
  const { accessToken } = useAuthStore.getState()
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

// Response interceptor: unwrap envelope, handle single 401->refresh->retry.
client.interceptors.response.use(
  (response) => unwrapEnvelope(response),
  async (error) => {
    const original = error?.config || {}

    if (error?.code === 'ERR_CANCELED') {
      return Promise.reject(error)
    }

    const status = error?.response?.status
    const body = error?.response?.data
    const traceId = error?.response?.headers?.['x-trace-id']

    // Network/timeout failures.
    if (!error?.response) {
      const message =
        error?.code === 'ECONNABORTED'
          ? 'The request timed out. Please try again.'
          : 'Cannot reach the server. Check your connection.'
      if (!original.suppressToast) toast.error(message)
      return Promise.reject(new ApiError(message, { status: 0 }))
    }

    // Build the ApiError from the backend envelope.
    let apiError
    try {
      apiError = isEnvelope(body) ? buildApiError(body, status, traceId) : new ApiError(
        body?.message || `Request failed with status ${status}`,
        { status, traceId },
      )
    } catch {
      apiError = new ApiError(`Request failed with status ${status}`, { status, traceId })
    }

    // Single 401 -> refresh -> retry flow (never for the refresh call itself).
    if (
      status === 401 &&
      !original._retry &&
      !String(original.url).includes('/auth/refresh') &&
      !String(original.url).includes('/auth/login')
    ) {
      const state = useAuthStore.getState()
      if (!state.refreshToken) {
        state.logout()
        if (!original.suppressToast) toast.error(apiError.message)
        return Promise.reject(apiError)
      }

      if (isRefreshing) {
        try {
          const token = await new Promise((resolve, reject) => {
            waitQueue.push({ resolve, reject })
          })
          if (token) {
            original._retry = true
            original.headers.Authorization = `Bearer ${token}`
            return client(original)
          }
          throw apiError
        } catch (queueError) {
          return Promise.reject(queueError)
        }
      }

      isRefreshing = true
      try {
        const payload = await performRefresh()
        const tokens = extractTokens(payload)
        if (!tokens) throw new ApiError('Unable to refresh session.', { status: 401 })
        useAuthStore.getState().setAuth(tokens.accessToken, tokens.refreshToken, tokens.user)
        flushQueue(null, tokens.accessToken)
        original._retry = true
        original.headers.Authorization = `Bearer ${tokens.accessToken}`
        return client(original)
      } catch (refreshError) {
        flushQueue(refreshError)
        useAuthStore.getState().logout()
        if (!original.suppressToast) toast.error('Your session has expired. Please sign in again.')
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    if (!original.suppressToast) {
      const skipCodes = new Set(['404', '403', '400'])
      // Not-found and validation failures are surfaced by the caller's UI, not here.
      if (!skipCodes.has(String(status))) {
        toast.error(apiError.message)
      }
    }

    return Promise.reject(apiError)
  },
)

export const api = {
  get: (url, config) => client.get(url, config),
  getPaged: async (url, config) => {
    const payload = await client.get(url, config)
    return normalizePaged(payload)
  },
  post: (url, data, config) => client.post(url, data, config),
  put: (url, data, config) => client.put(url, data, config),
  patch: (url, data, config) => client.patch(url, data, config),
  delete: (url, config) => client.delete(url, config),
  /**
   * Multipart POST for file uploads. `Content-Type: undefined` removes the default
   * JSON header so the browser sets `multipart/form-data` with a boundary.
   */
  upload: (url, data, config) =>
    client.post(url, data, { ...config, headers: { ...config?.headers, 'Content-Type': undefined } }),
}

export default api
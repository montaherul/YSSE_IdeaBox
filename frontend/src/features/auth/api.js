import api from '@/services/apiClient'

export const authApi = {
  register: (payload) => api.post('/auth/register', payload),
  login: (payload) => api.post('/auth/login', payload),
  refresh: (refreshToken) => api.post('/auth/refresh', { refreshToken }),
  logout: () => api.post('/auth/logout'),
  me: () => api.get('/auth/me'),
  requestVerify: (payload) => api.post('/auth/verify/request', payload),
  verify: (payload) => api.post('/auth/verify', payload),
  forgotPassword: (payload) => api.post('/auth/forgot-password', payload, { suppressToast: true }),
  resetPassword: (payload) => api.post('/auth/reset-password', payload, { suppressToast: true }),
}

export default authApi
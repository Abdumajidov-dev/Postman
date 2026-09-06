import { defineStore } from 'pinia'
import { api } from '../api/client'

export interface AuthUser {
  id: string
  email: string
  fullName: string
}

const TOKEN_KEY = 'pochtachi:token'
const USER_KEY = 'pochtachi:user'

function readStoredUser(): AuthUser | null {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? (JSON.parse(raw) as AuthUser) : null
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem(TOKEN_KEY) as string | null,
    user: readStoredUser(),
    error: null as string | null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
  actions: {
    async login(email: string, password: string) {
      this.error = null
      try {
        const { data } = await api.post<{ token: string; user: AuthUser }>('/api/auth/login', { email, password })
        this.setSession(data.token, data.user)
        return true
      } catch (e: any) {
        this.error = e?.response?.data?.message ?? 'Kirishda xatolik yuz berdi'
        return false
      }
    },

    async register(email: string, password: string, fullName: string) {
      this.error = null
      try {
        const { data } = await api.post<{ token: string; user: AuthUser }>('/api/auth/register', { email, password, fullName })
        this.setSession(data.token, data.user)
        return true
      } catch (e: any) {
        this.error = e?.response?.data?.message ?? "Ro'yxatdan o'tishda xatolik yuz berdi"
        return false
      }
    },

    setSession(token: string, user: AuthUser) {
      this.token = token
      this.user = user
      localStorage.setItem(TOKEN_KEY, token)
      localStorage.setItem(USER_KEY, JSON.stringify(user))
    },

    logout() {
      this.token = null
      this.user = null
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(USER_KEY)
    },
  },
})

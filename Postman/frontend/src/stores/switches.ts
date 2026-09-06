import { defineStore } from 'pinia'
import { api } from '../api/client'
import { useWorkspaceStore } from './workspace'

/**
 * "Switch Dimensions" — TZ 3.3.1. Backend'dan dimension/option/variable ro'yxati
 * olinadi; aktiv option o'zgartirilsa, server variable qiymatlarini shu bo'yicha
 * resolve qiladi va bu yerga qaytadi — hech qanday joyni qo'lda tuzatish shart emas.
 */
export interface SwitchOption {
  id: string
  name: string
  order: number
}

export interface SwitchDimension {
  id: string
  workspaceId: string
  name: string
  activeOptionId: string | null
  options: SwitchOption[]
}

export const useSwitchStore = defineStore('switches', {
  state: () => ({
    dimensions: [] as SwitchDimension[],
    resolved: {} as Record<string, string>,
  }),
  actions: {
    async fetchAll() {
      const workspace = useWorkspaceStore()
      await workspace.bootstrap()
      const { data } = await api.get<SwitchDimension[]>('/api/switch-dimensions', {
        params: { workspaceId: workspace.id },
      })
      this.dimensions = data
      await this.fetchResolved()
    },

    async fetchResolved() {
      const workspace = useWorkspaceStore()
      const { data } = await api.get<Record<string, string>>('/api/variables/resolved', {
        params: { workspaceId: workspace.id },
      })
      this.resolved = data
    },

    async setActiveOption(dimensionId: string, optionId: string) {
      const { data } = await api.put<SwitchDimension>(`/api/switch-dimensions/${dimensionId}/active-option`, {
        optionId,
      })
      const idx = this.dimensions.findIndex((d) => d.id === dimensionId)
      if (idx >= 0) this.dimensions[idx] = data
      await this.fetchResolved()
    },

    resolve(key: string): string | undefined {
      return this.resolved[key]
    },
  },
})

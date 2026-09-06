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

export interface Variable {
  id: string
  key: string
  isSecret: boolean
  /** optionId -> qiymat; "default" — hech qanday switch tanlanmaganda ishlatiladi */
  valuesByOptionId: Record<string, string>
}

export const useSwitchStore = defineStore('switches', {
  state: () => ({
    dimensions: [] as SwitchDimension[],
    variables: [] as Variable[],
    resolved: {} as Record<string, string>,
  }),
  actions: {
    async fetchAll() {
      const workspace = useWorkspaceStore()
      await workspace.bootstrap()
      await Promise.all([this.fetchDimensions(), this.fetchVariables()])
      await this.fetchResolved()
    },

    async fetchDimensions() {
      const workspace = useWorkspaceStore()
      const { data } = await api.get<SwitchDimension[]>('/api/switch-dimensions', {
        params: { workspaceId: workspace.id },
      })
      this.dimensions = data
    },

    async fetchVariables() {
      const workspace = useWorkspaceStore()
      const { data } = await api.get<Variable[]>('/api/variables', {
        params: { workspaceId: workspace.id },
      })
      this.variables = data
    },

    async fetchResolved() {
      const workspace = useWorkspaceStore()
      const { data } = await api.get<Record<string, string>>('/api/variables/resolved', {
        params: { workspaceId: workspace.id },
      })
      this.resolved = data
    },

    async createDimension(name: string, optionNames: string[]) {
      const workspace = useWorkspaceStore()
      const { data } = await api.post<SwitchDimension>('/api/switch-dimensions', {
        workspaceId: workspace.id,
        name,
        optionNames,
      })
      this.dimensions.push(data)
    },

    async addOption(dimensionId: string, name: string) {
      const { data } = await api.post<SwitchDimension>(`/api/switch-dimensions/${dimensionId}/options`, { name })
      const idx = this.dimensions.findIndex((d) => d.id === dimensionId)
      if (idx >= 0) this.dimensions[idx] = data
    },

    async setActiveOption(dimensionId: string, optionId: string) {
      const { data } = await api.put<SwitchDimension>(`/api/switch-dimensions/${dimensionId}/active-option`, {
        optionId,
      })
      const idx = this.dimensions.findIndex((d) => d.id === dimensionId)
      if (idx >= 0) this.dimensions[idx] = data
      await this.fetchResolved()
    },

    async createVariable(key: string, isSecret: boolean) {
      const workspace = useWorkspaceStore()
      const { data } = await api.post<Variable>('/api/variables', { key, isSecret }, {
        params: { workspaceId: workspace.id },
      })
      this.variables.push(data)
      await this.fetchResolved()
    },

    async setVariableValue(variableId: string, optionId: string | null, value: string) {
      const { data } = await api.put<Variable>(`/api/variables/${variableId}/value`, { optionId, value })
      const idx = this.variables.findIndex((v) => v.id === variableId)
      if (idx >= 0) this.variables[idx] = data
      await this.fetchResolved()
    },

    async deleteVariable(variableId: string) {
      await api.delete(`/api/variables/${variableId}`)
      this.variables = this.variables.filter((v) => v.id !== variableId)
      await this.fetchResolved()
    },

    resolve(key: string): string | undefined {
      return this.resolved[key]
    },
  },
})

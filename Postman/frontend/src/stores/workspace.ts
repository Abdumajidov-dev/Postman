import { defineStore } from 'pinia'
import { api } from '../api/client'

export const useWorkspaceStore = defineStore('workspace', {
  state: () => ({
    id: null as string | null,
    name: '',
    loaded: false,
  }),
  actions: {
    async bootstrap() {
      if (this.loaded) return
      const { data } = await api.get('/api/workspaces/default')
      this.id = data.id
      this.name = data.name
      this.loaded = true
    },
  },
})

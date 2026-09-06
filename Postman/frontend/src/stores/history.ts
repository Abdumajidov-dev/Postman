import { defineStore } from 'pinia'
import { api } from '../api/client'
import { useWorkspaceStore } from './workspace'

export interface HistoryEntry {
  id: string
  method: string
  url: string
  status: number | null
  durationMs: number
  executedAt: string
  requestSnapshotJson: string
  responseSnapshotJson: string
}

export const useHistoryStore = defineStore('history', {
  state: () => ({
    entries: [] as HistoryEntry[],
  }),
  actions: {
    async fetch() {
      const workspace = useWorkspaceStore()
      const { data } = await api.get<HistoryEntry[]>('/api/history', {
        params: { workspaceId: workspace.id, limit: 50 },
      })
      this.entries = data
    },

    async record(entry: { method: string; url: string; status: number | null; durationMs: number; request: unknown; response: unknown }) {
      const workspace = useWorkspaceStore()
      await api.post('/api/history', {
        workspaceId: workspace.id,
        method: entry.method,
        url: entry.url,
        status: entry.status,
        durationMs: entry.durationMs,
        requestSnapshotJson: JSON.stringify(entry.request),
        responseSnapshotJson: JSON.stringify(entry.response),
      })
      await this.fetch()
    },
  },
})

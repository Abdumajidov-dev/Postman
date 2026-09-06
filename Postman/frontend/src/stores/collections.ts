import { defineStore } from 'pinia'
import { api } from '../api/client'
import { useWorkspaceStore } from './workspace'

export interface CollectionItem {
  id: string
  workspaceId: string
  name: string
  description: string | null
}

export interface FolderItem {
  id: string
  collectionId: string
  parentFolderId: string | null
  name: string
  order: number
}

export interface KeyValue {
  key: string
  value: string
  enabled: boolean
}

export type BodyMode = 'none' | 'raw-json' | 'raw-text' | 'x-www-form-urlencoded' | 'form-data'

export interface AuthConfig {
  type: 'NoAuth' | 'Basic' | 'Bearer' | 'ApiKey' | 'OAuth2' | 'Digest'
  values: Record<string, string>
}

export interface RequestItem {
  id: string
  collectionId: string
  folderId: string | null
  name: string
  method: string
  url: string
  headers: KeyValue[]
  queryParams: KeyValue[]
  bodyMode: BodyMode
  body: string | null
  auth: AuthConfig | null
  preRequestScript: string | null
  testScript: string | null
  order: number
}

export const useCollectionStore = defineStore('collections', {
  state: () => ({
    items: [] as CollectionItem[],
    foldersByCollection: {} as Record<string, FolderItem[]>,
    requestsByCollection: {} as Record<string, RequestItem[]>,
    expanded: {} as Record<string, boolean>,
    expandedFolders: {} as Record<string, boolean>,
    selectedRequestId: null as string | null,
  }),
  getters: {
    selectedRequest(state): RequestItem | undefined {
      for (const list of Object.values(state.requestsByCollection)) {
        const found = list.find((r) => r.id === state.selectedRequestId)
        if (found) return found
      }
      return undefined
    },
  },
  actions: {
    async fetchAll() {
      const workspace = useWorkspaceStore()
      await workspace.bootstrap()
      const { data } = await api.get<CollectionItem[]>('/api/collections', {
        params: { workspaceId: workspace.id },
      })
      this.items = data
    },

    async create(name: string) {
      const trimmed = name.trim()
      if (!trimmed) return
      const workspace = useWorkspaceStore()
      const { data } = await api.post<CollectionItem>('/api/collections', {
        workspaceId: workspace.id,
        name: trimmed,
        description: null,
      })
      this.items.push(data)
    },

    async remove(id: string) {
      await api.delete(`/api/collections/${id}`)
      this.items = this.items.filter((c) => c.id !== id)
      delete this.requestsByCollection[id]
      delete this.foldersByCollection[id]
    },

    async toggleExpand(collectionId: string) {
      this.expanded[collectionId] = !this.expanded[collectionId]
      if (this.expanded[collectionId] && !this.requestsByCollection[collectionId]) {
        await Promise.all([this.fetchRequests(collectionId), this.fetchFolders(collectionId)])
      }
    },

    toggleExpandFolder(folderId: string) {
      this.expandedFolders[folderId] = !this.expandedFolders[folderId]
    },

    async fetchFolders(collectionId: string) {
      const { data } = await api.get<FolderItem[]>('/api/folders', { params: { collectionId } })
      this.foldersByCollection[collectionId] = data
    },

    async createFolder(collectionId: string, name: string, parentFolderId: string | null) {
      const trimmed = name.trim()
      if (!trimmed) return
      const { data } = await api.post<FolderItem>('/api/folders', {
        collectionId,
        parentFolderId,
        name: trimmed,
      })
      if (!this.foldersByCollection[collectionId]) this.foldersByCollection[collectionId] = []
      this.foldersByCollection[collectionId].push(data)
    },

    async fetchRequests(collectionId: string) {
      const { data } = await api.get<RequestItem[]>('/api/requests', {
        params: { collectionId },
      })
      this.requestsByCollection[collectionId] = data
    },

    async createRequest(collectionId: string, name: string, folderId: string | null = null) {
      const trimmed = name.trim()
      if (!trimmed) return
      const { data } = await api.post<RequestItem>('/api/requests', {
        collectionId,
        folderId,
        name: trimmed,
        method: 'GET',
        url: '',
      })
      if (!this.requestsByCollection[collectionId]) this.requestsByCollection[collectionId] = []
      this.requestsByCollection[collectionId].push(data)
      this.selectedRequestId = data.id
    },

    async saveRequest(request: RequestItem) {
      await api.put(`/api/requests/${request.id}`, {
        name: request.name,
        method: request.method,
        url: request.url,
        headers: request.headers,
        queryParams: request.queryParams,
        bodyMode: request.bodyMode,
        body: request.body,
        auth: request.auth,
        preRequestScript: request.preRequestScript,
        testScript: request.testScript,
        order: request.order,
      })
      const list = this.requestsByCollection[request.collectionId]
      const idx = list?.findIndex((r) => r.id === request.id)
      if (list && idx !== undefined && idx >= 0) list[idx] = { ...request }
    },

    selectRequest(id: string) {
      this.selectedRequestId = id
    },

    async importCollection(json: string) {
      const workspace = useWorkspaceStore()
      const { data } = await api.post<CollectionItem>('/api/collections/import', json, {
        params: { workspaceId: workspace.id },
        headers: { 'Content-Type': 'application/json' },
      })
      this.items.push(data)
      return data
    },

    async exportCollection(collectionId: string): Promise<string> {
      const { data } = await api.get<string>(`/api/collections/${collectionId}/export`, {
        transformResponse: (res) => res,
      })
      return data
    },
  },
})

import { contextBridge, ipcRenderer } from 'electron'

export interface HttpRequestConfig {
  method: string
  url: string
  headers?: Record<string, string>
  body?: string
}

export interface HttpResponseResult {
  ok: boolean
  status?: number
  statusText?: string
  headers?: Record<string, string>
  body?: string
  error?: string
  durationMs: number
}

contextBridge.exposeInMainWorld('pochtachi', {
  sendRequest: (config: HttpRequestConfig): Promise<HttpResponseResult> =>
    ipcRenderer.invoke('pochtachi:send-request', config),
})

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

export interface SaveFileResult {
  ok: boolean
  canceled?: boolean
  filePath?: string
}

export interface OpenFileResult {
  ok: boolean
  canceled?: boolean
  filePath?: string
  content?: string
}

contextBridge.exposeInMainWorld('pochtachi', {
  sendRequest: (config: HttpRequestConfig): Promise<HttpResponseResult> =>
    ipcRenderer.invoke('pochtachi:send-request', config),

  saveFile: (options: { defaultName: string; content: string }): Promise<SaveFileResult> =>
    ipcRenderer.invoke('pochtachi:save-file', options),

  openFile: (): Promise<OpenFileResult> => ipcRenderer.invoke('pochtachi:open-file'),
})

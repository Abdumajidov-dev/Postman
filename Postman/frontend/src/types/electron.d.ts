import type { HttpRequestConfig, HttpResponseResult, SaveFileResult, OpenFileResult } from '../../electron/preload'

declare global {
  interface Window {
    pochtachi: {
      sendRequest: (config: HttpRequestConfig) => Promise<HttpResponseResult>
      saveFile: (options: { defaultName: string; content: string }) => Promise<SaveFileResult>
      openFile: () => Promise<OpenFileResult>
    }
  }
}

export {}

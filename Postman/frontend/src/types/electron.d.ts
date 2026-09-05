import type { HttpRequestConfig, HttpResponseResult } from '../../electron/preload'

declare global {
  interface Window {
    pochtachi: {
      sendRequest: (config: HttpRequestConfig) => Promise<HttpResponseResult>
    }
  }
}

export {}

import type { HttpResponseResult } from '../../electron/preload'
import type { AuthConfig, KeyValue, RequestItem } from '../stores/collections'
import { resolveTemplate } from './template'
import { createPreRequestPm, createTestPm, runScript, type TestResult } from './scripting'

export interface ExecutionResult {
  fullUrl: string
  requestHeaders: Record<string, string>
  requestBody?: string
  response: HttpResponseResult
  testResults: TestResult[]
  scriptError: string | null
}

/** OAuth2 client_credentials orqali token oladi (Electron main process orqali, CORS'siz). */
async function fetchOAuth2Token(values: Record<string, string>, resolve: (k: string) => string | undefined) {
  const url = resolveTemplate(values.accessTokenUrl ?? '', resolve)
  const clientId = resolveTemplate(values.clientId ?? '', resolve)
  const clientSecret = resolveTemplate(values.clientSecret ?? '', resolve)
  if (!url) return undefined

  const body = new URLSearchParams({
    grant_type: values.grantType || 'client_credentials',
    client_id: clientId,
    client_secret: clientSecret,
  }).toString()

  const res = await window.pochtachi.sendRequest({
    method: 'POST',
    url,
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body,
  })
  if (!res.ok || !res.body) return undefined
  try {
    return (JSON.parse(res.body) as { access_token?: string }).access_token
  } catch {
    return undefined
  }
}

async function resolveAuthHeaders(auth: AuthConfig | null, query: URLSearchParams, resolve: (k: string) => string | undefined) {
  const headers: Record<string, string> = {}
  if (!auth) return headers

  switch (auth.type) {
    case 'Bearer':
      headers.Authorization = `Bearer ${resolveTemplate(auth.values.token ?? '', resolve)}`
      break
    case 'Basic': {
      const user = resolveTemplate(auth.values.username ?? '', resolve)
      const pass = resolveTemplate(auth.values.password ?? '', resolve)
      headers.Authorization = `Basic ${btoa(`${user}:${pass}`)}`
      break
    }
    case 'ApiKey': {
      const key = auth.values.key ?? ''
      const value = resolveTemplate(auth.values.value ?? '', resolve)
      if (auth.values.in === 'query') query.set(key, value)
      else headers[key] = value
      break
    }
    case 'OAuth2': {
      const token = auth.values.accessToken?.trim()
        ? resolveTemplate(auth.values.accessToken, resolve)
        : await fetchOAuth2Token(auth.values, resolve)
      if (token) headers.Authorization = `Bearer ${token}`
      break
    }
  }
  return headers
}

/**
 * Bitta so'rovni to'liq bajaradi: pre-request script -> URL/header/query/auth/body
 * qurish (switch'lar orqali resolve qilib) -> yuborish -> test script.
 * RequestBar (bitta so'rov) va CollectionRunner (butun collection) ikkalasi shu yerdan foydalanadi.
 */
export async function executeRequest(
  request: RequestItem,
  ownerAuth: AuthConfig | null,
  baseResolve: (key: string) => string | undefined,
): Promise<ExecutionResult> {
  const runtimeVars = new Map<string, string>()
  const resolve = (key: string) => runtimeVars.get(key) ?? baseResolve(key)
  let scriptError: string | null = null

  if (request.preRequestScript) {
    const pm = createPreRequestPm({
      get: (key) => resolve(key),
      set: (key, value) => runtimeVars.set(key, value),
    })
    const { error } = runScript(request.preRequestScript, pm)
    if (error) scriptError = `Pre-request script xatosi: ${error}`
  }

  const url = resolveTemplate(request.url, resolve)
  const headers: Record<string, string> = {}
  for (const h of request.headers) {
    if (h.enabled && h.key) headers[h.key] = resolveTemplate(h.value, resolve)
  }

  const query = new URLSearchParams()
  for (const p of request.queryParams) {
    if (p.enabled && p.key) query.set(p.key, resolveTemplate(p.value, resolve))
  }

  const effectiveAuth = request.auth ?? ownerAuth
  Object.assign(headers, await resolveAuthHeaders(effectiveAuth, query, resolve))

  const queryStr = query.toString()
  const fullUrl = queryStr ? `${url}${url.includes('?') ? '&' : '?'}${queryStr}` : url

  let body: string | undefined
  if (request.bodyMode === 'raw-json' || request.bodyMode === 'raw-text') {
    body = request.body ? resolveTemplate(request.body, resolve) : undefined
    if (request.bodyMode === 'raw-json') headers['Content-Type'] ??= 'application/json'
  } else if (request.bodyMode === 'x-www-form-urlencoded') {
    const list: KeyValue[] = request.body ? JSON.parse(request.body) : []
    const params = new URLSearchParams()
    for (const kv of list) if (kv.enabled && kv.key) params.set(kv.key, resolveTemplate(kv.value, resolve))
    body = params.toString()
    headers['Content-Type'] ??= 'application/x-www-form-urlencoded'
  }

  const response = await window.pochtachi.sendRequest({ method: request.method, url: fullUrl, headers, body })

  const testResults: TestResult[] = []
  if (request.testScript) {
    const pm = createTestPm(
      {
        code: response.status ?? 0,
        status: response.statusText ?? '',
        responseTime: response.durationMs,
        headers: response.headers ?? {},
        text: () => response.body ?? '',
        json: () => JSON.parse(response.body ?? '{}'),
      },
      testResults,
    )
    const { error } = runScript(request.testScript, pm)
    if (error) scriptError = (scriptError ? `${scriptError}; ` : '') + `Test script xatosi: ${error}`
  }

  return { fullUrl, requestHeaders: headers, requestBody: body, response, testResults, scriptError }
}

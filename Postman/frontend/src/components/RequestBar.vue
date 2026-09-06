<script setup lang="ts">
import { computed, ref } from 'vue'
import { useSwitchStore } from '../stores/switches'
import { useCollectionStore, type KeyValue, type BodyMode, type AuthConfig } from '../stores/collections'
import { useHistoryStore } from '../stores/history'
import { resolveTemplate } from '../utils/template'

const methods = ['GET', 'POST', 'PUT', 'PATCH', 'DELETE'] as const
const methodColor: Record<string, string> = {
  GET: 'text-method-get',
  POST: 'text-method-post',
  PUT: 'text-method-put',
  PATCH: 'text-method-patch',
  DELETE: 'text-method-delete',
}

const bodyModes: { value: BodyMode; label: string }[] = [
  { value: 'none', label: 'None' },
  { value: 'raw-json', label: 'Raw (JSON)' },
  { value: 'raw-text', label: 'Raw (Text)' },
  { value: 'x-www-form-urlencoded', label: 'x-www-form-urlencoded' },
  { value: 'form-data', label: 'Form Data' },
]

const authTypes: { value: AuthConfig['type']; label: string }[] = [
  { value: 'NoAuth', label: 'No Auth' },
  { value: 'Bearer', label: 'Bearer Token' },
  { value: 'Basic', label: 'Basic Auth' },
  { value: 'ApiKey', label: 'API Key' },
  { value: 'OAuth2', label: 'OAuth 2.0' },
  { value: 'Digest', label: 'Digest Auth' },
]

const switchStore = useSwitchStore()
const collectionStore = useCollectionStore()
const historyStore = useHistoryStore()

const current = computed(() => collectionStore.selectedRequest)
const activeTab = ref<'params' | 'headers' | 'body' | 'auth'>('params')
const sending = ref(false)
const saving = ref(false)
const result = ref<{ status?: number; durationMs?: number; body?: string; error?: string } | null>(null)

function resolve(key: string) {
  return switchStore.resolve(key)
}

function resolvedUrl(url: string) {
  return resolveTemplate(url, resolve)
}

function addRow(list: KeyValue[]) {
  list.push({ key: '', value: '', enabled: true })
}

function removeRow(list: KeyValue[], index: number) {
  list.splice(index, 1)
}

const bodyKeyValues = computed<KeyValue[]>({
  get() {
    if (!current.value?.body) return []
    try {
      return JSON.parse(current.value.body)
    } catch {
      return []
    }
  },
  set(list) {
    if (current.value) current.value.body = JSON.stringify(list)
  },
})

function updateBodyRow(index: number, field: keyof KeyValue, value: string | boolean) {
  const list = bodyKeyValues.value
  // @ts-expect-error - field/value tandem is always a valid KeyValue key/value pair
  list[index][field] = value
  bodyKeyValues.value = list
}

function removeBodyRow(index: number) {
  bodyKeyValues.value = bodyKeyValues.value.filter((_, i) => i !== index)
}

function addBodyRow() {
  bodyKeyValues.value = [...bodyKeyValues.value, { key: '', value: '', enabled: true }]
}

function setBodyMode(mode: BodyMode) {
  if (!current.value) return
  current.value.bodyMode = mode
  if (mode === 'x-www-form-urlencoded' || mode === 'form-data') {
    if (!current.value.body) current.value.body = '[]'
  } else if (mode === 'none') {
    current.value.body = null
  }
}

function setAuthType(type: AuthConfig['type']) {
  if (!current.value) return
  if (type === 'NoAuth') {
    current.value.auth = null
    return
  }
  const defaults: Record<string, Record<string, string>> = {
    Bearer: { token: '' },
    Basic: { username: '', password: '' },
    ApiKey: { key: '', value: '', in: 'header' },
    OAuth2: { accessToken: '' },
    Digest: { username: '', password: '' },
  }
  current.value.auth = { type, values: defaults[type] ?? {} }
}

async function save() {
  if (!current.value) return
  saving.value = true
  try {
    await collectionStore.saveRequest(current.value)
  } finally {
    saving.value = false
  }
}

function buildAuthHeaders(auth: AuthConfig | null, query: URLSearchParams) {
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
  }
  return headers
}

async function send() {
  if (!current.value) return
  sending.value = true
  result.value = null

  const url = resolvedUrl(current.value.url)
  const headers: Record<string, string> = {}
  for (const h of current.value.headers) {
    if (h.enabled && h.key) headers[h.key] = resolveTemplate(h.value, resolve)
  }

  const query = new URLSearchParams()
  for (const p of current.value.queryParams) {
    if (p.enabled && p.key) query.set(p.key, resolveTemplate(p.value, resolve))
  }

  Object.assign(headers, buildAuthHeaders(current.value.auth, query))

  const queryStr = query.toString()
  const fullUrl = queryStr ? `${url}${url.includes('?') ? '&' : '?'}${queryStr}` : url

  let body: string | undefined
  if (current.value.bodyMode === 'raw-json' || current.value.bodyMode === 'raw-text') {
    body = current.value.body ? resolveTemplate(current.value.body, resolve) : undefined
    if (current.value.bodyMode === 'raw-json') headers['Content-Type'] ??= 'application/json'
  } else if (current.value.bodyMode === 'x-www-form-urlencoded') {
    const params = new URLSearchParams()
    for (const kv of bodyKeyValues.value) if (kv.enabled && kv.key) params.set(kv.key, resolveTemplate(kv.value, resolve))
    body = params.toString()
    headers['Content-Type'] ??= 'application/x-www-form-urlencoded'
  }

  try {
    const res = await window.pochtachi.sendRequest({ method: current.value.method, url: fullUrl, headers, body })
    result.value = res
    await historyStore.record({
      method: current.value.method,
      url: fullUrl,
      status: res.status ?? null,
      durationMs: res.durationMs,
      request: { method: current.value.method, url: fullUrl, headers, body },
      response: res,
    })
  } finally {
    sending.value = false
  }
}
</script>

<template>
  <div v-if="!current" class="flex flex-1 items-center justify-center text-sm text-gray-500">
    So'rov tanlang yoki chapdan yangi so'rov yarating
  </div>

  <template v-else>
    <div class="flex items-center gap-2 border-b border-border-subtle bg-surface-1 px-4 py-3">
      <select
        v-model="current.method"
        class="rounded-md border border-border-subtle bg-surface-2 px-2 py-2 text-sm font-semibold outline-none"
        :class="methodColor[current.method]"
      >
        <option v-for="m in methods" :key="m">{{ m }}</option>
      </select>
      <input
        v-model="current.url"
        class="flex-1 rounded-md border border-border-subtle bg-surface-2 px-3 py-2 font-mono text-sm text-gray-200 outline-none focus:border-brand-500"
        placeholder="{{baseUrl}}/path"
      />
      <button
        class="rounded-md border border-border-subtle px-3 py-2 text-sm text-gray-300 transition hover:bg-surface-2 disabled:opacity-50"
        :disabled="saving"
        @click="save"
      >
        {{ saving ? 'Saqlanmoqda…' : 'Save' }}
      </button>
      <button
        class="rounded-md bg-brand-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-brand-600 disabled:opacity-50"
        :disabled="sending"
        @click="send"
      >
        {{ sending ? 'Yuborilmoqda…' : 'Send' }}
      </button>
    </div>

    <div class="flex gap-4 border-b border-border-subtle bg-surface-1 px-4 text-xs">
      <button
        v-for="tab in (['params', 'headers', 'auth', 'body'] as const)"
        :key="tab"
        class="border-b-2 py-2 capitalize transition"
        :class="activeTab === tab ? 'border-brand-500 text-gray-100' : 'border-transparent text-gray-500 hover:text-gray-300'"
        @click="activeTab = tab"
      >
        {{ tab }}
      </button>
    </div>

    <div class="flex-1 overflow-auto p-4">
      <table v-if="activeTab === 'params' || activeTab === 'headers'" class="w-full text-xs">
        <thead>
          <tr class="text-left text-gray-500">
            <th class="w-6"></th>
            <th class="pb-2 font-medium">Key</th>
            <th class="pb-2 font-medium">Value</th>
            <th class="w-6"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(row, i) in (activeTab === 'params' ? current.queryParams : current.headers)" :key="i">
            <td><input type="checkbox" v-model="row.enabled" /></td>
            <td class="pr-2">
              <input v-model="row.key" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1 font-mono text-gray-200 outline-none" placeholder="key" />
            </td>
            <td class="pr-2">
              <input v-model="row.value" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1 font-mono text-gray-200 outline-none" placeholder="value" />
            </td>
            <td>
              <button class="text-gray-500 hover:text-method-delete" @click="removeRow(activeTab === 'params' ? current.queryParams : current.headers, i)">✕</button>
            </td>
          </tr>
        </tbody>
      </table>
      <button
        v-if="activeTab === 'params' || activeTab === 'headers'"
        class="mt-2 text-xs text-brand-400 hover:underline"
        @click="addRow(activeTab === 'params' ? current.queryParams : current.headers)"
      >
        + qator qo'shish
      </button>

      <div v-if="activeTab === 'auth'" class="max-w-md space-y-3">
        <select
          :value="current.auth?.type ?? 'NoAuth'"
          class="w-full rounded-md border border-border-subtle bg-surface-2 px-2 py-2 text-xs text-gray-200 outline-none"
          @change="setAuthType(($event.target as HTMLSelectElement).value as AuthConfig['type'])"
        >
          <option v-for="t in authTypes" :key="t.value" :value="t.value">{{ t.label }}</option>
        </select>

        <template v-if="current.auth?.type === 'Bearer'">
          <label class="block text-xs text-gray-500">Token</label>
          <input v-model="current.auth.values.token" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" placeholder="{{authToken}}" />
        </template>

        <template v-else-if="current.auth?.type === 'Basic'">
          <label class="block text-xs text-gray-500">Username</label>
          <input v-model="current.auth.values.username" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 text-xs text-gray-200 outline-none" />
          <label class="block text-xs text-gray-500">Password</label>
          <input v-model="current.auth.values.password" type="password" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 text-xs text-gray-200 outline-none" />
        </template>

        <template v-else-if="current.auth?.type === 'ApiKey'">
          <label class="block text-xs text-gray-500">Key</label>
          <input v-model="current.auth.values.key" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" />
          <label class="block text-xs text-gray-500">Value</label>
          <input v-model="current.auth.values.value" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" />
          <label class="block text-xs text-gray-500">Qo'shish joyi</label>
          <select v-model="current.auth.values.in" class="w-full rounded-md border border-border-subtle bg-surface-2 px-2 py-1.5 text-xs text-gray-200 outline-none">
            <option value="header">Header</option>
            <option value="query">Query Param</option>
          </select>
        </template>

        <p v-else-if="current.auth?.type === 'OAuth2' || current.auth?.type === 'Digest'" class="text-xs text-gray-500">
          {{ current.auth.type }} sozlamalari saqlanadi, lekin so'rov yuborishda avtomatik hisoblash keyingi versiyada qo'shiladi.
        </p>
      </div>

      <div v-if="activeTab === 'body'">
        <select
          :value="current.bodyMode"
          class="mb-3 rounded-md border border-border-subtle bg-surface-2 px-2 py-1.5 text-xs text-gray-200 outline-none"
          @change="setBodyMode(($event.target as HTMLSelectElement).value as BodyMode)"
        >
          <option v-for="m in bodyModes" :key="m.value" :value="m.value">{{ m.label }}</option>
        </select>

        <textarea
          v-if="current.bodyMode === 'raw-json' || current.bodyMode === 'raw-text'"
          v-model="current.body"
          class="h-48 w-full rounded-md border border-border-subtle bg-surface-2 p-3 font-mono text-xs text-gray-200 outline-none focus:border-brand-500"
          placeholder='{"key": "value"}'
        />

        <template v-else-if="current.bodyMode === 'x-www-form-urlencoded' || current.bodyMode === 'form-data'">
          <table class="w-full text-xs">
            <thead>
              <tr class="text-left text-gray-500">
                <th class="w-6"></th>
                <th class="pb-2 font-medium">Key</th>
                <th class="pb-2 font-medium">Value</th>
                <th class="w-6"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, i) in bodyKeyValues" :key="i">
                <td><input type="checkbox" :checked="row.enabled" @change="updateBodyRow(i, 'enabled', ($event.target as HTMLInputElement).checked)" /></td>
                <td class="pr-2"><input :value="row.key" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1 font-mono text-gray-200 outline-none" @input="updateBodyRow(i, 'key', ($event.target as HTMLInputElement).value)" /></td>
                <td class="pr-2"><input :value="row.value" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1 font-mono text-gray-200 outline-none" @input="updateBodyRow(i, 'value', ($event.target as HTMLInputElement).value)" /></td>
                <td><button class="text-gray-500 hover:text-method-delete" @click="removeBodyRow(i)">✕</button></td>
              </tr>
            </tbody>
          </table>
          <button class="mt-2 text-xs text-brand-400 hover:underline" @click="addBodyRow">
            + qator qo'shish
          </button>
        </template>
      </div>
    </div>

    <div v-if="result" class="border-t border-border-subtle bg-surface-1 px-4 py-3 font-mono text-xs">
      <template v-if="result.error">
        <span class="text-method-delete">Xato: {{ result.error }}</span>
      </template>
      <template v-else>
        <span class="text-method-get">{{ result.status }}</span>
        <span class="ml-3 text-gray-500">{{ result.durationMs }}ms</span>
        <pre class="mt-2 max-h-64 overflow-auto whitespace-pre-wrap text-gray-300">{{ result.body }}</pre>
      </template>
    </div>
  </template>
</template>

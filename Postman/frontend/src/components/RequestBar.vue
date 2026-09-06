<script setup lang="ts">
import { computed, ref } from 'vue'
import { useSwitchStore } from '../stores/switches'
import { useCollectionStore, type KeyValue } from '../stores/collections'
import { resolveTemplate } from '../utils/template'

const methods = ['GET', 'POST', 'PUT', 'PATCH', 'DELETE'] as const
const methodColor: Record<string, string> = {
  GET: 'text-method-get',
  POST: 'text-method-post',
  PUT: 'text-method-put',
  PATCH: 'text-method-patch',
  DELETE: 'text-method-delete',
}

const switchStore = useSwitchStore()
const collectionStore = useCollectionStore()

const current = computed(() => collectionStore.selectedRequest)
const activeTab = ref<'params' | 'headers' | 'body'>('params')
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

async function save() {
  if (!current.value) return
  saving.value = true
  try {
    await collectionStore.saveRequest(current.value)
  } finally {
    saving.value = false
  }
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

  const query = current.value.queryParams
    .filter((p) => p.enabled && p.key)
    .map((p) => `${encodeURIComponent(p.key)}=${encodeURIComponent(resolveTemplate(p.value, resolve))}`)
    .join('&')
  const fullUrl = query ? `${url}${url.includes('?') ? '&' : '?'}${query}` : url

  try {
    const res = await window.pochtachi.sendRequest({
      method: current.value.method,
      url: fullUrl,
      headers,
      body: current.value.body ? resolveTemplate(current.value.body, resolve) : undefined,
    })
    result.value = res
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
        v-for="tab in (['params', 'headers', 'body'] as const)"
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
          <tr
            v-for="(row, i) in (activeTab === 'params' ? current.queryParams : current.headers)"
            :key="i"
          >
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

      <textarea
        v-if="activeTab === 'body'"
        v-model="current.body"
        class="h-48 w-full rounded-md border border-border-subtle bg-surface-2 p-3 font-mono text-xs text-gray-200 outline-none focus:border-brand-500"
        placeholder='{"key": "value"}'
      />
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

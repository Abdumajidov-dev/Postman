<script setup lang="ts">
import { ref } from 'vue'
import { useSwitchStore } from '../stores/switches'

const methods = ['GET', 'POST', 'PUT', 'PATCH', 'DELETE'] as const
const methodColor: Record<(typeof methods)[number], string> = {
  GET: 'text-method-get',
  POST: 'text-method-post',
  PUT: 'text-method-put',
  PATCH: 'text-method-patch',
  DELETE: 'text-method-delete',
}

const store = useSwitchStore()
const method = ref<(typeof methods)[number]>('GET')
const url = ref('{{baseUrl}}/api/health')
const sending = ref(false)
const result = ref<{ status?: number; durationMs?: number; body?: string; error?: string } | null>(null)

function resolveUrl(raw: string) {
  return raw.replace(/{{\s*(\w+)\s*}}/g, (_match, key: string) => store.resolve(key) ?? `{{${key}}}`)
}

async function send() {
  sending.value = true
  result.value = null
  const resolved = resolveUrl(url.value)
  const authToken = store.resolve('authToken')
  try {
    const res = await window.pochtachi.sendRequest({
      method: method.value,
      url: resolved,
      headers: authToken ? { Authorization: `Bearer ${authToken}` } : undefined,
    })
    result.value = res
  } finally {
    sending.value = false
  }
}
</script>

<template>
  <div class="flex items-center gap-2 border-b border-border-subtle bg-surface-1 px-4 py-3">
    <select
      v-model="method"
      class="rounded-md border border-border-subtle bg-surface-2 px-2 py-2 text-sm font-semibold outline-none"
      :class="methodColor[method]"
    >
      <option v-for="m in methods" :key="m">{{ m }}</option>
    </select>
    <input
      v-model="url"
      class="flex-1 rounded-md border border-border-subtle bg-surface-2 px-3 py-2 font-mono text-sm text-gray-200 outline-none focus:border-brand-500"
      placeholder="{{baseUrl}}/path"
    />
    <span class="font-mono text-xs text-gray-500">{{ resolveUrl(url) }}</span>
    <button
      class="rounded-md bg-brand-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-brand-600 disabled:opacity-50"
      :disabled="sending"
      @click="send"
    >
      {{ sending ? 'Yuborilmoqda…' : 'Send' }}
    </button>
  </div>

  <div v-if="result" class="border-b border-border-subtle bg-surface-1 px-4 py-3 font-mono text-xs">
    <template v-if="result.error">
      <span class="text-method-delete">Xato: {{ result.error }}</span>
    </template>
    <template v-else>
      <span class="text-method-get">{{ result.status }}</span>
      <span class="ml-3 text-gray-500">{{ result.durationMs }}ms</span>
      <pre class="mt-2 whitespace-pre-wrap text-gray-300">{{ result.body }}</pre>
    </template>
  </div>
</template>

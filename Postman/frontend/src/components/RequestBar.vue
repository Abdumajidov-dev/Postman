<script setup lang="ts">
import { computed, ref } from 'vue'
import { useSwitchStore } from '../stores/switches'
import { useCollectionStore, type KeyValue, type BodyMode } from '../stores/collections'
import { useHistoryStore } from '../stores/history'
import { executeRequest } from '../utils/requestExecutor'
import type { TestResult } from '../utils/scripting'
import AuthEditor from './AuthEditor.vue'

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

const switchStore = useSwitchStore()
const collectionStore = useCollectionStore()
const historyStore = useHistoryStore()

const current = computed(() => collectionStore.selectedRequest)
const ownerCollection = computed(() => collectionStore.items.find((c) => c.id === current.value?.collectionId))

const activeTab = ref<'params' | 'headers' | 'auth' | 'body' | 'scripts'>('params')
const sending = ref(false)
const saving = ref(false)
const result = ref<{ status?: number; durationMs?: number; body?: string; error?: string } | null>(null)
const testResults = ref<TestResult[]>([])
const scriptError = ref<string | null>(null)

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
  testResults.value = []
  scriptError.value = null

  try {
    const { fullUrl, requestHeaders, requestBody, response, testResults: results, scriptError: err } = await executeRequest(
      current.value,
      ownerCollection.value?.auth ?? null,
      (key) => switchStore.resolve(key),
    )
    result.value = response
    testResults.value = results
    scriptError.value = err

    await historyStore.record({
      method: current.value.method,
      url: fullUrl,
      status: response.status ?? null,
      durationMs: response.durationMs,
      request: { method: current.value.method, url: fullUrl, headers: requestHeaders, body: requestBody },
      response,
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
        v-for="tab in (['params', 'headers', 'auth', 'body', 'scripts'] as const)"
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

      <div v-if="activeTab === 'auth'">
        <p v-if="!current.auth && ownerCollection?.auth" class="mb-2 text-xs text-gray-500">
          Collection'dan meros: <span class="text-gray-300">{{ ownerCollection.auth.type }}</span>. O'zgartirish uchun bu yerda tanlang.
        </p>
        <AuthEditor v-model="current.auth" />
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

      <div v-if="activeTab === 'scripts'" class="space-y-4">
        <div>
          <label class="mb-1 block text-xs font-semibold text-gray-400">Pre-request Script</label>
          <textarea
            v-model="current.preRequestScript"
            class="h-32 w-full rounded-md border border-border-subtle bg-surface-2 p-3 font-mono text-xs text-gray-200 outline-none focus:border-brand-500"
            placeholder="pm.variables.set('token', '123')"
          />
        </div>
        <div>
          <label class="mb-1 block text-xs font-semibold text-gray-400">Test Script</label>
          <textarea
            v-model="current.testScript"
            class="h-32 w-full rounded-md border border-border-subtle bg-surface-2 p-3 font-mono text-xs text-gray-200 outline-none focus:border-brand-500"
            placeholder="pm.test('status 200', () => { pm.expect(pm.response.code).to.equal(200) })"
          />
        </div>
        <p class="text-xs text-gray-600">
          `pm.environment`/`pm.variables` — get/set (shu yuborishga xos, saqlanmaydi). `pm.test(nomi, fn)`,
          `pm.expect(qiymat).to.equal/eql/a/above/below/include/true/false`. To'liq Postman API emas — asosiy kichik qism.
        </p>
      </div>
    </div>

    <div v-if="result || scriptError" class="border-t border-border-subtle bg-surface-1 px-4 py-3 font-mono text-xs">
      <p v-if="scriptError" class="mb-2 text-method-delete">{{ scriptError }}</p>
      <template v-if="result?.error">
        <span class="text-method-delete">Xato: {{ result.error }}</span>
      </template>
      <template v-else-if="result">
        <span class="text-method-get">{{ result.status }}</span>
        <span class="ml-3 text-gray-500">{{ result.durationMs }}ms</span>

        <div v-if="testResults.length > 0" class="mt-2 space-y-1">
          <div v-for="t in testResults" :key="t.name" class="flex items-center gap-2">
            <span :class="t.passed ? 'text-method-get' : 'text-method-delete'">{{ t.passed ? '✓' : '✗' }}</span>
            <span class="text-gray-300">{{ t.name }}</span>
            <span v-if="t.error" class="text-gray-500">— {{ t.error }}</span>
          </div>
        </div>

        <pre class="mt-2 max-h-64 overflow-auto whitespace-pre-wrap text-gray-300">{{ result.body }}</pre>
      </template>
    </div>
  </template>
</template>

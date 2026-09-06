<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useCollectionStore } from '../stores/collections'
import { useSwitchStore } from '../stores/switches'
import { useHistoryStore } from '../stores/history'
import { executeRequest } from '../utils/requestExecutor'

const props = defineProps<{ collectionId: string }>()
const emit = defineEmits<{ close: [] }>()

const collectionStore = useCollectionStore()
const switchStore = useSwitchStore()
const historyStore = useHistoryStore()

const collection = computed(() => collectionStore.items.find((c) => c.id === props.collectionId))
const requests = computed(() => collectionStore.requestsByCollection[props.collectionId] ?? [])

type RunState = 'idle' | 'running' | 'passed' | 'failed' | 'error'
const states = ref<Record<string, RunState>>({})
const summaries = ref<Record<string, { status?: number; durationMs: number; testsPassed: number; testsTotal: number; error?: string }>>({})
const running = ref(false)

onMounted(async () => {
  if (!collectionStore.requestsByCollection[props.collectionId]) {
    await collectionStore.fetchRequests(props.collectionId)
  }
})

const methodColor: Record<string, string> = {
  GET: 'text-method-get',
  POST: 'text-method-post',
  PUT: 'text-method-put',
  PATCH: 'text-method-patch',
  DELETE: 'text-method-delete',
}

const totalPassed = computed(() => Object.values(summaries.value).filter((s) => s.testsTotal > 0 && s.testsPassed === s.testsTotal).length)
const totalRun = computed(() => Object.keys(summaries.value).length)

async function runAll() {
  running.value = true
  states.value = {}
  summaries.value = {}

  for (const request of requests.value) {
    states.value[request.id] = 'running'
    try {
      const { fullUrl, requestHeaders, requestBody, response, testResults } = await executeRequest(
        request,
        collection.value?.auth ?? null,
        (key) => switchStore.resolve(key),
      )

      const passed = testResults.filter((t) => t.passed).length
      const ok = response.ok && (testResults.length === 0 || passed === testResults.length)
      states.value[request.id] = ok ? 'passed' : testResults.length > 0 ? 'failed' : response.ok ? 'passed' : 'error'
      summaries.value[request.id] = {
        status: response.status,
        durationMs: response.durationMs,
        testsPassed: passed,
        testsTotal: testResults.length,
        error: response.error,
      }

      await historyStore.record({
        method: request.method,
        url: fullUrl,
        status: response.status ?? null,
        durationMs: response.durationMs,
        request: { method: request.method, url: fullUrl, headers: requestHeaders, body: requestBody },
        response,
      })
    } catch (e) {
      states.value[request.id] = 'error'
      summaries.value[request.id] = { durationMs: 0, testsPassed: 0, testsTotal: 0, error: e instanceof Error ? e.message : String(e) }
    }
  }

  running.value = false
}
</script>

<template>
  <div class="absolute inset-0 z-20 flex items-center justify-center bg-black/40" @click.self="emit('close')">
    <div class="flex max-h-[80vh] w-[min(90vw,600px)] flex-col rounded-lg border border-border-subtle bg-surface-1 shadow-2xl">
      <div class="flex items-center justify-between border-b border-border-subtle px-4 py-3">
        <span class="text-sm font-semibold text-gray-100">Collection Runner — {{ collection?.name }}</span>
        <button class="text-gray-500 hover:text-gray-200" @click="emit('close')">✕</button>
      </div>

      <div class="flex items-center justify-between border-b border-border-subtle px-4 py-3">
        <span class="text-xs text-gray-500">{{ requests.length }} ta so'rov</span>
        <button
          class="rounded-md bg-brand-500 px-4 py-1.5 text-xs font-semibold text-white transition hover:bg-brand-600 disabled:opacity-50"
          :disabled="running || requests.length === 0"
          @click="runAll"
        >
          {{ running ? 'Ishlamoqda…' : 'Run all' }}
        </button>
      </div>

      <div class="flex-1 overflow-y-auto">
        <div v-if="requests.length === 0" class="p-4 text-center text-xs text-gray-500">Bu collection'da so'rov yo'q</div>
        <div
          v-for="req in requests"
          :key="req.id"
          class="flex items-center gap-2 border-b border-border-subtle px-4 py-2 text-xs"
        >
          <span class="w-12 shrink-0 font-mono font-semibold" :class="methodColor[req.method] ?? 'text-gray-400'">{{ req.method }}</span>
          <span class="flex-1 truncate text-gray-300">{{ req.name }}</span>

          <template v-if="states[req.id] === 'running'">
            <span class="text-gray-500">…</span>
          </template>
          <template v-else-if="summaries[req.id]">
            <span :class="states[req.id] === 'passed' ? 'text-method-get' : 'text-method-delete'">
              {{ summaries[req.id].error ? 'xato' : summaries[req.id].status }}
            </span>
            <span class="text-gray-500">{{ summaries[req.id].durationMs }}ms</span>
            <span v-if="summaries[req.id].testsTotal > 0" :class="summaries[req.id].testsPassed === summaries[req.id].testsTotal ? 'text-method-get' : 'text-method-delete'">
              {{ summaries[req.id].testsPassed }}/{{ summaries[req.id].testsTotal }} test
            </span>
          </template>
        </div>
      </div>

      <div v-if="totalRun > 0" class="border-t border-border-subtle px-4 py-2 text-xs text-gray-400">
        {{ totalPassed }}/{{ totalRun }} so'rov to'liq muvaffaqiyatli
      </div>
    </div>
  </div>
</template>

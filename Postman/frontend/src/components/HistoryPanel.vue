<script setup lang="ts">
import { onMounted } from 'vue'
import { useHistoryStore } from '../stores/history'
import { useCollectionStore } from '../stores/collections'

const emit = defineEmits<{ close: [] }>()

const historyStore = useHistoryStore()
const collectionStore = useCollectionStore()

onMounted(() => historyStore.fetch())

const methodColor: Record<string, string> = {
  GET: 'text-method-get',
  POST: 'text-method-post',
  PUT: 'text-method-put',
  PATCH: 'text-method-patch',
  DELETE: 'text-method-delete',
}

function reuse(entry: (typeof historyStore.entries)[number]) {
  const current = collectionStore.selectedRequest
  if (!current) return
  current.method = entry.method
  current.url = entry.url
}

function timeAgo(iso: string) {
  const diffMs = Date.now() - new Date(iso).getTime()
  const mins = Math.floor(diffMs / 60000)
  if (mins < 1) return 'hozir'
  if (mins < 60) return `${mins} daq oldin`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `${hours} soat oldin`
  return `${Math.floor(hours / 24)} kun oldin`
}
</script>

<template>
  <div class="absolute right-0 top-0 z-10 flex h-full w-80 flex-col border-l border-border-subtle bg-surface-1 shadow-xl">
    <div class="flex items-center justify-between border-b border-border-subtle px-3 py-2">
      <span class="text-xs font-semibold uppercase tracking-wide text-gray-500">History</span>
      <button class="text-gray-500 hover:text-gray-200" @click="emit('close')">✕</button>
    </div>

    <div class="flex-1 overflow-y-auto">
      <div v-if="historyStore.entries.length === 0" class="p-4 text-center text-xs text-gray-500">
        Hali tarix yo'q
      </div>
      <button
        v-for="entry in historyStore.entries"
        :key="entry.id"
        class="flex w-full flex-col gap-0.5 border-b border-border-subtle px-3 py-2 text-left text-xs hover:bg-surface-2"
        title="Tanlangan so'rovga metod+URL'ni yuklash"
        @click="reuse(entry)"
      >
        <div class="flex items-center gap-2">
          <span class="w-10 shrink-0 font-mono font-semibold" :class="methodColor[entry.method] ?? 'text-gray-400'">{{ entry.method }}</span>
          <span class="truncate font-mono text-gray-300">{{ entry.url }}</span>
        </div>
        <div class="flex items-center gap-2 pl-12 text-gray-500">
          <span :class="entry.status && entry.status < 400 ? 'text-method-get' : 'text-method-delete'">{{ entry.status ?? 'xato' }}</span>
          <span>{{ entry.durationMs }}ms</span>
          <span>·</span>
          <span>{{ timeAgo(entry.executedAt) }}</span>
        </div>
      </button>
    </div>
  </div>
</template>

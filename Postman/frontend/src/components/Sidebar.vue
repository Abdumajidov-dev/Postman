<script setup lang="ts">
import { ref } from 'vue'
import { useCollectionStore } from '../stores/collections'

const store = useCollectionStore()

const creatingCollection = ref(false)
const newCollectionName = ref('')
const creatingRequestFor = ref<string | null>(null)
const newRequestName = ref('')

function startCreateCollection() {
  creatingCollection.value = true
  newCollectionName.value = ''
}

async function confirmCreateCollection() {
  await store.create(newCollectionName.value)
  creatingCollection.value = false
}

function startCreateRequest(collectionId: string) {
  creatingRequestFor.value = collectionId
  newRequestName.value = ''
}

async function confirmCreateRequest(collectionId: string) {
  await store.createRequest(collectionId, newRequestName.value)
  creatingRequestFor.value = null
}

const methodColor: Record<string, string> = {
  GET: 'text-method-get',
  POST: 'text-method-post',
  PUT: 'text-method-put',
  PATCH: 'text-method-patch',
  DELETE: 'text-method-delete',
}
</script>

<template>
  <aside class="w-64 shrink-0 overflow-y-auto border-r border-border-subtle bg-surface-1 p-3">
    <div class="flex items-center justify-between">
      <span class="text-xs font-semibold uppercase tracking-wide text-gray-500">Collections</span>
      <button
        class="flex h-6 w-6 items-center justify-center rounded-md text-lg leading-none text-gray-400 transition hover:bg-surface-2 hover:text-brand-400"
        title="Yangi collection"
        @click="startCreateCollection"
      >
        +
      </button>
    </div>

    <div v-if="creatingCollection" class="mt-2 flex gap-1">
      <input
        v-model="newCollectionName"
        autofocus
        class="w-full rounded-md border border-border-subtle bg-surface-2 px-2 py-1 text-xs text-gray-200 outline-none focus:border-brand-500"
        placeholder="Collection nomi"
        @keyup.enter="confirmCreateCollection"
        @keyup.esc="creatingCollection = false"
      />
    </div>

    <div v-if="store.items.length === 0 && !creatingCollection" class="mt-3 rounded-md border border-dashed border-border-subtle p-4 text-center text-xs text-gray-500">
      Hali collection yo'q — "+" bosib yarating
    </div>

    <ul v-else class="mt-2 space-y-0.5">
      <li v-for="col in store.items" :key="col.id">
        <div class="group flex items-center justify-between rounded-md px-2 py-1.5 text-sm text-gray-300 hover:bg-surface-2">
          <button class="flex flex-1 items-center gap-1.5 text-left" @click="store.toggleExpand(col.id)">
            <span class="w-3 text-xs text-gray-500">{{ store.expanded[col.id] ? '▾' : '▸' }}</span>
            <span>{{ col.name }}</span>
          </button>
          <div class="hidden items-center gap-1 group-hover:flex">
            <button class="text-xs text-gray-500 hover:text-brand-400" title="Yangi so'rov" @click="startCreateRequest(col.id)">+</button>
            <button class="text-xs text-gray-500 hover:text-method-delete" title="O'chirish" @click="store.remove(col.id)">✕</button>
          </div>
        </div>

        <div v-if="creatingRequestFor === col.id" class="ml-5 mt-1">
          <input
            v-model="newRequestName"
            autofocus
            class="w-full rounded-md border border-border-subtle bg-surface-2 px-2 py-1 text-xs text-gray-200 outline-none focus:border-brand-500"
            placeholder="So'rov nomi"
            @keyup.enter="confirmCreateRequest(col.id)"
            @keyup.esc="creatingRequestFor = null"
          />
        </div>

        <ul v-if="store.expanded[col.id]" class="ml-4 mt-0.5 space-y-0.5 border-l border-border-subtle pl-2">
          <li
            v-for="req in store.requestsByCollection[col.id] ?? []"
            :key="req.id"
            class="flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-xs hover:bg-surface-2"
            :class="store.selectedRequestId === req.id ? 'bg-surface-2' : ''"
            @click="store.selectRequest(req.id)"
          >
            <span class="w-10 shrink-0 font-mono font-semibold" :class="methodColor[req.method] ?? 'text-gray-400'">{{ req.method }}</span>
            <span class="truncate text-gray-300">{{ req.name }}</span>
          </li>
          <li v-if="(store.requestsByCollection[col.id] ?? []).length === 0" class="px-2 py-1 text-xs text-gray-600">
            So'rov yo'q
          </li>
        </ul>
      </li>
    </ul>
  </aside>
</template>

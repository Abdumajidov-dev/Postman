<script setup lang="ts">
import { ref } from 'vue'
import { useCollectionStore } from '../stores/collections'
import FolderNode from './FolderNode.vue'

const store = useCollectionStore()

const creatingCollection = ref(false)
const newCollectionName = ref('')
const importing = ref(false)
const exportingId = ref<string | null>(null)

function startCreateCollection() {
  creatingCollection.value = true
  newCollectionName.value = ''
}

async function confirmCreateCollection() {
  await store.create(newCollectionName.value)
  creatingCollection.value = false
}

async function importCollection() {
  importing.value = true
  try {
    const result = await window.pochtachi.openFile()
    if (result.ok && result.content) {
      await store.importCollection(result.content)
    }
  } finally {
    importing.value = false
  }
}

async function exportCollection(id: string, name: string) {
  exportingId.value = id
  try {
    const json = await store.exportCollection(id)
    await window.pochtachi.saveFile({ defaultName: `${name}.postman_collection.json`, content: json })
  } finally {
    exportingId.value = null
  }
}
</script>

<template>
  <aside class="w-64 shrink-0 overflow-y-auto border-r border-border-subtle bg-surface-1 p-3">
    <div class="flex items-center justify-between">
      <span class="text-xs font-semibold uppercase tracking-wide text-gray-500">Collections</span>
      <div class="flex items-center gap-1">
        <button
          class="flex h-6 w-6 items-center justify-center rounded-md text-xs text-gray-400 transition hover:bg-surface-2 hover:text-brand-400 disabled:opacity-50"
          title="Postman collection import qilish"
          :disabled="importing"
          @click="importCollection"
        >
          ⇩
        </button>
        <button
          class="flex h-6 w-6 items-center justify-center rounded-md text-lg leading-none text-gray-400 transition hover:bg-surface-2 hover:text-brand-400"
          title="Yangi collection"
          @click="startCreateCollection"
        >
          +
        </button>
      </div>
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
      Hali collection yo'q — "+" bosib yarating yoki Postman'dan import qiling
    </div>

    <ul v-else class="mt-2 space-y-0.5">
      <li v-for="col in store.items" :key="col.id">
        <div class="group flex items-center justify-between rounded-md px-2 py-1.5 text-sm text-gray-300 hover:bg-surface-2">
          <button class="flex flex-1 items-center gap-1.5 text-left" @click="store.toggleExpand(col.id)">
            <span class="w-3 text-xs text-gray-500">{{ store.expanded[col.id] ? '▾' : '▸' }}</span>
            <span>{{ col.name }}</span>
          </button>
          <div class="hidden items-center gap-1 group-hover:flex">
            <button
              class="text-xs text-gray-500 hover:text-brand-400 disabled:opacity-50"
              title="Postman formatida eksport"
              :disabled="exportingId === col.id"
              @click="exportCollection(col.id, col.name)"
            >⇧</button>
            <button class="text-xs text-gray-500 hover:text-method-delete" title="O'chirish" @click="store.remove(col.id)">✕</button>
          </div>
        </div>

        <FolderNode v-if="store.expanded[col.id]" :collection-id="col.id" :parent-folder-id="null" :depth="0" />
      </li>
    </ul>
  </aside>
</template>

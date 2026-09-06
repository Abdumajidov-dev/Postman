<script setup lang="ts">
import { computed, ref } from 'vue'
import { useCollectionStore } from '../stores/collections'

const props = defineProps<{
  collectionId: string
  parentFolderId: string | null
  depth: number
}>()

const store = useCollectionStore()

const childFolders = computed(() =>
  (store.foldersByCollection[props.collectionId] ?? []).filter((f) => f.parentFolderId === props.parentFolderId),
)
const childRequests = computed(() =>
  (store.requestsByCollection[props.collectionId] ?? []).filter((r) => r.folderId === props.parentFolderId),
)

const methodColor: Record<string, string> = {
  GET: 'text-method-get',
  POST: 'text-method-post',
  PUT: 'text-method-put',
  PATCH: 'text-method-patch',
  DELETE: 'text-method-delete',
}

const creatingFolder = ref(false)
const creatingRequest = ref(false)
const newName = ref('')

async function confirmCreateFolder() {
  await store.createFolder(props.collectionId, newName.value, props.parentFolderId)
  creatingFolder.value = false
}

async function confirmCreateRequest() {
  await store.createRequest(props.collectionId, newName.value, props.parentFolderId)
  creatingRequest.value = false
}
</script>

<template>
  <div :style="{ marginLeft: depth > 0 ? '0.75rem' : '0' }" :class="depth > 0 ? 'border-l border-border-subtle pl-2' : ''">
    <div class="flex gap-2 px-2 py-0.5 text-[11px] text-gray-600">
      <button class="hover:text-brand-400" @click="creatingFolder = true">+ papka</button>
      <button class="hover:text-brand-400" @click="creatingRequest = true">+ so'rov</button>
    </div>

    <div v-if="creatingFolder" class="px-2 pb-1">
      <input
        v-model="newName"
        autofocus
        class="w-full rounded-md border border-border-subtle bg-surface-2 px-2 py-1 text-xs text-gray-200 outline-none focus:border-brand-500"
        placeholder="Papka nomi"
        @keyup.enter="confirmCreateFolder"
        @keyup.esc="creatingFolder = false"
      />
    </div>
    <div v-if="creatingRequest" class="px-2 pb-1">
      <input
        v-model="newName"
        autofocus
        class="w-full rounded-md border border-border-subtle bg-surface-2 px-2 py-1 text-xs text-gray-200 outline-none focus:border-brand-500"
        placeholder="So'rov nomi"
        @keyup.enter="confirmCreateRequest"
        @keyup.esc="creatingRequest = false"
      />
    </div>

    <ul class="space-y-0.5">
      <li v-for="folder in childFolders" :key="folder.id">
        <button
          class="flex w-full items-center gap-1.5 rounded-md px-2 py-1 text-left text-xs text-gray-300 hover:bg-surface-2"
          @click="store.toggleExpandFolder(folder.id)"
        >
          <span class="w-3 text-gray-500">{{ store.expandedFolders[folder.id] ? '▾' : '▸' }}</span>
          <span>📁 {{ folder.name }}</span>
        </button>

        <FolderNode
          v-if="store.expandedFolders[folder.id]"
          :collection-id="collectionId"
          :parent-folder-id="folder.id"
          :depth="depth + 1"
        />
      </li>

      <li
        v-for="req in childRequests"
        :key="req.id"
        class="flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-xs hover:bg-surface-2"
        :class="store.selectedRequestId === req.id ? 'bg-surface-2' : ''"
        @click="store.selectRequest(req.id)"
      >
        <span class="w-10 shrink-0 font-mono font-semibold" :class="methodColor[req.method] ?? 'text-gray-400'">{{ req.method }}</span>
        <span class="truncate text-gray-300">{{ req.name }}</span>
      </li>
    </ul>
  </div>
</template>

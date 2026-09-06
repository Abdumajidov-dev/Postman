<script setup lang="ts">
import { onMounted } from 'vue'
import SwitchBar from './components/SwitchBar.vue'
import RequestBar from './components/RequestBar.vue'
import Sidebar from './components/Sidebar.vue'
import { useCollectionStore } from './stores/collections'
import { useSwitchStore } from './stores/switches'

const collectionStore = useCollectionStore()
const switchStore = useSwitchStore()

onMounted(async () => {
  await Promise.all([collectionStore.fetchAll(), switchStore.fetchAll()])
})
</script>

<template>
  <div class="flex h-full flex-col bg-surface-0 text-gray-200">
    <SwitchBar />

    <div class="flex flex-1 overflow-hidden">
      <Sidebar />

      <main class="flex flex-1 flex-col overflow-hidden">
        <RequestBar />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import SwitchBar from './components/SwitchBar.vue'
import RequestBar from './components/RequestBar.vue'
import Sidebar from './components/Sidebar.vue'
import HistoryPanel from './components/HistoryPanel.vue'
import { useCollectionStore } from './stores/collections'
import { useSwitchStore } from './stores/switches'

const collectionStore = useCollectionStore()
const switchStore = useSwitchStore()
const showHistory = ref(false)

onMounted(async () => {
  await Promise.all([collectionStore.fetchAll(), switchStore.fetchAll()])
})
</script>

<template>
  <div class="relative flex h-full flex-col bg-surface-0 text-gray-200">
    <SwitchBar @toggle-history="showHistory = !showHistory" />

    <div class="flex flex-1 overflow-hidden">
      <Sidebar />

      <main class="flex flex-1 flex-col overflow-hidden">
        <RequestBar />
      </main>
    </div>

    <HistoryPanel v-if="showHistory" @close="showHistory = false" />
  </div>
</template>

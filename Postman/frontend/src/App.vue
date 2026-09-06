<script setup lang="ts">
import { onMounted, ref } from 'vue'
import SwitchBar from './components/SwitchBar.vue'
import RequestBar from './components/RequestBar.vue'
import Sidebar from './components/Sidebar.vue'
import HistoryPanel from './components/HistoryPanel.vue'
import VariablesPanel from './components/VariablesPanel.vue'
import { useCollectionStore } from './stores/collections'
import { useSwitchStore } from './stores/switches'

const collectionStore = useCollectionStore()
const switchStore = useSwitchStore()
const showHistory = ref(false)
const showVariables = ref(false)

onMounted(async () => {
  await Promise.all([collectionStore.fetchAll(), switchStore.fetchAll()])
})
</script>

<template>
  <div class="relative flex h-full flex-col bg-surface-0 text-gray-200">
    <SwitchBar @toggle-history="showHistory = !showHistory" @toggle-variables="showVariables = !showVariables" />

    <div class="flex flex-1 overflow-hidden">
      <Sidebar />

      <main class="flex flex-1 flex-col overflow-hidden">
        <RequestBar />
      </main>
    </div>

    <HistoryPanel v-if="showHistory" @close="showHistory = false" />
    <VariablesPanel v-if="showVariables" @close="showVariables = false" />
  </div>
</template>

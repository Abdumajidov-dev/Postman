<script setup lang="ts">
import SwitchBar from './components/SwitchBar.vue'
import RequestBar from './components/RequestBar.vue'
</script>

<template>
  <div class="flex h-full flex-col bg-surface-0 text-gray-200">
    <SwitchBar />

    <div class="flex flex-1 overflow-hidden">
      <aside class="w-64 shrink-0 border-r border-border-subtle bg-surface-1 p-3">
        <div class="text-xs font-semibold uppercase tracking-wide text-gray-500">Collections</div>
        <div class="mt-3 rounded-md border border-dashed border-border-subtle p-4 text-center text-xs text-gray-500">
          Hali collection yo'q — "+" bosib yarating
        </div>
      </aside>

      <main class="flex flex-1 flex-col overflow-hidden">
        <RequestBar />
        <div class="flex-1 overflow-auto p-4 text-sm text-gray-500">
          So'rov yuboring — javob shu yerda ko'rinadi.
        </div>
      </main>
    </div>
  </div>
</template>

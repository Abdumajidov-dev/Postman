<script setup lang="ts">
import { useSwitchStore } from '../stores/switches'

const store = useSwitchStore()
</script>

<template>
  <div class="flex items-center gap-3 border-b border-border-subtle bg-surface-1 px-4 py-2">
    <span class="text-sm font-semibold text-brand-400">Pochtachi</span>
    <div class="h-4 w-px bg-border-subtle" />
    <div v-for="dim in store.dimensions" :key="dim.id" class="flex items-center gap-1.5">
      <span class="text-xs text-gray-500">{{ dim.name }}</span>
      <select
        class="rounded-md border border-border-subtle bg-surface-2 px-2 py-1 text-xs text-gray-200 outline-none focus:border-brand-500"
        :value="dim.activeOptionId ?? ''"
        @change="store.setActiveOption(dim.id, ($event.target as HTMLSelectElement).value)"
      >
        <option v-for="opt in dim.options" :key="opt.id" :value="opt.id">{{ opt.name }}</option>
      </select>
    </div>
  </div>
</template>

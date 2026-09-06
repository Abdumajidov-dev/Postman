<script setup lang="ts">
import { useSwitchStore } from '../stores/switches'
import { useAuthStore } from '../stores/auth'

defineEmits<{ 'toggle-history': []; 'toggle-variables': [] }>()

const store = useSwitchStore()
const auth = useAuthStore()
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

    <div class="flex-1" />
    <button
      class="rounded-md border border-border-subtle px-2 py-1 text-xs text-gray-300 transition hover:bg-surface-2"
      @click="$emit('toggle-variables')"
    >
      Variables
    </button>
    <button
      class="rounded-md border border-border-subtle px-2 py-1 text-xs text-gray-300 transition hover:bg-surface-2"
      @click="$emit('toggle-history')"
    >
      History
    </button>

    <div class="h-4 w-px bg-border-subtle" />
    <span class="text-xs text-gray-400">{{ auth.user?.fullName }}</span>
    <button class="text-xs text-gray-500 hover:text-method-delete" title="Chiqish" @click="auth.logout()">Chiqish</button>
  </div>
</template>

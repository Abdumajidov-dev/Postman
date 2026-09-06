<script setup lang="ts">
import { ref } from 'vue'
import { useSwitchStore } from '../stores/switches'

const emit = defineEmits<{ close: [] }>()
const store = useSwitchStore()

const creatingDimension = ref(false)
const newDimensionName = ref('')
const newDimensionOptions = ref('')

const addingOptionFor = ref<string | null>(null)
const newOptionName = ref('')

const creatingVariable = ref(false)
const newVariableKey = ref('')
const newVariableSecret = ref(false)

const revealed = ref<Record<string, boolean>>({})

async function confirmCreateDimension() {
  const names = newDimensionOptions.value.split(',').map((s) => s.trim()).filter(Boolean)
  if (!newDimensionName.value.trim() || names.length === 0) return
  await store.createDimension(newDimensionName.value.trim(), names)
  creatingDimension.value = false
  newDimensionName.value = ''
  newDimensionOptions.value = ''
}

async function confirmAddOption(dimensionId: string) {
  if (!newOptionName.value.trim()) return
  await store.addOption(dimensionId, newOptionName.value.trim())
  addingOptionFor.value = null
  newOptionName.value = ''
}

async function confirmCreateVariable() {
  if (!newVariableKey.value.trim()) return
  await store.createVariable(newVariableKey.value.trim(), newVariableSecret.value)
  creatingVariable.value = false
  newVariableKey.value = ''
  newVariableSecret.value = false
}

function cellValue(variable: (typeof store.variables)[number], optionId: string) {
  return variable.valuesByOptionId[optionId] ?? ''
}

let debounceTimer: ReturnType<typeof setTimeout> | undefined
function onCellInput(variableId: string, optionId: string | null, value: string) {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => store.setVariableValue(variableId, optionId, value), 400)
}
</script>

<template>
  <div class="absolute inset-0 z-20 flex items-center justify-center bg-black/40" @click.self="emit('close')">
    <div class="flex max-h-[80vh] w-[min(90vw,900px)] flex-col rounded-lg border border-border-subtle bg-surface-1 shadow-2xl">
      <div class="flex items-center justify-between border-b border-border-subtle px-4 py-3">
        <span class="text-sm font-semibold text-gray-100">O'zgaruvchilar va Switch'lar</span>
        <button class="text-gray-500 hover:text-gray-200" @click="emit('close')">✕</button>
      </div>

      <div class="overflow-y-auto p-4">
        <!-- Dimensions -->
        <div class="mb-6">
          <div class="mb-2 flex items-center justify-between">
            <span class="text-xs font-semibold uppercase tracking-wide text-gray-500">Switch o'lchamlari</span>
            <button class="text-xs text-brand-400 hover:underline" @click="creatingDimension = true">+ yangi o'lcham</button>
          </div>

          <div v-if="creatingDimension" class="mb-3 flex gap-2">
            <input v-model="newDimensionName" placeholder="Masalan: Environment" class="w-40 rounded border border-border-subtle bg-surface-2 px-2 py-1 text-xs text-gray-200 outline-none" />
            <input v-model="newDimensionOptions" placeholder="Local, Global" class="flex-1 rounded border border-border-subtle bg-surface-2 px-2 py-1 text-xs text-gray-200 outline-none" @keyup.enter="confirmCreateDimension" />
            <button class="text-xs text-brand-400 hover:underline" @click="confirmCreateDimension">Saqlash</button>
            <button class="text-xs text-gray-500 hover:underline" @click="creatingDimension = false">Bekor</button>
          </div>

          <div v-for="dim in store.dimensions" :key="dim.id" class="mb-2 flex flex-wrap items-center gap-2 text-xs">
            <span class="font-semibold text-gray-300">{{ dim.name }}:</span>
            <span v-for="opt in dim.options" :key="opt.id" class="rounded-full bg-surface-2 px-2 py-0.5 text-gray-300">{{ opt.name }}</span>

            <template v-if="addingOptionFor === dim.id">
              <input v-model="newOptionName" class="w-24 rounded border border-border-subtle bg-surface-2 px-2 py-0.5 text-gray-200 outline-none" @keyup.enter="confirmAddOption(dim.id)" />
              <button class="text-brand-400 hover:underline" @click="confirmAddOption(dim.id)">✓</button>
            </template>
            <button v-else class="text-gray-500 hover:text-brand-400" @click="addingOptionFor = dim.id; newOptionName = ''">+ variant</button>
          </div>

          <p v-if="store.dimensions.length === 0 && !creatingDimension" class="text-xs text-gray-500">
            Hali switch o'lchami yo'q. "Environment" (Local/Global) yoki "Role" (Admin/User) kabi yarating.
          </p>
        </div>

        <!-- Variables -->
        <div>
          <div class="mb-2 flex items-center justify-between">
            <span class="text-xs font-semibold uppercase tracking-wide text-gray-500">O'zgaruvchilar</span>
            <button class="text-xs text-brand-400 hover:underline" @click="creatingVariable = true">+ yangi o'zgaruvchi</button>
          </div>

          <div v-if="creatingVariable" class="mb-3 flex items-center gap-2">
            <input v-model="newVariableKey" placeholder="baseUrl" class="w-40 rounded border border-border-subtle bg-surface-2 px-2 py-1 font-mono text-xs text-gray-200 outline-none" @keyup.enter="confirmCreateVariable" />
            <label class="flex items-center gap-1 text-xs text-gray-400">
              <input type="checkbox" v-model="newVariableSecret" /> Secret
            </label>
            <button class="text-xs text-brand-400 hover:underline" @click="confirmCreateVariable">Saqlash</button>
            <button class="text-xs text-gray-500 hover:underline" @click="creatingVariable = false">Bekor</button>
          </div>

          <div class="overflow-x-auto">
            <table class="w-full min-w-[500px] text-xs">
              <thead>
                <tr class="text-left text-gray-500">
                  <th class="pb-2 pr-3 font-medium">Key</th>
                  <th v-for="dim in store.dimensions" :key="dim.id" :colspan="dim.options.length" class="pb-1 text-center font-medium">{{ dim.name }}</th>
                  <th class="pb-2 font-medium">Default</th>
                  <th class="w-6"></th>
                </tr>
                <tr class="text-left text-gray-600">
                  <th></th>
                  <template v-for="dim in store.dimensions" :key="dim.id">
                    <th v-for="opt in dim.options" :key="opt.id" class="pb-2 pr-2 font-normal">{{ opt.name }}</th>
                  </template>
                  <th></th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="variable in store.variables" :key="variable.id" class="border-t border-border-subtle">
                  <td class="py-1.5 pr-3 font-mono text-gray-300">
                    {{ variable.key }}
                    <span v-if="variable.isSecret" class="ml-1 text-gray-600">🔒</span>
                  </td>
                  <template v-for="dim in store.dimensions" :key="dim.id">
                    <td v-for="opt in dim.options" :key="opt.id" class="pr-2 py-1">
                      <input
                        :type="variable.isSecret && !revealed[variable.id] ? 'password' : 'text'"
                        :value="cellValue(variable, opt.id)"
                        class="w-28 rounded border border-border-subtle bg-surface-2 px-1.5 py-1 font-mono text-gray-200 outline-none focus:border-brand-500"
                        @input="onCellInput(variable.id, opt.id, ($event.target as HTMLInputElement).value)"
                      />
                    </td>
                  </template>
                  <td class="pr-2 py-1">
                    <input
                      :type="variable.isSecret && !revealed[variable.id] ? 'password' : 'text'"
                      :value="variable.valuesByOptionId['default'] ?? ''"
                      class="w-28 rounded border border-border-subtle bg-surface-2 px-1.5 py-1 font-mono text-gray-200 outline-none focus:border-brand-500"
                      @input="onCellInput(variable.id, null, ($event.target as HTMLInputElement).value)"
                    />
                  </td>
                  <td class="flex items-center gap-1 py-1">
                    <button v-if="variable.isSecret" class="text-gray-500 hover:text-brand-400" @click="revealed[variable.id] = !revealed[variable.id]">👁</button>
                    <button class="text-gray-500 hover:text-method-delete" @click="store.deleteVariable(variable.id)">✕</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <p v-if="store.variables.length === 0" class="mt-2 text-xs text-gray-500">
            Hali o'zgaruvchi yo'q. Masalan "baseUrl" yarating va Environment ustunlariga har xil qiymat bering — pastdagi
            "Default" ustuni hech qanday switch tanlanmaganda ishlatiladi.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

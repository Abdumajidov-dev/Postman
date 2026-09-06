<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'

const emit = defineEmits<{ success: [] }>()

const auth = useAuthStore()
const mode = ref<'login' | 'register'>('login')
const email = ref('')
const password = ref('')
const fullName = ref('')
const submitting = ref(false)

async function submit() {
  submitting.value = true
  try {
    const ok = mode.value === 'login'
      ? await auth.login(email.value, password.value)
      : await auth.register(email.value, password.value, fullName.value)
    if (ok) emit('success')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="flex h-full items-center justify-center bg-surface-0">
    <div class="w-80 rounded-lg border border-border-subtle bg-surface-1 p-6">
      <div class="mb-6 text-center">
        <div class="text-lg font-semibold text-brand-400">Pochtachi</div>
        <div class="text-xs text-gray-500">{{ mode === 'login' ? 'Hisobingizga kiring' : "Yangi hisob yarating" }}</div>
      </div>

      <form class="space-y-3" @submit.prevent="submit">
        <input
          v-if="mode === 'register'"
          v-model="fullName"
          required
          placeholder="Ism familiya"
          class="w-full rounded-md border border-border-subtle bg-surface-2 px-3 py-2 text-sm text-gray-200 outline-none focus:border-brand-500"
        />
        <input
          v-model="email"
          type="email"
          required
          placeholder="Email"
          class="w-full rounded-md border border-border-subtle bg-surface-2 px-3 py-2 text-sm text-gray-200 outline-none focus:border-brand-500"
        />
        <input
          v-model="password"
          type="password"
          required
          minlength="6"
          placeholder="Parol"
          class="w-full rounded-md border border-border-subtle bg-surface-2 px-3 py-2 text-sm text-gray-200 outline-none focus:border-brand-500"
        />

        <p v-if="auth.error" class="text-xs text-method-delete">{{ auth.error }}</p>

        <button
          type="submit"
          class="w-full rounded-md bg-brand-500 px-3 py-2 text-sm font-semibold text-white transition hover:bg-brand-600 disabled:opacity-50"
          :disabled="submitting"
        >
          {{ submitting ? 'Kutilmoqda…' : mode === 'login' ? 'Kirish' : "Ro'yxatdan o'tish" }}
        </button>
      </form>

      <button
        class="mt-4 w-full text-center text-xs text-gray-500 hover:text-brand-400"
        @click="mode = mode === 'login' ? 'register' : 'login'; auth.error = null"
      >
        {{ mode === 'login' ? "Hisobingiz yo'qmi? Ro'yxatdan o'ting" : "Hisobingiz bormi? Kiring" }}
      </button>
    </div>
  </div>
</template>

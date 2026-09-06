<script setup lang="ts">
import type { AuthConfig } from '../stores/collections'

const props = defineProps<{ modelValue: AuthConfig | null }>()
const emit = defineEmits<{ 'update:modelValue': [AuthConfig | null] }>()

const authTypes: { value: AuthConfig['type']; label: string }[] = [
  { value: 'NoAuth', label: 'No Auth' },
  { value: 'Bearer', label: 'Bearer Token' },
  { value: 'Basic', label: 'Basic Auth' },
  { value: 'ApiKey', label: 'API Key' },
  { value: 'OAuth2', label: 'OAuth 2.0' },
  { value: 'Digest', label: 'Digest Auth' },
]

function setType(type: AuthConfig['type']) {
  if (type === 'NoAuth') {
    emit('update:modelValue', null)
    return
  }
  const defaults: Record<string, Record<string, string>> = {
    Bearer: { token: '' },
    Basic: { username: '', password: '' },
    ApiKey: { key: '', value: '', in: 'header' },
    OAuth2: { grantType: 'client_credentials', accessTokenUrl: '', clientId: '', clientSecret: '', accessToken: '' },
    Digest: { username: '', password: '' },
  }
  emit('update:modelValue', { type, values: props.modelValue?.type === type ? props.modelValue.values : (defaults[type] ?? {}) })
}

function setValue(key: string, value: string) {
  if (!props.modelValue) return
  emit('update:modelValue', { ...props.modelValue, values: { ...props.modelValue.values, [key]: value } })
}
</script>

<template>
  <div class="max-w-md space-y-3">
    <select
      :value="modelValue?.type ?? 'NoAuth'"
      class="w-full rounded-md border border-border-subtle bg-surface-2 px-2 py-2 text-xs text-gray-200 outline-none"
      @change="setType(($event.target as HTMLSelectElement).value as AuthConfig['type'])"
    >
      <option v-for="t in authTypes" :key="t.value" :value="t.value">{{ t.label }}</option>
    </select>

    <template v-if="modelValue?.type === 'Bearer'">
      <label class="block text-xs text-gray-500">Token</label>
      <input :value="modelValue.values.token" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" placeholder="{{authToken}}" @input="setValue('token', ($event.target as HTMLInputElement).value)" />
    </template>

    <template v-else-if="modelValue?.type === 'Basic'">
      <label class="block text-xs text-gray-500">Username</label>
      <input :value="modelValue.values.username" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 text-xs text-gray-200 outline-none" @input="setValue('username', ($event.target as HTMLInputElement).value)" />
      <label class="block text-xs text-gray-500">Password</label>
      <input :value="modelValue.values.password" type="password" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 text-xs text-gray-200 outline-none" @input="setValue('password', ($event.target as HTMLInputElement).value)" />
    </template>

    <template v-else-if="modelValue?.type === 'ApiKey'">
      <label class="block text-xs text-gray-500">Key</label>
      <input :value="modelValue.values.key" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" @input="setValue('key', ($event.target as HTMLInputElement).value)" />
      <label class="block text-xs text-gray-500">Value</label>
      <input :value="modelValue.values.value" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" @input="setValue('value', ($event.target as HTMLInputElement).value)" />
      <label class="block text-xs text-gray-500">Qo'shish joyi</label>
      <select :value="modelValue.values.in" class="w-full rounded-md border border-border-subtle bg-surface-2 px-2 py-1.5 text-xs text-gray-200 outline-none" @change="setValue('in', ($event.target as HTMLSelectElement).value)">
        <option value="header">Header</option>
        <option value="query">Query Param</option>
      </select>
    </template>

    <template v-else-if="modelValue?.type === 'OAuth2'">
      <label class="block text-xs text-gray-500">Access Token (tayyor bo'lsa, to'g'ridan-to'g'ri shu ishlatiladi)</label>
      <input :value="modelValue.values.accessToken" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" @input="setValue('accessToken', ($event.target as HTMLInputElement).value)" />
      <p class="text-xs text-gray-600">Access Token bo'sh bo'lsa, Client Credentials orqali avtomatik olinadi:</p>
      <label class="block text-xs text-gray-500">Token URL</label>
      <input :value="modelValue.values.accessTokenUrl" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" @input="setValue('accessTokenUrl', ($event.target as HTMLInputElement).value)" />
      <label class="block text-xs text-gray-500">Client ID</label>
      <input :value="modelValue.values.clientId" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" @input="setValue('clientId', ($event.target as HTMLInputElement).value)" />
      <label class="block text-xs text-gray-500">Client Secret</label>
      <input :value="modelValue.values.clientSecret" type="password" class="w-full rounded border border-border-subtle bg-surface-2 px-2 py-1.5 font-mono text-xs text-gray-200 outline-none" @input="setValue('clientSecret', ($event.target as HTMLInputElement).value)" />
    </template>

    <p v-else-if="modelValue?.type === 'Digest'" class="text-xs text-gray-500">
      Digest sozlamalari saqlanadi, lekin server challenge-response hisoblash keyingi versiyada qo'shiladi.
    </p>
  </div>
</template>

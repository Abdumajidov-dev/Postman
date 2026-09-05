import { defineStore } from 'pinia'

/**
 * "Switch Dimensions" — TZ 3.3.1. Har bir dimension (Environment, Role, ...) mustaqil
 * o'z aktiv variantiga ega. Bitta o'zgaruvchi shu variantlar bo'yicha turli qiymat
 * saqlaydi (masalan baseUrl: local/global; authToken: admin/user/superadmin).
 * Switch o'zgarganda — shu o'zgaruvchiga bog'liq barcha so'rovlar avtomatik yangi
 * qiymatni oladi, hech narsani qo'lda tuzatish shart emas.
 */
export interface SwitchOption {
  id: string
  name: string
}

export interface SwitchDimension {
  id: string
  name: string
  options: SwitchOption[]
  activeOptionId: string | null
}

export interface Variable {
  key: string
  isSecret: boolean
  /** optionId -> qiymat; "default" — hech qanday switch tanlanmaganda ishlatiladi */
  valuesByOption: Record<string, string>
}

export const useSwitchStore = defineStore('switches', {
  state: () => ({
    dimensions: [
      {
        id: 'env',
        name: 'Environment',
        options: [
          { id: 'local', name: 'Local' },
          { id: 'global', name: 'Global' },
        ],
        activeOptionId: 'local',
      },
      {
        id: 'role',
        name: 'Role',
        options: [
          { id: 'admin', name: 'Admin' },
          { id: 'user', name: 'User' },
          { id: 'superadmin', name: 'SuperAdmin' },
        ],
        activeOptionId: 'user',
      },
    ] as SwitchDimension[],
    variables: [
      {
        key: 'baseUrl',
        isSecret: false,
        valuesByOption: {
          local: 'http://localhost:5299',
          global: 'https://api.company.com',
        },
      },
      {
        key: 'authToken',
        isSecret: true,
        valuesByOption: {
          admin: 'admin-token-xxxx',
          user: 'user-token-xxxx',
          superadmin: 'superadmin-token-xxxx',
        },
      },
    ] as Variable[],
  }),
  actions: {
    setActiveOption(dimensionId: string, optionId: string) {
      const dim = this.dimensions.find((d) => d.id === dimensionId)
      if (dim) dim.activeOptionId = optionId
    },
    /** Joriy switch holatiga qarab o'zgaruvchi qiymatini resolve qiladi. */
    resolve(key: string): string | undefined {
      const variable = this.variables.find((v) => v.key === key)
      if (!variable) return undefined

      for (const dim of this.dimensions) {
        const optionId = dim.activeOptionId
        if (optionId && variable.valuesByOption[optionId] !== undefined) {
          return variable.valuesByOption[optionId]
        }
      }
      return variable.valuesByOption['default']
    },
  },
})

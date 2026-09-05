import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import electron from 'vite-plugin-electron/simple'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    tailwindcss(),
    electron({
      main: {
        entry: 'electron/main.ts',
      },
      preload: {
        input: 'electron/preload.ts',
      },
    }),
  ],
})

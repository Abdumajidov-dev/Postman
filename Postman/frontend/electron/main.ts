import { app, BrowserWindow, ipcMain, dialog } from 'electron'
import fs from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const isDev = !app.isPackaged

function createWindow() {
  const win = new BrowserWindow({
    width: 1400,
    height: 900,
    title: 'Pochtachi',
    backgroundColor: '#0f1115',
    webPreferences: {
      preload: path.join(__dirname, 'preload.mjs'),
      contextIsolation: true,
      nodeIntegration: false,
    },
  })

  if (isDev && process.env.VITE_DEV_SERVER_URL) {
    win.loadURL(process.env.VITE_DEV_SERVER_URL)
  } else {
    win.loadFile(path.join(__dirname, '../dist/index.html'))
  }
}

// Renderer'dan kelgan so'rovni Node process'da bajaramiz — CORS cheklovisiz,
// barcha metod/header/binary body qo'llab-quvvatlanadi.
ipcMain.handle('pochtachi:send-request', async (_event, config: {
  method: string
  url: string
  headers?: Record<string, string>
  body?: string
}) => {
  const startedAt = Date.now()
  try {
    const response = await fetch(config.url, {
      method: config.method,
      headers: config.headers,
      body: config.method === 'GET' || config.method === 'HEAD' ? undefined : config.body,
    })
    const text = await response.text()
    return {
      ok: true,
      status: response.status,
      statusText: response.statusText,
      headers: Object.fromEntries(response.headers.entries()),
      body: text,
      durationMs: Date.now() - startedAt,
    }
  } catch (error) {
    return {
      ok: false,
      error: error instanceof Error ? error.message : String(error),
      durationMs: Date.now() - startedAt,
    }
  }
})

// Renderer'dan berilgan matnni foydalanuvchi tanlagan faylga saqlaydi (export uchun).
ipcMain.handle('pochtachi:save-file', async (_event, options: { defaultName: string; content: string }) => {
  const win = BrowserWindow.getFocusedWindow()
  const result = win
    ? await dialog.showSaveDialog(win, { defaultPath: options.defaultName })
    : await dialog.showSaveDialog({ defaultPath: options.defaultName })

  if (result.canceled || !result.filePath) return { ok: false, canceled: true }

  await fs.writeFile(result.filePath, options.content, 'utf-8')
  return { ok: true, filePath: result.filePath }
})

// Foydalanuvchi tanlagan fayl matnini o'qiydi (import uchun).
ipcMain.handle('pochtachi:open-file', async () => {
  const win = BrowserWindow.getFocusedWindow()
  const result = win
    ? await dialog.showOpenDialog(win, { properties: ['openFile'], filters: [{ name: 'JSON', extensions: ['json'] }] })
    : await dialog.showOpenDialog({ properties: ['openFile'], filters: [{ name: 'JSON', extensions: ['json'] }] })

  if (result.canceled || result.filePaths.length === 0) return { ok: false, canceled: true }

  const content = await fs.readFile(result.filePaths[0], 'utf-8')
  return { ok: true, filePath: result.filePaths[0], content }
})

app.whenReady().then(createWindow)

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit()
})

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) createWindow()
})

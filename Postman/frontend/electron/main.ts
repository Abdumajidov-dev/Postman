import { app, BrowserWindow, ipcMain } from 'electron'
import path from 'node:path'

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

app.whenReady().then(createWindow)

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit()
})

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) createWindow()
})

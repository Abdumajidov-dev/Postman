import axios from 'axios'

// TODO: keyinchalik Settings ekranidan o'zgartiriladigan qilish (bir nechta backend/kompaniya uchun).
export const API_BASE_URL = 'http://localhost:5299'

export const api = axios.create({ baseURL: API_BASE_URL })

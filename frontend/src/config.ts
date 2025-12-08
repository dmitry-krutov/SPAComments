const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? 'http://localhost:5287'

export const config = {
  apiBaseUrl: apiBaseUrl.replace(/\/$/, ''),
}

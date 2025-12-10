const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? ''
const signalRUrl = (import.meta.env.VITE_SIGNALR_URL as string | undefined) ?? ''

export const config = {
  apiBaseUrl: apiBaseUrl.replace(/\/$/, ''),
  signalRUrl,
}

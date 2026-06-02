export interface ProxyData {
  proxy: string
  protocol: string
  ip: string
  port: number
}

export interface Session {
  psi: string
  cookie: string
  userAgent: string
  updatedAt: string
  secChUa?: string
  secChUaPlatform?: string
}

export interface HarvestResult {
  success: number
  failed: number
  errors: string[]
}

export interface ProxyTestResult {
  proxy: string
  ip: string
  port: number
  success: boolean
  durationMs: number
  error?: string
}

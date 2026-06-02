import { launch } from 'cloakbrowser/puppeteer'
import type { Browser } from 'puppeteer-core'

export interface BrowserSession {
  browser: Browser
}

export interface BrowserOptions {
  headless?: boolean
  humanPreset?: 'default' | 'careful'
  fingerprintSeed?: number
  geoip?: boolean
  secChUaBrand?: string
  secChUaBrandVersion?: string
  secChUaPlatform?: string
}

export async function createBrowserSession(
  proxyUrl: string,
  options?: BrowserOptions,
): Promise<BrowserSession> {
  const args: string[] = []

  if (options?.fingerprintSeed) {
    args.push(`--fingerprint=${options.fingerprintSeed}`)
  }
  if (options?.secChUaBrand) args.push(`--fingerprint-brand=${options.secChUaBrand}`)
  if (options?.secChUaBrandVersion) args.push(`--fingerprint-brand-version=${options.secChUaBrandVersion}`)
  if (options?.secChUaPlatform) args.push(`--fingerprint-platform=${options.secChUaPlatform}`)

  const browser = await launch({
    headless: options?.headless ?? true,
    proxy: proxyUrl,
    geoip: options?.geoip ?? true,
    humanize: true,
    humanPreset: options?.humanPreset ?? 'default',
    args: args.length > 0 ? args : undefined,
  })

  return { browser }
}

export async function closeBrowserSession(session: BrowserSession): Promise<void> {
  try {
    await session.browser.close()
  } catch {
    // ignore close errors
  }
}

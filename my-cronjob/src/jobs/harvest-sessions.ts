import { env } from '../config/env.js'
import { loadProxies } from '../lib/proxy.js'
import { createBrowserSession, closeBrowserSession } from '../lib/browser.js'
import { harvestSingleSession } from '../lib/session.js'
import { writeSessions } from '../lib/store.js'
import type { HarvestResult, Session } from '../types/index.js'

function parseArgs(): { count: number; headless: boolean; fingerprintSeed?: number } {
  const args = process.argv.slice(2)

  const countIdx = args.indexOf('--count')
  const count = countIdx !== -1 ? parseInt(args[countIdx + 1] ?? '', 10) : env.harvest.defaultCount

  const headless = args.includes('--headed') ? false : env.harvest.headless

  const fingerprintIdx = args.indexOf('--fingerprint')
  const fingerprintSeed = fingerprintIdx !== -1
    ? parseInt(args[fingerprintIdx + 1] ?? '', 10) || undefined
    : env.harvest.fingerprintSeed

  return {
    count: isNaN(count) ? env.harvest.defaultCount : count,
    headless,
    fingerprintSeed,
  }
}

async function main(): Promise<void> {
  const { count, headless, fingerprintSeed } = parseArgs()
  console.log(JSON.stringify({ event: 'harvest.start', count, headless, fingerprintSeed }))

  const proxies = loadProxies(env.paths.proxiesFile)
  if (proxies.length === 0) {
    console.error(JSON.stringify({ event: 'harvest.failed', reason: 'no_proxies' }))
    process.exit(1)
  }

  const result: HarvestResult = { success: 0, failed: 0, errors: [] }
  const newSessions: Session[] = []

  for (let i = 0; i < count; i++) {
    const proxyData = proxies[i % proxies.length]
    if (!proxyData) continue

    const proxyUrl = proxyData.proxy || `http://${proxyData.ip}:${proxyData.port}`
    const brand = 'Google Chrome'
    const brandVersion = String(Math.floor(Math.random() * 4) + 144)
    const platform = Math.random() > 0.5 ? 'windows' : 'macos'
    console.log(JSON.stringify({ event: 'harvest.attempt', index: i + 1, total: count, proxy: proxyUrl, brand, brandVersion, platform }))

    try {
      const session = await createBrowserSession(proxyUrl, {
        headless,
        humanPreset: env.harvest.humanPreset,
        fingerprintSeed,
        geoip: env.harvest.geoipEnabled,
        secChUaBrand: brand,
        secChUaBrandVersion: brandVersion,
        secChUaPlatform: platform,
      })

      const harvested = await harvestSingleSession(session)

      if (harvested) {
        newSessions.push(harvested)
        result.success++
        console.log(JSON.stringify({ event: 'harvest.ok', index: i + 1 }))
      } else {
        result.failed++
        const msg = `No session data captured`
        result.errors.push(msg)
        console.warn(JSON.stringify({ event: 'harvest.empty', index: i + 1 }))
      }

      await closeBrowserSession(session)
    } catch (error: any) {
      result.failed++
      const msg = error?.message || String(error)
      result.errors.push(msg)
      console.error(JSON.stringify({ event: 'harvest.error', index: i + 1, error: msg }))
    }

    if (i < count - 1) {
      await sleep(env.harvest.delayBetweenMs)
    }
  }

  if (newSessions.length > 0) {
    writeSessions(env.paths.sessionsFile, newSessions)
  }

  console.log(JSON.stringify({
    event: 'harvest.done',
    total: count,
    success: result.success,
    failed: result.failed,
    headless,
    fingerprintSeed,
    sessionFile: env.paths.sessionsFile,
  }))

  if (result.failed > 0 && result.success === 0) {
    process.exit(1)
  }
}

function sleep(ms: number): Promise<void> {
  return new Promise(r => setTimeout(r, ms))
}

main()

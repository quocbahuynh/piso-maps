import axios from 'axios'
import fs from 'fs'
import pLimit from 'p-limit'
import { env } from '../config/env.js'
import { loadProxies, createProxyAgent } from '../lib/proxy.js'
import type { ProxyTestResult } from '../types/index.js'

const MAX_DURATION = 5000
const CONCURRENCY = 20

function removeFailedProxies(failedUrls: Set<string>): void {
  const filePath = env.paths.proxiesFile
  if (!fs.existsSync(filePath)) return

  const raw = fs.readFileSync(filePath, 'utf-8')
  const lines = raw.split('\n')

  const kept = lines.filter(line => {
    const trimmed = line.trim()
    if (trimmed === '') return true
    const url = reconstructUrl(trimmed)
    return url ? !failedUrls.has(url) : !failedUrls.has(trimmed)
  })

  fs.writeFileSync(filePath, kept.join('\n'))
  console.log(`[proxies] Removed ${failedUrls.size} failed proxies`)
}

function reconstructUrl(line: string): string | null {
  const parts = line.trim().split(':')
  if (parts.length === 4) {
    const [ip, port, user, pass] = parts
    return ip && port && user && pass ? `http://${user}:${pass}@${ip}:${port}` : null
  }
  return null
}

async function main(): Promise<void> {
  const proxies = loadProxies(env.paths.proxiesFile)
  if (proxies.length === 0) {
    console.error('No proxies to test')
    process.exit(1)
  }

  console.log(`Testing ${proxies.length} proxies (concurrency: ${CONCURRENCY})...`)
  const limit = pLimit(CONCURRENCY)
  const failedUrls = new Set<string>()
  const results: ProxyTestResult[] = []

  const tasks = proxies.map(proxyData =>
    limit(async () => {
      if (!proxyData) return

      const agent = createProxyAgent(proxyData.proxy)
      const start = Date.now()

      try {
        await axios.get('https://www.google.com/favicon.ico', {
          httpsAgent: agent || undefined,
          timeout: MAX_DURATION,
        })

        const durationMs = Date.now() - start

        if (durationMs > MAX_DURATION) {
          failedUrls.add(proxyData.proxy)
          results.push({ ...proxyData, success: false, durationMs, error: 'timeout' })
          console.log(`  FAIL ${proxyData.ip}:${proxyData.port} — ${durationMs}ms (timeout)`)
        } else {
          results.push({ ...proxyData, success: true, durationMs })
          console.log(`  OK   ${proxyData.ip}:${proxyData.port} — ${durationMs}ms`)
        }
      } catch (error: any) {
        failedUrls.add(proxyData.proxy)
        results.push({ ...proxyData, success: false, durationMs: Date.now() - start, error: error.message })
        console.log(`  FAIL ${proxyData.ip}:${proxyData.port} — ${error.message}`)
      }
    }),
  )

  await Promise.all(tasks)

  const passed = results.filter(r => r.success).length
  const failed = results.filter(r => !r.success).length

  console.log(`\n--- Results ---`)
  console.log(`Total:   ${proxies.length}`)
  console.log(`Passed:  ${passed}`)
  console.log(`Failed:  ${failed}`)
  console.log(`Rate:    ${((passed / proxies.length) * 100).toFixed(1)}%`)

  if (failedUrls.size > 0) {
    removeFailedProxies(failedUrls)
  }

  process.exit(failed > 0 ? 1 : 0)
}

main()

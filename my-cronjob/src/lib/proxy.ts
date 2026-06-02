import fs from 'fs'
import { HttpsProxyAgent } from 'https-proxy-agent'
import type { ProxyData } from '../types/index.js'

export function loadProxies(filePath: string): ProxyData[] {
  if (!fs.existsSync(filePath)) {
    console.warn(`[proxy] File not found: ${filePath}`)
    return []
  }

  const raw = fs.readFileSync(filePath, 'utf-8')
  const lines = raw.split('\n').filter(line => line.trim() !== '')

  const all = lines.map(line => parseProxyLine(line.trim()))
  const unique = new Map<string, ProxyData>()
  for (const p of all) {
    if (p) unique.set(p.proxy, p)
  }

  const proxies = Array.from(unique.values())
  console.log(`[proxy] Loaded ${proxies.length} unique proxies`)
  return proxies
}

function parseProxyLine(line: string): ProxyData | null {
  const parts = line.split(':')

  if (parts.length === 4) {
    const [ip, port, user, pass] = parts
    if (!ip || !port || !user || !pass) return null
    return {
      proxy: `http://${user}:${pass}@${ip}:${port}`,
      protocol: 'http',
      ip,
      port: parseInt(port, 10),
    }
  }

  return {
    proxy: line,
    protocol: line.split(':')[0] || 'http',
    ip: parts[1]?.replace('//', '') || '',
    port: parseInt(parts[2] || '0', 10) || 0,
  }
}

export function createProxyAgent(proxyUrl: string) {
  try {
    return new HttpsProxyAgent(proxyUrl)
  } catch (error) {
    console.error(`[proxy] Failed to create agent for ${proxyUrl}:`, error)
    return null
  }
}

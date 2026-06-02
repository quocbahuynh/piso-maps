import type { Page, HTTPRequest } from 'puppeteer-core'
import type { Session } from '../types/index.js'
import type { BrowserSession } from './browser.js'

const NEWS_URL = 'https://news.google.com/home?hl=vi&gl=VN&ceid=VN:vi'
const MAPS_URL = 'https://www.google.com/maps'
const VN_CITIES = [
  { name: 'Hanoi',            lat: 21.0278, lng: 105.8342 },
  { name: 'Ho Chi Minh City', lat: 10.8231, lng: 106.6297 },
  { name: 'Da Nang',          lat: 16.0544, lng: 108.2022 },
  { name: 'Hai Phong',        lat: 20.8449, lng: 106.6881 },
  { name: 'Can Tho',          lat: 10.0452, lng: 105.7469 },
  { name: 'Nha Trang',        lat: 12.2388, lng: 109.1967 },
  { name: 'Hue',              lat: 16.4637, lng: 107.5909 },
  { name: 'Da Lat',           lat: 11.9465, lng: 108.4419 },
  { name: 'Ha Long',          lat: 20.9513, lng: 107.0800 },
  { name: 'Vung Tau',         lat: 10.3460, lng: 107.0843 },
  { name: 'Pleiku',           lat: 13.9833, lng: 108.0000 },
  { name: 'Buon Ma Thuot',    lat: 12.6667, lng: 108.0500 },
  { name: 'Phan Thiet',       lat: 10.9804, lng: 108.2584 },
  { name: 'Rach Gia',         lat: 10.0167, lng: 105.0833 },
  { name: 'Ca Mau',           lat: 9.1769,  lng: 105.1524 },
]

function randomCity(): { name: string; lat: string; lng: string } {
  const c = VN_CITIES[Math.floor(Math.random() * VN_CITIES.length)]
  return { name: c.name, lat: c.lat.toFixed(4), lng: c.lng.toFixed(4) }
}

function sleep(ms: number): Promise<void> {
  return new Promise(r => setTimeout(r, ms))
}

function randomId(): string {
  return Math.random().toString(36).substring(7)
}

export async function navigateToNews(page: Page, timeoutMs = 45000): Promise<void> {
  await page.goto(NEWS_URL, { waitUntil: 'networkidle2', timeout: timeoutMs })
  await sleep(1000)
}

export async function navigateToMaps(page: Page, timeoutMs = 45000): Promise<void> {
  const { name, lat, lng } = randomCity()
  console.log(`[maps] Navigating to ${name} (${lat}, ${lng})`)
  await page.goto(`${MAPS_URL}/@${lat},${lng},15z`, { waitUntil: 'networkidle2', timeout: timeoutMs })
}

export async function searchOnMaps(page: Page): Promise<void> {
  try {
    await page.waitForSelector('input[name="q"]', { timeout: 10000 })
    await sleep(500)
    await page.click('input[name="q"]')
    await sleep(300)
    await page.type('input[name="q"]', 'Highland coffee', { delay: 60 })
    await sleep(1500)
    await page.click('button[aria-label="Search"]')
    await sleep(5000)
  } catch {
    // silent fallback
  }
}

export function startRequestCapture(page: Page): {
  stop: () => { psi: string | null; secChUa: string | null; secChUaPlatform: string | null }
} {
  let psi: string | null = null
  let secChUa: string | null = null
  let secChUaPlatform: string | null = null

  const handler = (request: HTTPRequest) => {
    const url = request.url()
    const headers = request.headers()

    if (url.includes('psi=')) {
      try {
        const u = new URL(url)
        const p = u.searchParams.get('psi')
        if (p && !psi) {
          psi = p
        }
      } catch {
        // ignore malformed URLs
      }
    }

    if (!secChUa && headers['sec-ch-ua']) {
      secChUa = headers['sec-ch-ua']
      secChUaPlatform = headers['sec-ch-ua-platform'] ?? null
    }
  }

  page.on('request', handler)

  return {
    stop: () => {
      page.off('request', handler)
      return { psi, secChUa, secChUaPlatform }
    },
  }
}

export async function getCapturedPsi(page: Page): Promise<string | null> {
  const psiFromDom = await page.evaluate(() => {
    const m = document.documentElement.innerHTML.match(/"psi":"(.*?)"/)
    return m?.[1] ?? null
  })
  return psiFromDom
}

export async function extractCookies(page: Page): Promise<string> {
  const cookies = await page.cookies()
  return cookies.map(c => `${c.name}=${c.value}`).join('; ')
}

export async function extractUserAgent(page: Page): Promise<string> {
  return page.evaluate(() => navigator.userAgent)
}

export async function harvestSingleSession(
  session: BrowserSession,
): Promise<Session | null> {
  const { browser } = session

  const page = await browser.newPage()
  await page.setViewport({ width: 1280, height: 800 })

  const capture = startRequestCapture(page)

  try {
    await navigateToNews(page)
  } catch (error: any) {
    console.error(`[session] navigateToNews failed: ${error?.message}`)
    await page.close()
    return null
  }

  try {
    await navigateToMaps(page)
  } catch (error: any) {
    console.error(`[session] navigateToMaps failed: ${error?.message}`)
    await page.close()
    return null
  }

  try {
    await searchOnMaps(page)
  } catch (error: any) {
    console.error(`[session] searchOnMaps failed: ${error?.message}`)
  }

  const { psi: capturedPsi, secChUa, secChUaPlatform } = capture.stop()

  let psi = capturedPsi
  if (!psi) {
    try {
      psi = await getCapturedPsi(page)
    } catch (error: any) {
      console.error(`[session] getCapturedPsi failed: ${error?.message}`)
    }
  }

  let cookie = ''
  let userAgent = ''

  try {
    cookie = await extractCookies(page)
  } catch (error: any) {
    console.error(`[session] extractCookies failed: ${error?.message}`)
  }

  try {
    userAgent = await extractUserAgent(page)
  } catch (error: any) {
    console.error(`[session] extractUserAgent failed: ${error?.message}`)
  }

  await page.close()

  if (!psi && !cookie) return null

  return {
    psi: psi || `fallback_${randomId()}`,
    cookie,
    userAgent,
    updatedAt: new Date().toISOString(),
    ...(secChUa && { secChUa }),
    ...(secChUaPlatform && { secChUaPlatform }),
  }
}

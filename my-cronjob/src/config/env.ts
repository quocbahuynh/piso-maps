import dotenv from 'dotenv'
import path from 'path'

dotenv.config()

export const env = {
  harvest: {
    defaultCount: parseInt(process.env.HARVEST_COUNT || '20', 10),
    delayBetweenMs: parseInt(process.env.HARVEST_DELAY_MS || '500', 10),
    headless: process.env.HARVEST_HEADLESS !== 'false',
    humanPreset: (process.env.HARVEST_HUMAN_PRESET === 'careful' ? 'careful' : 'default') as 'default' | 'careful',
    fingerprintSeed: parseInt(process.env.HARVEST_FINGERPRINT_SEED || '', 10) || undefined,
    geoipEnabled: process.env.GEOIP_ENABLED !== 'false',
  },

  paths: {
    sessionsFile: path.resolve(process.cwd(), 'data', 'sessions.json'),
    proxiesFile: path.resolve(process.cwd(), 'data', 'proxies.txt'),
  },
}

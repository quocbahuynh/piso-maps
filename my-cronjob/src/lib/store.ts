import fs from 'fs'
import path from 'path'
import type { Session } from '../types/index.js'

export function writeSessions(filePath: string, sessions: Session[]): void {
  const dir = path.dirname(filePath)
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true })
  fs.writeFileSync(filePath, JSON.stringify(sessions, null, 2))
}

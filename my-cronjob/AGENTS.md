# AGENTS.md — CloakMap Session Harvester

## Commands

| Command | Notes |
|---------|-------|
| `npm run harvest-sessions -- --count 5 --headed` | Main harvester; `--headed` shows browser window |
| `npm run test-proxies` | Benchmarks proxies, removes failed/slow from file |
| `npx tsc --noEmit` | Typecheck (no build step — `tsx` runs `.ts` directly) |

No linter, formatter, or test framework configured.

## TypeScript Quirks

- `module: "nodenext"` + `verbatimModuleSyntax: true` → imports need `.js` extension, type-only imports must use `import type`
- `strict: true` enabled

## CloakBrowser

- Import from `cloakbrowser/puppeteer` (not puppeteer-core directly)
- First run downloads stealth Chromium to `~/.cloakbrowser/` (~200MB)
- `--fingerprint` seed randomizes canvas/WebGL/audio/fonts each launch (not user-agent)
- `geoip: true` requires `mmdb-lib` npm package
- Random per-session: `--fingerprint-brand-version` (144–147), `--fingerprint-platform` (windows/macos)

## TypeScript Quirks

- `module: "nodenext"` + `verbatimModuleSyntax: true` → imports need `.js` extension, type-only imports must use `import type`
- `strict: true` enabled
- No build step — `tsx` runs `.ts` directly, no `dist/` folder

## Data Files

- `data/sessions.json` — harvested sessions (gitignored, overwritten per run)
- `data/proxies.txt` — proxy list (`IP:PORT:USER:PASS` or full URL, one per line)
- `.env` — env config (gitignored)
- `.github/workflows/harvest.yml` — GitHub Actions cronjob (every 6h)

## Harvest Flow

```
CloakBrowser launch (headless, geoip, humanize, random fingerprintSeed, random secChUa)
  → Google News with VN locale (build reputation)
    → Google Maps at random VN city, zoom 15
      → Type "Highland coffee" → click search button → wait 5s
        → Capture PSI from request URL (`psi=` param)
          → Fallback: DOM regex `"psi":"(.*?)"`
            → Capture sec-ch-ua + sec-ch-ua-platform from first request
              → Extract cookies + userAgent
                → writeSessions (overwrites data/sessions.json)
```

PSI from network capture preferred; DOM regex fallback if network fails.

## Session Schema

```
Session { psi, cookie, userAgent, updatedAt, secChUa?, secChUaPlatform? }
```

## Logging Convention

JSON Lines (one JSON object per line) for cron compatibility. Exit 0 if ≥1 sessions, 1 if all fail.

## GitHub Actions

Workflow at `.github/workflows/harvest.yml`. Requires secrets: `PAT`, `PROXIES_B64` (base64 of `data/proxies.txt`), optional `HARVEST_COUNT`.

## What an Agent Likely Misses

- `readSessions` and `appendSessions` were removed (dead code) — `writeSessions` is the only store function
- Duplicate dependencies: `https-proxy-agent`, `p-limit`, `axios` are only used by `test-proxies`, not harvest
- CloakBrowser binary is NOT in `node_modules` — downloaded to `~/.cloakbrowser/` on first `launch()` call
- `store.ts` is now just `writeSessions` — no read/append/dedup logic

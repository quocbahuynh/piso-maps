# PISO Maps Session Harvester

Session harvester backend for **PISO SaaS** — provides Google Maps API identities (PSI tokens, cookies, client hints) at scale via rotating proxies and [CloakBrowser](https://github.com/quocbahuynh/CloakBrowser)'s C++ stealth engine. Designed to run as a **cronjob** for continuous identity pool replenishment.

## How It Works

Each harvested session follows this flow:

```
Pick proxy from pool
  → Launch CloakBrowser (headless, geoip, humanized, random sec-ch-ua)
    → Visit Google News Vietnam (build reputation, VN locale)
      → Visit Google Maps at random Vietnamese city
        → Search "Highland coffee" (provokes internal API calls)
          → Capture PSI token (from network or DOM fallback)
            → Capture sec-ch-ua + sec-ch-ua-platform headers
              → Extract cookies + user agent
                → Overwrite data/sessions.json
```

Sessions **overwrite** the file on each run (no accumulation).

This project is part of **PISO SaaS** — a platform that provides ready-to-use Google Maps API identities for web scraping, allowing consumers to make authenticated Google Maps search, place, and autocomplete requests without managing browsers or proxies.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | Node.js 22, TypeScript 6.0 |
| Runner | `tsx` (no build step) |
| Browser | CloakBrowser (stealth Chromium) + Puppeteer Core |
| Proxy | `https-proxy-agent` |
| Networking | `axios` (proxy benchmark) |
| Concurrency | `p-limit` (proxy benchmark) |
| GeoIP | `mmdb-lib` |
| Env | `dotenv` |
| CI | GitHub Actions (Ubuntu, scheduled every 6h) |

## Prerequisites

- **Node.js** 20+
- **CloakBrowser** — the `cloakbrowser` npm package automatically downloads and manages its stealth Chromium binary. No manual Chrome install needed.

## Setup

```bash
# 1. Install dependencies
npm install

# 2. Configure environment
# Edit .env (see Environment Variables table below)

# 3. Add proxies to data/proxies.txt
# Format (one per line):
#   IP:PORT:USER:PASS           (Webshare format)
#   http://user:pass@host:port  (full URL)
```

### Environment Variables

| Variable | Default | Description |
|---|---|---|
| `HARVEST_COUNT` | `20` | Sessions per harvest run |
| `HARVEST_DELAY_MS` | `500` | Delay between consecutive sessions in ms |
| `HARVEST_HEADLESS` | `true` | Run browser in headless mode |
| `HARVEST_HUMAN_PRESET` | `default` | Human behavior preset (`default` or `careful`) |
| `HARVEST_FINGERPRINT_SEED` | — | Persistent fingerprint seed for returning-visitor identity |
| `GEOIP_ENABLED` | `true` | Auto-detect timezone/locale from proxy IP (requires `mmdb-lib`) |

## Usage

### Test Proxies

Benchmarks every proxy in `data/proxies.txt` against `https://www.google.com/favicon.ico`. Automatically **removes** failed or slow (>5s) proxies from the file.

```bash
npm run test-proxies
```

Output:
```
Testing 25 proxies (concurrency: 20)...
  OK   1.2.3.4:8080 — 342ms
  FAIL 5.6.7.8:3128 — timeout

--- Results ---
Total:   25
Passed:  23
Failed:  2
Rate:    92.0%
```

### Harvest Sessions

Launches CloakBrowser instances through each proxy and captures Google session identities.

```bash
# Harvest 20 sessions (default)
npm run harvest-sessions

# Harvest 100 sessions
npm run harvest-sessions -- --count 100

# Run in headed mode (see the browser windows)
npm run harvest-sessions -- --headed

# Use a persistent fingerprint seed (returning-visitor identity)
npm run harvest-sessions -- --fingerprint 12345
```

Output (JSON Lines — cron-friendly):
```json
{"event":"harvest.start","count":20,"headless":true}
{"event":"harvest.attempt","index":1,"total":20,"proxy":"http://user:pass@1.2.3.4:8080"}
{"event":"harvest.ok","index":1}
{"event":"harvest.error","index":2,"error":"Navigation timeout exceeded"}
...
{"event":"harvest.done","total":20,"success":18,"failed":2,"headless":true,"sessionFile":"/path/to/data/sessions.json"}
```

Exit codes: `0` if at least one session was harvested, `1` if all failed.

## Cronjob (GitHub Actions)

The recommended way is the pre-configured GitHub Actions workflow at `.github/workflows/harvest.yml`. It runs every 6 hours and commits `data/sessions.json` back to the repo.

### Setup

1. Create a **Personal Access Token** with `Contents: write` scope
2. Add it as `PAT` secret in your repo settings
3. Encode `data/proxies.txt` to base64 and add as `PROXIES_B64` secret:
   ```bash
   base64 -i data/proxies.txt | pbcopy
   ```
4. (Optional) Add `HARVEST_COUNT` secret to override default 20

### Self-hosted alternative

```cron
0 */6 * * * cd /path/to/my-backend && npm run harvest-sessions >> /var/log/cloakmap-harvest.log 2>&1


## Output Format

Harvested sessions are stored in `data/sessions.json`:

```json
[
  {
    "psi": "UBwDauX2ErSr4-EP46LNyQw",
    "cookie": "NID=...; AEC=...; ...",
    "userAgent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)...",
    "updatedAt": "2026-06-02T12:00:00.000Z",
    "secChUa": "\"Not:A-Brand\";v=\"99\", \"Google Chrome\";v=\"145\", \"Chromium\";v=\"145\"",
    "secChUaPlatform": "\"macOS\""
  }
```

Each session contains:
- **`psi`** — Google's internal session token (required for Maps API calls)
- **`cookie`** — Full cookie string for authenticated requests
- **`userAgent`** — Browser user agent captured during harvest
- **`updatedAt`** — ISO timestamp of when the session was captured
- **`secChUa`** — Client hint brand header (varies per session: Chrome 144–147)
- **`secChUaPlatform`** — Client hint platform header (varies: Windows or macOS)

## Project Structure

```
src/
├── jobs/                    # Runnable entry points
│   ├── harvest-sessions.ts  # Cronjob: CLI arg parsing, session loop, structured logging
│   └── test-proxies.ts      # Utility: concurrent proxy benchmark with auto-cleanup
├── lib/                     # Shared domain logic (pure functions, no side effects)
│   ├── browser.ts           # launch() / close() wrappers around CloakBrowser
│   ├── proxy.ts             # Parse proxies.txt, create HTTP agent
│   ├── session.ts           # Harvest workflow: capture → news → maps → search → extract
│   └── store.ts             # Read / write / append sessions.json
├── types/
│   └── index.ts             # ProxyData, Session, HarvestResult, ProxyTestResult
└── config/
    └── env.ts               # Environment config via dotenv (no server leftovers)
.github/
└── workflows/
    └── harvest.yml          # GitHub Actions cronjob (every 6h)
data/
├── sessions.json            # Session store (gitignored, auto-created)
└── proxies.txt              # Proxy list (gitignored, one per line)
```


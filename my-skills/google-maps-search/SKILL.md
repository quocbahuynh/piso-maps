---
name: google-maps-search
description: Documents the Google Maps search crawling recipe — HTTP request requirements, response parsing, place entry detection, field extraction, and all verified optional/required parameters for Google's /search endpoint.
---

# Google Maps Search Crawling Recipe

## When to Use

- Understanding or debugging the `/api/maps/search` crawl flow
- Modifying field extraction indices or parsing logic
- Adding/removing fields returned by the search endpoint
- Building or debugging the HTTP request layer of the search crawler
- Troubleshooting blocked requests, rate limiting, or empty responses

## Verified by Live Tests (May 2026)

Tests performed against `https://www.google.com/search?tbm=map` with query `"starbucks"`, Vietnamese locale (`hl=vi`). All variants compared for 20-place Starbucks result.

## HTTP Request Details

**Endpoint:** `GET https://www.google.com/search`

### Required URL Parameters

| Param | Value | Notes |
|---|---|---|
| `tbm` | `map` | Google Maps view |
| `hl` | `vi` | Language (Vietnamese) |
| `authuser` | `0` | Auth user |
| `pb` | `<BuildSearchPb(lat, lng)>` | Protocol buffer encoded params |
| `q` | `<query>` | Search query |
| `oq` | `<query>` | Original query (can duplicate `q`) |

### Optional URL Parameters (Noise)

These can be **safely omitted** without affecting results:

| Param | Was thought to be | Actually | Notes |
|---|---|---|---|
| `gs_l` | Required | **Optional** | Google search context / suggestion tracking |
| `psi` | Required | **Optional** | Page state ID (`<session>.<epoch_ms>.<counter>`) |
| `tch` | Required | **Optional** | Controls response wrapper shape |
| `ech` | Required | **Optional** | Paired with `tch` |
| `rtt` | Optional | **Optional** | Network RTT hint |
| `downlink` | Optional | **Optional** | Bandwidth hint |

### Required Headers

| Header | Notes |
|---|---|
| `user-agent` | Chrome UA (e.g. `Mozilla/5.0 (...Chrome/148... Safari/537.36`) |
| `referer` | `https://www.google.com/` |
| `accept` | `*/*` |

### Optional Headers (Noise)

| Header | Was thought to be | Actually | Notes |
|---|---|---|---|
| `x-browser-validation` | Required | **Optional** | HMAC-SHA1 (20 bytes, base64) |
| `x-browser-channel` | Required | **Optional** | `stable` |
| `x-browser-copyright` | Required | **Optional** | `Copyright 2026 Google LLC.` |
| `x-browser-year` | Required | **Optional** | `2026` |
| `x-maps-diversion-context-bin` | Optional | **Optional** | `CAE=` |
| `available-dictionary` | Required | **Optional** | Brotli dictionary hint — response is plain text anyway |
| `sec-ch-ua` | Optional | **Optional** | Browser brand hint |
| `sec-ch-ua-mobile` | Optional | **Optional** | Mobile flag |
| `sec-ch-ua-platform` | Optional | **Optional** | OS hint |
| `sec-fetch-dest` | Optional | **Optional** | `empty` |
| `sec-fetch-mode` | Optional | **Optional** | `cors` |
| `sec-fetch-site` | Optional | **Optional** | `same-origin` |
| `priority` | Optional | **Optional** | `u=1, i` |

### Cookies

| Cookie | Required | Notes |
|---|---|---|
| `NID` | **Yes** | Required for full format data with pricing, reviews, hours, features |

Additional notes:
- `NID` alone suffices (~764KB response). `__Secure-STRP`, `AEC`, `DV` are not needed.
- Without cookies, the minimal format (~189KB) lacks pricing, has reviews at `entry[37][1]`, limited hours/features — not used.

### Working Request

```bash
curl 'https://www.google.com/search?tbm=map&authuser=0&hl=vi\
  &pb=!4m12...77b1\
  &q=starbucks&oq=starbucks' \
  -H 'user-agent: Mozilla/5.0 (...Chrome/148...)' \
  -H 'referer: https://www.google.com/' \
  -H 'accept: */*' \
  -b 'NID=531=TT-...'
```

Returns 20 places with **pricing, reviews at `entry[4][8]`, 7-day hours, 50+ features.**

### Cookie Renewal

1. **NID cookie**: Set by visiting `https://www.google.com/` once. Renews automatically via `Set-Cookie` headers on initial visit. No authentication required.
2. **__Secure-STRP**: Stronger auth cookie, not needed for search but may help under heavy rate limiting.
3. **AEC + DV**: Anti-abuse cookies. Renew on each page visit.

A simple `GET https://www.google.com/` with the same UA + referer headers suffices to get fresh cookies.

### Error Codes / Rate Limiting

| Symptom | Likely Cause | Fix |
|---|---|---|
| Empty results / captcha | IP rate-limited | Rotate IP or add cookies |
| `p: false` in wrapper | Request flagged | Add `NID` cookie |
| 403 response | IP blocked | Rotate IP, add cookies, valid UA |
| `c: 1` or non-zero | Error or empty results | Check query or PB string |

## Response Shapes

The `tch=1&ech=1` query params control the **outer wrapper format**:

| Shape | `tch` + `ech` | Wrapper | Example |
|---|---|---|---|
| Shape 1 | Present | `{"c":0,"d":"<string>","e":"<base64>","p":true,"u":"<orig_url>"}/*""*/` | Full wrapper with tracking metadata |
| Shape 3 | Absent | `)]}'\n[...]` | Raw array, no wrapper |

**Step 1 — Strip XSSI:**
```csharp
if (raw.StartsWith(")]}'"))
    raw = raw[4..].Trim();
```

**Step 2 — Extract first JSON value:**
```csharp
raw = ExtractFirstJsonValue(raw);  // walks Utf8JsonReader to depth 0
```

**Step 3 — Handle 3 response shapes:**

| Shape | Handling |
|---|---|
| `{ "d": "<string>" }` | Get string, strip XSSI, extract JSON, parse → array → `FindPlaceEntries()` |
| `{ "d": [...] }` | Use array directly → `FindPlaceEntries()` |
| `[...]` | Use root directly → `FindPlaceEntries()` |

The `d` key contains an XSSI-prefixed JSON string: `)]}'\n[<67-element-array>]`.

**To parse either shape (Python):**

```python
import json

text = response.decode("utf-8")

# Handle outer wrapper
if text.lstrip().startswith("{"):
    obj = json.loads(text[:text.rfind("}")+1])
    inner_str = obj["d"]
else:
    inner_str = text

# Strip XSSI prefix
if inner_str.startswith(")]}'"):
    inner_str = inner_str[4:].strip()

# Parse the actual data
data = json.loads(inner_str)  # list with ~67 elements
```

### Wrapper Key Meanings

| Key | Type | Meaning |
|---|---|---|
| `c` | int | Always `0` in observed responses |
| `d` | string | Escaped JSON array (actual data payload) |
| `e` | string | Per-request session/tracking ID (base64, changes each request) |
| `p` | bool | `True` = public access (not rate-limited) |
| `u` | string | The original request URL (echoed back) |

## Full Data Flow

```
HTTP GET /api/maps/search?q=...&lat=...&lng=...
    |
    v
MapsController.Search()
    |
    v
IPlaceService.SearchAsync()            [PISO.Service.Contracts]
    |
    v
PlaceService.SearchAsync()             [PISO.Service] (pass-through)
    |
    v
GoogleMapManager.SearchAsync()         [PISO.GoogleMapService] (line 224)
    |-- ExecuteSearchRequestAsync()     (line 256)
    |-- ParseSearchResponse()           (line 562)
    |-- returns SearchResponseDto
```

## Controllers & Services

| Layer | File | Method | Role |
|---|---|---|---|
| Controller | `MapsController.cs:38` | `Search()` | HTTP entry, returns `SearchResponseDto` |
| Service Interface | `IPlaceService.cs:8` | `SearchAsync()` | Contract |
| Service Impl | `PlaceService.cs:19` | `SearchAsync()` | Delegates to `GoogleMapManager` |
| Infrastructure Interface | `IGoogleMapManager.cs:8` | `SearchAsync()` | Contract |
| Crawler | `GoogleMapManager.cs:224` | `SearchAsync()` | Orchestrates request + parse |

## PB Parameter Construction

Built by `BuildSearchPb()` (line 307). Encodes lat/lng and Google Maps feature flags:

Key sections:
- `!4m12!1m3!1d...!2d{lng}!3d{lat}` — coordinate encoding with zoom level
- `!12m25...` — result-type and feature flags (menu, media, reviews, plus codes, popular times)
- `!20m65...` — UI config flags
- `!24m107...` — extraction flags (same pattern across all endpoints)

## Response Parsing (`ParseSearchResponse`, line 562)

**Step 4 — Deduplicate results:**
```csharp
response.Results = results.Values.ToList();
// results is Dictionary<string, PlaceDetailResponse> keyed by placeId
```

## Place Entry Detection (`FindPlaceEntries`, line 1223)

**Recursive descent algorithm:**

1. First, recursively visit all child arrays (DFS)
2. At each array node, check for **place candidate** — all 3 must match:
   - A string matching `IsPlaceId()`: starts with `0x`, contains `:`, length ≥ 32
   - A `[null, null, lat, lng]` array (coordinate tuple)
   - Array length ≥ 10
3. Deduplicate by place ID (skip if already found)
4. Call `ExtractPlaceDetail(element, placeId)` to populate `PlaceDetailResponse`

## Field Extraction (`ExtractPlaceDetail`, line 1295)

### Direct Index Extraction

| Index | Constant | DTO Path | Type | Notes |
|---|---|---|---|---|
| `entry[11]` | `nameIdx = 11` | `Name` | `string` | HTML-decoded, trimmed |
| `entry[30]` | `tzIdx = 30` | `Location.Timezone` | `string` | |
| `entry[39]` | `addrIdx = 39` | `Location.Address` | `AddressDto` | Parsed by `ParseAddress()` |
| `entry[89]` | `cidIdx = 89` | `Cid` | `string` | Contains `/g/...` Place ID, NOT a `0x...` CID |
| `entry[243]` | `ccIdx = 243` | `Location.CountryCode` | `string` | |
| `entry[7][0]` | — | `Contacts.Website` | `string` | Domain validation via `LooksLikeDomain()` |
| `entry[88]` | — | `Category` | `string` | Strips `SearchResult.TYPE_` prefix, mapped through `CategoryNames` dictionary |
| `entry[203]` | — | `Status` + `WeeklyHours` | — | Calls `FindStatusInArray` |
| `entry[100]` | — | `Features` | `FeatureDto` | Calls `ExtractFeatures` |
| `entry[178]` | — | `Contacts.Phone` + `Pricing` | — | Calls `FindPhoneInArray` + `FindPricingInArray` |
| `entry[4][2]` | — | `Pricing` | `string` | Raw pricing string e.g. `"100-200 N ₫"`. `N` = nghìn (×1000). Parsed via `TryParsePricing()`. |
| `entry[4][7]` | — | `Rating` | `double` | Rounded to 1 decimal. |
| `entry[4][8]` | — | `TotalReviews` | `int` | |

### Fallback Scanning

After direct extraction, two fallback passes run:

**`FindCoordinatesInEntry` (line 1378):** Searches all child arrays for `[null, null, lat, lng]` pattern → sets `Location.Latitude` + `Location.Longitude`

**`ExtractDetailsFromSubArrays` (line 1391):** Scans all child arrays looking for:
- Strings matching `IsWebsite()` → `Contacts.Website`
- Strings matching `IsPhone()` → `Contacts.Phone`
- Strings containing currency symbols → `Pricing.Min` + `Pricing.Max` via `TryParsePricing()`
- Numbers between 1.0 and 5.0 → `Rating` + `TotalReviews` (from adjacent indices)

## Helper Methods

### `IsPlaceId(string s)` (line 1713)
```
s.StartsWith("0x") && s.Contains(':') && s.Length >= 32
```

### `TryGetCoordinates(JsonElement, out lat, out lng)` (line 1273)
Array must match `[null, null, number, number]` pattern.

### `ParseAddress(string full)` (line 1461)
Splits by `,` → assigns `{ country, city, ward, street }` from right to left:
```
"123 Street, Ward, City, Country"
→ street="123 Street", ward="Ward", city="City", country="Country"
```

### `LooksLikeDomain(string s)` (line 1434)
Must contain `.`, no spaces, no uppercase, at least one letter, not starting with `0x`, length ≥ 5.

### `IsWebsite(string s)` (line 1443)
Filters out `googleusercontent.com` and `/url?q=` URLs. Otherwise accepts `http` prefix or domain pattern.

### `IsPhone(string s)` (line 1451)
Length 7–15, contains digits, no letters, no currency symbols, no `:`, `–`, or `−`.

### `TryParsePricing(string s, out min, out max)` (line 1481)
Detects currency symbols, strips them, parses range (`min-max`) or single value. Removes `.` thousand separators. Handles Vietnamese multipliers: `"N"` or `"nghìn"` = ×1000, `"M"` or `"triệu"` = ×1,000,000.

## Status & Hours Extraction

### `FindStatusInArray` (line 1587)
1. Reads `element[0]` (array of day entries) — each entry has:
   - `[0]`: day name string (matched against `VietnameseDays`)
   - `[3][0][0]`: hours string
2. Adds `HourItemDto` per matched day, ordered by `VietnameseDayOrder`
3. Calls `FindOpenStatusRecursive` for open/closed status

### `FindOpenStatusRecursive` (line 1659)
Recursively searches for Vietnamese strings:
- "mở cửa" / "Mở cửa" / "Mở cửa" / "Đóng cửa"
- `IsOpenNow` = starts with "Đang" / "Mở" / "Mở"
- `Status.Text` = matched string

## Features Extraction (`ExtractFeatures`, line 1522)

Recursively finds strings matching `/geo/type/establishment_poi/<feature>` → extracts feature name, maps through `FeatureNames` dictionary, groups into categories (`accessibility`, `parking`, `payments`, `service_options`, `other`).

## Output DTOs

### SearchResponseDto
```json
{
  "local_result": [ PlaceDetailResponse, ... ]
}
```

### PlaceDetailResponse
```json
{
  "place_id":      "string",
  "data_id":       "string",
  "title":         "string",
  "type":          "string",
  "rating":        0,
  "reviews":       0,
  "pricing":       { "min": 0, "max": 0 },
  "contacts":      { "phone": "string", "website": "string" },
  "location":      {
    "address":     { "full": "string", "street": "string", "ward": "string", "city": "string", "country": "string" },
    "latitude":    0,
    "longitude":   0,
    "timezone":    "string",
    "country_code": "string"
  },
  "open_state":    { "is_open_now": true, "text": "string" },
  "opening_hours": [ { "day": "string", "hours": "string" } ],
  "features":      { "accessibility": ["string"], "parking": ["string"], "payments": ["string"] }
}
```

## Gs_l Structure (Documentation Only)

`gs_l` is **optional** but when present, it uses Google's suggestion tracking format:

```
maps.3..38i376k1j38i426k1l4.8597.617149.1.618099.11.11...
```

### Segments

| Segment | Meaning |
|---|---|
| `maps.3` | Service + version |
| `38i376k` / `38i426k` | Query suggestion IDs (numbers reference autocomplete suggestions) |
| `1j` / `1l` | Separator tokens between suggestion IDs |
| `8597.617149.1.618099` | Timing / position counters |
| `38i444k` / `38i377k` / `38i72k` | Second query segment (ow/related query suggestion IDs) |

The `38i<N>k` pattern encodes suggestion IDs from Google's autocomplete service. The numbers (`376`, `426`, `444`, `377`, `72`) map to suggestion entries returned by the `/s` autocomplete endpoint. Full encoding algorithm requires reverse-engineering Google's `BuildSearchPb()` in C# source.

Since `gs_l` is **optional**, decoding it is not needed for a working crawler.

## Psi Structure (Documentation Only)

`psi` is **optional**. When present, format:

```
<base64_session>.<epoch_ms>.<counter>
```

Example: `MVwaatKFF7je2roPveq4mA8.1780112434054.1`

- Base64 session = tied to the session in `pb` param
- Epoch ms = request timestamp
- Counter = request sequence number (starts at 1)

## Notes

- Index constants (`11`, `30`, `39`, `89`, `243`, etc.) are brittle — derived from Google's internal array layout and may break without notice
- `entry[89]` holds the `/g/...` Place ID, NOT the `0x...` CID. The CID is the string detected by `IsPlaceId()` and serves as the dedup key. Same dual-identifier pattern as autocomplete (`info[13]` block).
- The `BuildSearchPb()` string must stay in sync with Google's format
- **Cookies are required** — data comes in the full format (~764KB) with pricing at `entry[4][2]`, reviews at `entry[4][8]`, 7-day hours, 50+ features.
- The `tch=1&ech=1` params control response shape: with them → `{"c":0,"d":"..."}` wrapper; without → raw `)]}'\n[...]` array
- Set the `GOOGLE_COOKIE` env var and run `fetch_search.py` to get data with pricing, detailed reviews, 7-day hours, and 50+ features.
- Vietnamese language strings (`hl=vi`) are baked into status detection — changing locale would break `FindOpenStatusRecursive`
- `FindPlaceEntries` uses **recursive DFS** which can be expensive on large responses
- Response is always **plain text** (no brotli compression even when `available-dictionary` header is sent)
- `pb` string is the **only parameter that requires nontrivial construction**
- All tests performed with Vietnamese locale (`hl=vi`); other locales likely behave identically
- The `gs_l` encoding algorithm remains unknown — not needed since the parameter is optional
- This analysis covers the **search endpoint only**. Autocomplete (`/s`) and place detail (`/maps/preview/place`) have different requirements.

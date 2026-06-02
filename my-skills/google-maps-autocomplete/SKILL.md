---
name: google-maps-autocomplete
description: Documents the Google Maps autocomplete crawling recipe used by PISO's /api/maps/autocomplete endpoint — request building, response parsing, and field extraction from Google's /s endpoint.
---

# Google Maps Autocomplete Crawling Recipe

## When to Use

- Understanding or debugging the `/api/maps/autocomplete` crawl flow
- Modifying field extraction indices or parsing logic
- Adding/removing fields returned by the autocomplete endpoint
- Tracing how raw Google Maps data maps to `MapSuggestionDto`

## Full Data Flow

```
HTTP GET /api/maps/autocomplete?q=...&lat=...&lng=...
    |
    v
MapsController.Autocomplete()
    |
    v
IPlaceService.AutocompleteAsync()        [PISO.Service.Contracts]
    |
    v
PlaceService.AutocompleteAsync()         [PISO.Service] (pass-through)
    |
    v
GoogleMapManager.AutocompleteAsync()     [PISO.GoogleMapService] (line 211)
    |-- ExecuteAutocompleteRequestAsync()  (line 402)
    |-- ParseResponse()                    (line 467)
    |-- returns MapSearchResultDto
```

## Controllers & Services

| Layer | File | Method | Role |
|---|---|---|---|
| Controller | `MapsController.cs:20` | `Autocomplete()` | HTTP entry, returns `MapSearchResultDto` |
| Service Interface | `IPlaceService.cs:7` | `AutocompleteAsync()` | Contract |
| Service Impl | `PlaceService.cs:16` | `AutocompleteAsync()` | Delegates to `GoogleMapManager` |
| Infrastructure Interface | `IGoogleMapManager.cs:7` | `AutocompleteAsync()` | Contract |
| Crawler | `GoogleMapManager.cs:211` | `AutocompleteAsync()` | Orchestrates request + parse |

## HTTP Request Details

**Endpoint:** `GET https://www.google.com/s`

**Query Parameters:**

| Param | Value | Description |
|---|---|---|
| `gs_ri` | `maps` | Google service identifier |
| `suggest` | `p` | Suggest mode |
| `hl` | `vi` | Language (Vietnamese) |
| `q` | `<user query>` | Search query |
| `ech` | `8` | Echo param |
| `pb` | `<BuildPb(lat, lng)>` | Protocol buffer encoded params (see below) |

**Headers:**

| Header | Value |
|---|---|
| `accept` | `*/*` |
| `accept-language` | `vi-VN,vi;q=0.9` |
| `referer` | `https://www.google.com/` |
| `sec-ch-ua` | `"Google Chrome";v="147", ...` |
| `sec-ch-ua-mobile` | `?0` |
| `sec-ch-ua-platform` | `"macOS"` |
| `user-agent` | `Mozilla/5.0 (Macintosh; ...) Chrome/147.0.0.0 Safari/537.36` |
| `Cookie` | `_googleCookie` (set at startup) |

## PB Parameter Construction

Built by `BuildPb()` (line 441). Encodes lat/lng and various Google Maps internal flags:

```
!2i8!4m12!1m3!1d6895.626...!2d{lngStr}!3d{latStr}...
```

Key sections:
- `1m3!1d...!2d{lng}!3d{lat}` — coordinate encoding
- `20m3!5e2!6b1!14b1` — result-type flags
- `!24m107...` — feature flags (menu, media, reviews, plus codes, popular times)
- `!69i779!77b1` — version/padding

## Response Parsing (`ParseResponse`, line 467)

**Step 1 — Strip XSSI prefix:**
```csharp
if (raw.StartsWith(")]}'"))
    raw = raw[4..].Trim();
```

**Step 2 — Navigate the nested JSON array:**

```
root[0]              → main block (array)
root[0][1]           → suggestions array
root[0][1][i]        → i-th suggestion (array)
root[0][1][i][22]    → info block (InfoIndex = 22)
```

**Step 3 — Per-suggestion field extraction:**

| JSONPath in `info` | Constant | DTO Property | Type | Notes |
|---|---|---|---|---|
| `info[0][0]` or `info[1][0]` | — | `Value` | `string` | Title, HTML-decoded. Falls back from index 0 → 1 |
| `info[2][0]` or `info[2]` (raw string) | — | `Subtext` | `string` | Address, HTML-decoded |
| `info[11][2]`, `info[11][3]` | `LatLngIndex = 11` | `Latitude`, `Longitude` | `double` | Coordinates |
| `info[13][0][0]` | `CidIndex = 13` | `DataId` | `string?` | Google CID |
| `info[13][0][10]` | `CidIndex` | `PlaceId` | `string?` | Google Place ID (/g/...), same CID block sub-index 10 |

## Constants (GoogleMapManager.cs lines 13-17)

```csharp
private const string XssiPrefix = ")]'}";
private const int InfoIndex = 22;       // offset to suggestion info block
private const int LatLngIndex = 11;     // offset to [lat, lng] array
private const int CidIndex = 13;        // offset to CID block
```

## Output DTOs

### MapSearchResultDto
```
{
  "suggestions": [ MapSuggestionDto, ... ]
}
```

### MapSuggestionDto
```
{
  "value":      string,   // place name
  "subtext":    string,   // address
  "latitude":   double,
  "longitude":  double,
  "data_id":    string?   // google CID
  "place_id":   string?   // google Place ID (/g/...)
}
```

## Notes

- The PB string must be kept in sync with Google's internal format — it can break without notice
- The Google cookie (`_googleCookie`) is critical for auth; without it the endpoint returns 403/blocked
- Index constants (`InfoIndex`, `LatLngIndex`, `CidIndex`) are brittle — they depend on Google's internal array layout and may change when Google updates their API
- The CID block (`CidIndex = 13`) holds both `data_id` (index 0) and `place_id` (index 10); query suggestions that aren't actual places may have `null` for the entire block

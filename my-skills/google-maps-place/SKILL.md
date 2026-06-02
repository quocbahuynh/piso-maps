---
name: google-maps-place
description: Documents the Google Maps place detail crawling recipe used by PISO's /api/maps/place endpoint — request building, response parsing, place entry detection, and basic+extra field extraction from Google's /maps/preview/place endpoint.
---

# Google Maps Place Detail Crawling Recipe

## When to Use

- Understanding or debugging the `/api/maps/place` crawl flow
- Modifying field extraction indices or parsing logic
- Adding/removing fields returned by the place detail endpoint
- Tracing how raw Google Maps data maps to `PlaceDetailFullDto`

## Full Data Flow

```
HTTP GET /api/maps/place?google_cid=...&lat=...&lng=...
    |
    v
MapsController.PlaceDetail()
    |
    v
IPlaceService.PlaceDetailAsync()            [PISO.Service.Contracts]
    |
    v
PlaceService.PlaceDetailAsync()             [PISO.Service] (pass-through)
    |
    v
GoogleMapManager.PlaceDetailAsync()         [PISO.GoogleMapService] (line 237)
    |-- ExecutePlaceRequestAsync()            (line 358)
    |-- ParsePlaceFullResponse()              (line 623)
    |   |-- FindFirstPlaceEntry()             (line 705)
    |   |-- ExtractPlaceDetail()              (line 1295)
    |   |-- Copy to PlaceDetailFullDto        (lines 675-692)
    |   |-- ExtractExtraPlaceFields()         (line 747)
    |       |-- ScanAllExtractedFields()      (line 852)
    |       |-- ExtractPricingRanges()        (line 795)
    |-- returns PlaceDetailFullDto?
```

## Controllers & Services

| Layer | File | Method | Role |
|---|---|---|---|
| Controller | `MapsController.cs:56` | `PlaceDetail()` | HTTP entry, returns `PlaceDetailResultDto` |
| Service Interface | `IPlaceService.cs:9` | `PlaceDetailAsync()` | Contract |
| Service Impl | `PlaceService.cs:22` | `PlaceDetailAsync()` | Delegates to `GoogleMapManager` |
| Infrastructure Interface | `IGoogleMapManager.cs:9` | `PlaceDetailAsync()` | Contract |
| Crawler | `GoogleMapManager.cs:237` | `PlaceDetailAsync()` | Orchestrates request + parse |

## HTTP Request Details

**Endpoint:** `GET https://www.google.com/maps/preview/place`

**Query Parameters:**

| Param | Value | Description |
|---|---|---|
| `authuser` | `0` | Auth user |
| `hl` | `vi` | Language (Vietnamese) |
| `pb` | `<BuildPlacePb(googleId, lat, lng)>` | Protocol buffer encoded params |

**Headers:** Chrome 148 UA, Google cookie, standard sec-* headers. Same pattern as search/autocomplete.

## PB Parameter Construction

Built by `BuildPlacePb()` (line 334). Encodes googleId, lat/lng, and feature flags:

```
!1m14!1s{googleId}!3m12!1m3!1d15668...!2d{lngStr}!3d{latStr}...
```

Key sections:
- `!1m14!1s{googleId}` — the place CID
- `!3m12!1m3!1d...!2d{lng}!3d{lat}` — coordinates
- `!13m53...` — extraction flags (menu, media, reviews, plus codes, popular times, pricing ranges)
- `!15m108...` — extended feature flags

## Response Parsing (`ParsePlaceFullResponse`, line 623)

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
| `{ "d": "<string>" }` | Get string, strip XSSI, extract JSON, parse → array → payload |
| `{ "d": [...] }` | Use array directly → payload |
| `[...]` | Use root directly → payload |

**Step 4 — Find first place entry:**
```csharp
var (basic, entry) = FindFirstPlaceEntry(dPayload.Value);
// basic = PlaceDetailResponse, entry = raw JsonElement for extra fields
```

## Place Entry Detection (`FindFirstPlaceEntry`, line 705)

**Recursive DFS** — same algorithm as `FindPlaceEntries` in search but returns first match:

1. First recursively visit all child arrays
2. At each array node, check for **place candidate** — all 3 must match:
   - A string matching `IsPlaceId()`: starts with `0x`, contains `:`, length ≥ 32
   - A `[null, null, lat, lng]` array (coordinate tuple)
   - Array length ≥ 10
3. Return `(PlaceDetailResponse?, JsonElement?)` — the extracted basic DTO + raw entry for extra field scanning

## Basic Field Extraction (`ExtractPlaceDetail`, line 1295)

### Direct Index Extraction

| Index | Constant | Property | Type | Notes |
|---|---|---|---|---|
| `entry[11]` | `nameIdx = 11` | `Name` | `string` | Title, HTML-decoded |
| `entry[30]` | `tzIdx = 30` | `Location.Timezone` | `string` | |
| `entry[39]` | `addrIdx = 39` | `Location.Address` | `AddressDto` | Parsed by `ParseAddress()` (split by `,`) |
| `entry[89]` | `cidIdx = 89` | `Cid` | `string` | Google CID |
| `entry[243]` | `ccIdx = 243` | `Location.CountryCode` | `string` | |
| `entry[7][0]` | — | `Contacts.Website` | `string` | Domain validated via `LooksLikeDomain()` |
| `entry[88]` | — | `Category` | `string` | Strips `SearchResult.TYPE_` prefix, mapped via `CategoryNames` |
| `entry[203]` | — | `Status` + `WeeklyHours` | — | `FindStatusInArray` |
| `entry[100]` | — | `Features` | `FeatureDto` | `ExtractFeatures` |
| `entry[178]` | — | `Phone` + `Pricing` | — | `FindPhoneInArray` + `FindPricingInArray` |
| `entry[4][7]` | — | `Rating` | `double` | Rounded to 1 decimal |
| `entry[4][8]` | — | `TotalReviews` | `int` | |

### Fallback Scanning

After direct extraction, two fallback passes run:
- **`FindCoordinatesInEntry`** (line 1378) — searches for `[null, null, lat, lng]` arrays
- **`ExtractDetailsFromSubArrays`** (line 1391) — scans for website, phone, pricing, rating strings/numbers

## Copy to PlaceDetailFullDto (lines 675-692)

```csharp
new PlaceDetailFullDto
{
    Id = basic.Id,                    // place_id
    Cid = basic.Cid,                  // data_id
    Name = basic.Name,                // title
    Category = basic.Category,        // type
    Rating = basic.Rating,
    TotalReviews = basic.TotalReviews, // reviews
    Pricing = basic.Pricing,
    Contacts = basic.Contacts,
    Location = basic.Location,
    Status = basic.Status,            // open_state
    WeeklyHours = basic.WeeklyHours,  // opening_hours
    Features = basic.Features,
    GoogleMapsUrl = $"https://www.google.com/maps/place/?cid={basic.Cid}"
}
```

## Extra Fields Extraction (`ExtractExtraPlaceFields`, line 747)

### Verified Entry Paths (from Python extraction on real data)

| Field | Entry Path | Notes |
|---|---|---|
| Description | `entry[32][1][1]` | String, may be `None` |
| Popular Times | `entry[84][0]` | 7 days × 18 hours (6–23). Each day: `[day_blk, [hour_data, ...]]`. Hour item (len=7): `[hour, occupancy, status, "" , time_label, None, peak_ref]` — `time_label` at index **4**, not 3 |
| Plus Code | `entry[183]` → `entry[183][2][1]` = global code, `entry[183][2][2]` = compound code |
| Reviews | `entry[175][9][0]` | List of review items + `None`/string sentinels |
| Photos | `entry[171][0]` | NOT `entry[171]` directly — `entry[171]` has exactly 1 element wrapping all categories |
| Menu | `entry[171][0]` (same as photos) | Filter by `cat_label` containing "thực đơn" |
| Pricing Ranges | `entry[4][9][0]` | List of 3 tiers |

### Reviews — Detailed Structure

Each review item at `entry[175][9][0][N]` is a list of 8 elements:

```
item[0]    → wrapping list
  [0][0]   → data list (6 elements):
    [0]    → review_id (string, e.g. "Ci9DQUlRQ...")
    [1]    → author info (list, len=18):
      [1][4] → profile block (list, len=6):
        [1][4][5] → author details (list, len=11):
          [0] → author_name (string)
          [1] → author_profile_pic (URL string)
      [1][6] → relative_date (string, e.g. "2 tháng trước")
    [2]    → rating block (list, len=16):
      [2][0] → rating list: [rating_int] (e.g. [5])
      [2][2] → photos list (each item: [photo_id, [..., [6]=url, ...], ...])
    [3]    → reply/owner block (len=12)
    [4]    → extra data (len=7)
    [5]    → token string
item[1]    → extended data:
  [1][0]   → same shape as item[0][0] (6 elements)
    [1][0][2][15][0][0] → review text (string)
```

**Key extraction paths (Python):**
```python
rdata = item[0][0]           # data list
review_id = rdata[0]         # string
author_name = rdata[1][4][5][0]
author_pic = rdata[1][4][5][1]
rating = rdata[2][0][0]      # int
relative_date = rdata[1][6]  # string
text = item[1][0][2][15][0][0]  # deep path in extended data
photos = rdata[2][2]         # list of photo items
```

### Photos — Detailed Structure

`entry[171][0]` = list of 11 category items. Each category:

```
cat[0] → category_id (base64 string, e.g. "CgIgAQ==")
cat[2] → category_label (string, e.g. "Tất cả", "Mới nhất", "Video", "Thực đơn")
cat[3] → media list (each item: [media_id, [..., [6]=image_url, ...], ...])
```

### Menu

Same path as photos (`entry[171][0]`). Iterate categories filtering for `"thực đơn"` in `cat[2]`. Media items under that category are treated as menu dishes.

### Pricing Ranges — Detailed Structure

`entry[4][9][0]` = list of 3 tier items. Each tier (len=5):

```
tier[0] → [range_key, label, full_label]
  [0] → range_key (string, e.g. "E:VND_1_TO_100000")
  [1] → label (string, e.g. "1-100.000 ₫")
  [2] → full_label (string, e.g. "1 ₫ – 100.000 ₫")
tier[1] → [weight, percentage, index]
  [0] → weight (int)
  [1] → percentage (float)
  [2] → index (int)
tier[2] → None
tier[3] → None
tier[4] → token string
```

**Key extraction:**
```python
range_data = tier[0]  # list of 3
weight_data = tier[1] # list of 3
range_key = range_data[0]
label = range_data[1]
weight = weight_data[0]
```

### Bit Flag System (`ScanAllExtractedFields`, line 852)

Recursively scans the raw entry looking for data matching known Google Maps array patterns. Uses bit flags to track what's been found:

| Flag | Constant | Extractor | DTO Field |
|---|---|---|---|
| 1 | `FlagMenu` | `TryExtractMenuDishes` | `Menu` (list of `MenuDishDto`) |
| 2 | `FlagMedia` | `TryExtractMediaCategories` | `Photos` (list of `MediaCategoryDto`) |
| 4 | `FlagReviews` | `TryExtractReviewContainer` | `Reviews` (list of `ReviewDto`) |
| 8 | `FlagPlusCode` | `TryExtractPlusCode` | `GlobalPlusCode` |
| 16 | `FlagPopularTimes` | `TryExtractPopularTimes` | `PopularTimes` (list of `PopularTimeDto`) |

### Description (line 757-761)
```csharp
// entry[32][1][1] = description string
```

## HD Media URLs

Google user content URLs have size suffixes like `=w224-h298-k-no`. Replace with Full HD (1920×1080):

```python
def hd_url(url):
    if isinstance(url, str) and "googleusercontent.com" in url:
        url = re.sub(r"=w\d+-h\d+(-n)?-k-no", "=w1920-h1080-k-no", url)
        url = re.sub(r"=k-no", "=w1920-h1080-k-no", url)
    return url
```

- `=w224-h298-k-no` → `=w1920-h1080-k-no`
- `=k-no` (grass-cs / already no size constraint) → `=w1920-h1080-k-no`
- Profile pics (`=s120-...`): Not matched — left as thumbnails

## Helper Methods

(Shared with search — see `google-maps-search` skill for details)

- `IsPlaceId()` / `TryGetCoordinates()` — place candidate detection
- `ParseAddress()` — address splitting by `,`
- `LooksLikeDomain()` / `IsWebsite()` / `IsPhone()` — contact validation
- `TryParsePricing()` — price range parsing
- `FindStatusInArray` / `FindOpenStatusRecursive` — hours + open status
- `ExtractFeatures` — feature/amenity extraction

## Output DTOs

### PlaceDetailResultDto (wrapper)
```json
{
  "place_result": { PlaceDetailFullDto }
}
```

### PlaceDetailFullDto
```json
{
  "place_id":       "string",
  "data_id":        "string",
  "title":          "string",
  "description":    "string",
  "type":           "string",
  "rating":         0,
  "reviews":        0,
  "pricing":        { "min": 0, "max": 0 },
  "pricing_ranges": [{ "label": "string", "range_key": "string", "weight": 0 }],
  "contacts":       { "phone": "string", "website": "string" },
  "location":       {
    "address":      { "full": "string", "street": "string", "ward": "string", "city": "string", "country": "string" },
    "latitude":     0, "longitude": 0, "timezone": "string", "country_code": "string"
  },
  "open_state":     { "is_open_now": true, "text": "string" },
  "opening_hours":  [{ "day": "string", "hours": "string" }],
  "features":       { "accessibility": ["string"], "parking": ["string"], "payments": ["string"] },
  "popular_times":  [{ "day": "string", "hours": [{ "hour": 0, "occupancy_percent": 0, "status": "string", "time_label": "string" }] }],
  "review_list":    [{ "review_id": "string", "author_name": "string", "author_profile_pic": "string", "rating": 0, "relative_date": "string", "text": "string", "photos": [{ "id": "string", "url": "string", "width": 0, "height": 0 }] }],
  "menu":           [{ "dish_name": "string", "media": [{ "id": "string", "url": "string", "width": 0, "height": 0 }] }],
  "photos":         [{ "category_id": "string", "category_label": "string", "media": [{ "id": "string", "url": "string", "width": 0, "height": 0 }] }],
  "google_maps_url": "string",
  "global_plus_code": "string"
}
```

### Nested DTOs

**ReviewDto:** `review_id`, `author_name`, `author_profile_pic`, `rating`, `relative_date`, `text`, `photos` (list of `ReviewPhotoDto` with `id`, `url`, `width`, `height`)

**PopularTimeDto:** `day`, `hours` (list of `PopularTimeHourDto` with `hour`, `occupancy_percent`, `status`, `time_label`)

**MenuDishDto:** `dish_name`, `media` (list of `MediaItemDto` with `id`, `url`, `width`, `height`)

**MediaCategoryDto:** `category_id`, `category_label`, `media` (list of `MediaItemDto`)

**PricingRangeDto:** `label`, `range_key`, `weight`

## Notes

- Index constants are shared with search (`ExtractPlaceDetail` is the same method)
- `FindFirstPlaceEntry` returns after the first valid match (unlike search which collects all)
- Extra fields depend on the `pb` flags requesting them — if missing from the PB string, Google won't return the data
- The bit flag system (`ScanAllExtractedFields`) efficiently stops scanning once all expected fields are found
- `GoogleMapsUrl` is constructed client-side from the CID, not extracted from Google's response

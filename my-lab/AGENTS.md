# AGENTS.md

## Data

- `data.json` — raw Google Maps API responses. **Not valid JSON as-is**: strip the `)]}'` prefix before parsing.
  - Contains full format data (with cookie) — pricing, 7-day hours, 50+ features
  - Invalid JSON escapes (`\=`, etc.) — `extract_search.py` handles this with `fix_invalid_escapes()`
- Extracted output files are `extracted_*.json`.

## Scripts

| Script | Source | Format | Pricing | Reviews | Features |
|---|---|---|---|---|---|
| `fetch_search.py` | Live HTTP (with cookie) | Full (~764KB) | `entry[4][2]` | `entry[4][8]` | Full |
| `extract_search.py` | `data.json` (file) | Full (cached) | `entry[4][2]` | `entry[4][8]` | Full |

- The original `parse.py` is deleted. Only its compiled bytecache remains at `__pycache__/parse.cpython-314.pyc`.
- Python 3.14 was used.
- To regenerate the extraction scripts, check the `.pyc` with `marshal` + `dis` for the original logic patterns.

## Skills Created/Updated

| Skill | Description | Status |
|---|---|---|
| `google-maps-search` | Search crawling recipe (HTTP reqs + parsing + extraction) | Merged — combines request requirements and field extraction in one skill |
| `google-maps-place` | Place detail crawling recipe | Updated — verified entry paths, HD URLs, reviews/photos/pricing detail |
| `google-maps-autocomplete` | Autocomplete crawling recipe | Stable — no changes needed |

## Key Findings (May 2026 Live Tests)

### Search endpoint (`/search?tbm=map`)

- **Two response formats**: Full (~764KB, with cookies) vs Minimal (~189KB, without cookies)
- **No `gs_l` required** — purely suggestion tracking, can omit
- **No `psi` required** — page state tracking, can omit
- **No `x-browser-*` headers required** — HMAC-SHA1 validation is optional
- **No `available-dictionary` header required** — response is plain text
- **No `sec-*` headers required** — only UA + referer + accept needed
- `tch=1&ech=1` controls response shape only (wrapped vs raw array)
- **Truly required**: `tbm=map`, `hl`, `authuser=0`, `pb`, `q`, `oq`, `user-agent`, `referer`, `accept:*/*`

### Response format detection

```python
is_full_format = entry[4][2] is not None
if is_full_format:
    pricing = entry[4][2]    # "100-200 N ₫"
    reviews = entry[4][8]
else:
    reviews = entry[37][1]   # no pricing available
```

> **Note:** Only the full format (with cookies) is used. Minimal format references are retained here for historical documentation only.

## Notes

- Not a git repository.
- No dependencies file (no `requirements.txt`, `package.json`, etc.).

# my-lab

Google Maps data extraction scripts. Parses raw Google Maps API responses to extract search results, place details, and autocomplete suggestions.

## Project structure

| Script | Input | Output | Extracts |
|---|---|---|---|
| `extract_search.py` | `data.json` | `extracted_search.json` | Places list, pricing, reviews, features, hours |
| `extract_place.py` | `data.json` | `extracted_place.json` | Single place detail + popular times, reviews, menu, photos, pricing ranges |
| `extract_autocomplete.py` | `data.json` | `extracted_autocomplete.json` | Autocomplete suggestions with coords and place IDs |
| `fetch_search.py` | Live HTTP (`GOOGLE_COOKIE`) | `extracted_search.json` | Same fields as `extract_search.py` but from live API |

## Prerequisites

- Python 3.14+
- No external dependencies (stdlib only)

## Setup

```bash
git clone <repo-url>
cd my-lab
```

Place your `data.json` (raw Google Maps API response) in the project root, then run any extractor:

```bash
python extract_search.py
python extract_place.py
python extract_autocomplete.py
```

## Live fetching

`fetch_search.py` hits Google Maps live and requires a valid `GOOGLE_COOKIE` environment variable:

```bash
export GOOGLE_COOKIE="NID=...; ..."
python fetch_search.py --query "starbucks" --lat 10.956 --lng 107.004
```

## Data

- `data.json` is **not committed** to the repository. It must be obtained separately (cached API response).
- Extracted outputs (`extracted_*.json`) are also gitignored — regenerated on each run.
- See `AGENTS.md` for detailed field mapping and API endpoint notes.

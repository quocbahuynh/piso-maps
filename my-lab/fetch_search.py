#!/usr/bin/env python3
"""
Live Google Maps search → extracted_search.json

Uses google-maps-request-search-requirements (minimal HTTP)
+ google-maps-search (response parsing) skills.

Usage:
  python fetch_search.py [--query "starbucks"] [--lat 10.956] [--lng 107.004]

Cookie is read from the GOOGLE_COOKIE environment variable (required).
"""

import json
import re
import html
import os
import urllib.request
import urllib.parse
import sys
import math
import time

# ── Config ───────────────────────────────────────────────────────────────────

USER_AGENT = (
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) "
    "AppleWebKit/537.36 (KHTML, like Gecko) "
    "Chrome/148.0.0.0 Safari/537.36"
)
HL = "vi"
OUTPUT_FILE = "extracted_search.json"

PB_TEMPLATE = (
    "!4m12!1m3!1d{zoom}!2d{lng}!3d{lat}"
    "!2m3!1f0!2f0!3f0!3m2!1i585!2i827!4f13.1!7i20!10b1"
    "!12m25!1m5!18b1!30b1!31m1!1b1!34e1!2m4!5m1!6e2!20e3"
    "!39b1!10b1!12b1!13b1!16b1!17m1!3e1!20m3!5e2!6b1!14b1"
    "!46m1!1b0!96b1!99b1!19m4!2m3!1i360!2i120!4i8"
    "!20m65!2m2!1i203!2i100!3m2!2i4!5b1!6m6!1m2!1i86!2i86!"
    "1m2!1i408!2i240!7m33!1m3!1e1!2b0!3e3!1m3!1e2!2b1!3e2"
    "!1m3!1e2!2b0!3e3!1m3!1e8!2b0!3e3!1m3!1e10!2b0!3e3"
    "!1m3!1e10!2b1!3e2!1m3!1e10!2b0!3e4!1m3!1e9!2b1!3e2"
    "!2b1!9b0"
    "!15m16!1m7!1m2!1m1!1e2!2m2!1i195!2i195!3i20"
    "!1m7!1m2!1m1!1e2!2m2!1i195!2i195!3i20"
    "!22m6!1sMVwaatKFF7je2roPveq4mA8%3A2"
    "!2s1i%3A0%2Ct%3A11886%2Cp%3AMVwaatKFF7je2roPveq4mA8%3A2"
    "!7e81!12e5!17sMVwaatKFF7je2roPveq4mA8%3A86!18e15"
    "!24m107!1m27!13m9!2b1!3b1!4b1!6i1!8b1!9b1!14b1!20b1!25b1"
    "!18m16!3b1!4b1!5b1!6b1!9b1!13b1!14b1!17b1!20b1!21b1!22b1"
    "!32b1!33m1!1b1!34b1!36e2!10m1!8e3!11m1!3e1!17b1"
    "!20m2!1e3!1e6!24b1!25b1!26b1!27b1!29b1!30m1!2b1!36b1!37b1"
    "!39m3!2m2!2i1!3i1!43b1!52b1!55b1!56m1!1b1"
    "!61m2!1m1!1e1!65m5!3m4!1m3!1m2!1i224!2i298"
    "!72m22!1m8!2b1!5b1!7b1!12m4!1b1!2b1!4m1!1e1!4b1"
    "!8m10!1m6!4m1!1e1!4m1!1e3!4m1!1e4"
    "!3sother_user_google_review_posts__and__hotel_and_vr_partner_review_posts"
    "!6m1!1e1!9b1!89b1!90m2!1m1!1e2!98m3!1b1!2b1!3b1!103b1"
    "!113b1!114m3!1b1!2m1!1b1!117b1!122m1!1b1!126b1!127b1!128m1!1b0"
    "!26m4!2m3!1i80!2i92!4i8!30m0"
    "!34m19!2b1!3b1!4b1!6b1!8m6!1b1!3b1!4b1!5b1!6b1!7b1!9b1!12b1"
    "!14b1!20b1!23b1!25b1!26b1!31b1!37m1!1e81!42b1!47m0"
    "!49m10!3b1!6m2!1b1!2b1!7m2!1e3!2b1!8b1!9b1!10e2"
    "!50m4!2e2!3m2!1b1!3b1"
    "!67m5!7b1!10b1!14b1!15m1!1b0!69i780!77b1"
)

# ── Helpers (from google-maps-search skill) ──────────────────────────────────

VIETNAMESE_DAYS = {
    "Thứ Hai": "Monday", "Thứ Ba": "Tuesday", "Thứ Tư": "Wednesday",
    "Thứ Năm": "Thursday", "Thứ Sáu": "Friday", "Thứ Bảy": "Saturday",
    "Chủ Nhật": "Sunday",
}
VIETNAMESE_DAY_ORDER = {v: i for i, v in enumerate(VIETNAMESE_DAYS.values())}


def is_place_id(s):
    return isinstance(s, str) and s.startswith("0x") and ":" in s and len(s) >= 32


def is_phone(s):
    if not isinstance(s, str) or len(s) < 7 or len(s) > 15:
        return False
    if not re.search(r"\d", s):
        return False
    if re.search(r"[a-zA-Z₫$€£¥,:\u2013\u2212]", s):
        return False
    return True


def is_website(s):
    if not isinstance(s, str):
        return False
    if "googleusercontent.com" in s or s.startswith("/url?q="):
        return False
    return s.startswith("http") or s.startswith("www")


def looks_like_domain(s):
    if not isinstance(s, str) or "." not in s or " " in s:
        return False
    if s.lower() != s or s.startswith("0x") or len(s) < 5:
        return False
    return bool(re.search(r"[a-z]", s))


def try_parse_pricing(s):
    if not isinstance(s, str):
        return None, None
    s = s.replace("\xa0", " ").strip()
    multiplier = 1
    if "N" in s or "nghìn" in s.lower():
        multiplier = 1000
    elif "M" in s or "triệu" in s.lower():
        multiplier = 1000000
    cleaned = re.sub(r"[₫$€£¥\u20ab]", "", s)
    cleaned = re.sub(r"[Nn]ghìn|[NM]", "", cleaned).strip()
    cleaned = re.sub(r"\.(?=\d{3})", "", cleaned)
    parts = re.findall(r"[\d,]+", cleaned)
    nums = [int(p.replace(",", "")) for p in parts if p.replace(",", "").isdigit()]
    if not nums:
        return None, None
    if len(nums) >= 2:
        return nums[0] * multiplier, nums[1] * multiplier
    return nums[0] * multiplier, nums[0] * multiplier


def parse_address(full):
    if not isinstance(full, str):
        return {"full": full, "street": None, "ward": None, "city": None, "country": None}
    parts = [p.strip() for p in full.split(",")]
    r = {"full": full, "street": None, "ward": None, "city": None, "country": None}
    if len(parts) >= 1:
        r["country"] = parts[-1]
    if len(parts) >= 2:
        r["city"] = parts[-2]
    if len(parts) >= 3:
        r["ward"] = parts[-3]
    if len(parts) >= 4:
        r["street"] = ", ".join(parts[:-3])
    elif len(parts) == 3:
        r["street"] = parts[0]
    return r


def contains_coords_array(obj):
    if isinstance(obj, list):
        if len(obj) >= 4 and obj[0] is None and obj[1] is None and isinstance(obj[2], (int, float)) and isinstance(obj[3], (int, float)):
            return True
        return any(contains_coords_array(x) for x in obj)
    return False


def find_coords_in(obj):
    if isinstance(obj, list):
        if len(obj) >= 4 and obj[0] is None and obj[1] is None and isinstance(obj[2], (int, float)) and isinstance(obj[3], (int, float)):
            return (float(obj[2]), float(obj[3]))
        for item in obj:
            r = find_coords_in(item)
            if r:
                return r
    return None


def find_place_ids(obj, found=None):
    if found is None:
        found = {}
    if isinstance(obj, list):
        pid = None
        for item in obj:
            if is_place_id(item):
                pid = item
                break
        if pid and len(obj) >= 10 and contains_coords_array(obj):
            if pid not in found:
                found[pid] = obj
            return found
        for item in obj:
            find_place_ids(item, found)
    return found


def find_status_in_array(entry):
    is_open = None
    status_text = None

    def search(obj):
        nonlocal is_open, status_text
        if isinstance(obj, list):
            for item in obj:
                if isinstance(item, str):
                    lower = item.lower()
                    if "mở cửa" in lower or "mở cửa" in lower or "đóng cửa" in lower or "đang mở" in lower:
                        status_text = item
                        is_open = any(lower.startswith(x) for x in ["đang m", "mở", "mở"])
                        return
                search(item)

    search(entry)
    return is_open, status_text


def extract_hours(hours_entry):
    if not isinstance(hours_entry, list) or len(hours_entry) == 0:
        return []
    result = []
    day_entries = hours_entry[0] if isinstance(hours_entry[0], list) else hours_entry
    for de in day_entries:
        if not isinstance(de, list) or len(de) < 4:
            continue
        day_name_vi = de[0] if isinstance(de[0], str) else None
        if not day_name_vi:
            continue
        day_en = VIETNAMESE_DAYS.get(day_name_vi)
        if not day_en:
            continue
        hours_data = de[3]
        hours_str = None
        if isinstance(hours_data, list) and len(hours_data) > 0:
            entry = hours_data[0]
            if isinstance(entry, list) and len(entry) > 0:
                hours_str = entry[0]
            elif isinstance(entry, str):
                hours_str = entry
        if hours_str:
            result.append({"day": day_en, "hours": hours_str})
    result.sort(key=lambda x: VIETNAMESE_DAY_ORDER.get(x["day"], 99))
    return result


def extract_features(feats_entry):
    result = {}

    def search(obj):
        if isinstance(obj, list):
            for item in obj:
                if isinstance(item, str) and "/geo/type/establishment_poi/" in item:
                    feature_key = item.rsplit("/", 1)[-1].replace("_", " ")
                    result[feature_key] = True
                search(item)

    search(feats_entry)
    return result


def categorise_features(features_raw):
    features = {"accessibility": [], "parking": [], "payments": [], "service_options": [], "other": []}
    for k in features_raw:
        cat = "other"
        if "wheelchair" in k or "accessible" in k:
            cat = "accessibility"
        elif "parking" in k or "park" in k:
            cat = "parking"
        elif "payment" in k or "card" in k or "cash" in k or "credit" in k:
            cat = "payments"
        elif "delivery" in k or "takeout" in k or "seating" in k or "dine" in k or "service" in k or "outdoor" in k:
            cat = "service_options"
        features[cat].append(k)
    return {k: v for k, v in features.items() if v}


def extract_place_detail(entry, place_id):
    result = {}
    result["place_id"] = place_id

    data_id = entry[89] if len(entry) > 89 and isinstance(entry[89], str) else None
    result["data_id"] = data_id

    title = entry[11] if len(entry) > 11 and isinstance(entry[11], str) else None
    if title:
        title = html.unescape(title).strip()
    result["title"] = title

    category_raw = entry[88] if len(entry) > 88 else None
    cat = None
    if isinstance(category_raw, list):
        for c in category_raw:
            if isinstance(c, str) and c.startswith("SearchResult.TYPE_"):
                cat = c.replace("SearchResult.TYPE_", "").lower()
                break
            elif isinstance(c, str) and "/" not in c and len(c) < 50:
                cat = c
    result["type"] = cat

    rating = entry[4][7] if len(entry) > 4 and isinstance(entry[4], list) and len(entry[4]) > 7 and isinstance(entry[4][7], (int, float)) else None
    if rating is not None:
        rating = round(float(rating), 1)
    result["rating"] = rating

    pricing_str = entry[4][2] if len(entry) > 4 and isinstance(entry[4], list) and len(entry[4]) > 2 else None
    pmin, pmax = try_parse_pricing(pricing_str)
    result["pricing"] = {"min": pmin or 0, "max": pmax or 0}
    reviews = entry[4][8] if len(entry) > 4 and isinstance(entry[4], list) and len(entry[4]) > 8 and isinstance(entry[4][8], (int, float)) else None
    result["reviews"] = int(reviews) if reviews is not None else None

    contacts = {"phone": None, "website": None}

    if len(entry) > 7 and isinstance(entry[7], list) and len(entry[7]) > 0:
        ws = entry[7][0]
        if is_website(ws) or looks_like_domain(ws):
            contacts["website"] = ws

    if len(entry) > 178 and isinstance(entry[178], list):
        phone_block = entry[178]
        if len(phone_block) > 0 and isinstance(phone_block[0], list) and len(phone_block[0]) > 0:
            ph = phone_block[0][0]
            if is_phone(ph):
                contacts["phone"] = ph

    def scan_fallback(obj):
        if isinstance(obj, list):
            for item in obj:
                if isinstance(item, str):
                    if contacts["phone"] is None and is_phone(item):
                        contacts["phone"] = item
                    if contacts["website"] is None and is_website(item):
                        contacts["website"] = item
                    if contacts["website"] is None and looks_like_domain(item):
                        contacts["website"] = "http://" + item
                scan_fallback(item)

    scan_fallback(entry)
    result["contacts"] = contacts

    addr_full = entry[39] if len(entry) > 39 and isinstance(entry[39], str) else None
    addr = parse_address(addr_full)
    coords = find_coords_in(entry)
    tz = entry[30] if len(entry) > 30 and isinstance(entry[30], str) else None
    cc = entry[243] if len(entry) > 243 and isinstance(entry[243], str) else None
    result["location"] = {
        "address": addr,
        "latitude": float(coords[0]) if coords else None,
        "longitude": float(coords[1]) if coords else None,
        "timezone": tz,
        "country_code": cc,
    }

    is_open, status_text = find_status_in_array(entry)
    result["open_state"] = {"is_open_now": is_open, "text": status_text}

    hours_entry = entry[203] if len(entry) > 203 else None
    result["opening_hours"] = extract_hours(hours_entry)

    feats_entry = entry[100] if len(entry) > 100 else None
    result["features"] = categorise_features(extract_features(feats_entry))

    return result


# ── HTTP Request (from google-maps-request-search-requirements skill) ────────

def build_url(query, lat, lng):
    zoom = 16871.98952572803
    pb = PB_TEMPLATE.format(lat=lat, lng=lng, zoom=zoom)
    params = {
        "tbm": "map",
        "authuser": "0",
        "hl": HL,
        "pb": pb,
        "q": query,
        "oq": query,
        "tch": "1",
        "ech": "1",
    }
    return "https://www.google.com/search?" + urllib.parse.urlencode(params)


def fetch_search(query, lat, lng, cookie):
    url = build_url(query, lat, lng)
    print(f"Requesting: {query} @ {lat}, {lng}", flush=True)

    req = urllib.request.Request(url)
    req.add_header("user-agent", USER_AGENT)
    req.add_header("referer", "https://www.google.com/")
    req.add_header("accept", "*/*")
    req.add_header("cookie", cookie)

    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            body = resp.read().decode("utf-8", errors="replace")
    except Exception as e:
        print(f"  HTTP error: {e}", file=sys.stderr)
        sys.exit(1)

    print(f"  Response size: {len(body):,} bytes", flush=True)
    return body


def parse_response(body):
    text = body.strip()

    # Handle outer wrapper {"c":0,"d":"..."}
    if text.startswith("{"):
        last_brace = text.rfind("}")
        if last_brace == -1:
            print("  ERROR: malformed JSON wrapper", file=sys.stderr)
            sys.exit(1)
        outer = json.loads(text[: last_brace + 1])
        inner_str = outer.get("d", "")
        if not inner_str:
            print("  ERROR: no 'd' key in wrapper", file=sys.stderr)
            print(f"  Keys: {list(outer.keys())}, c={outer.get('c')}, p={outer.get('p')}", flush=True)
            sys.exit(1)
    else:
        inner_str = text

    # Strip XSSI prefix
    inner_str = inner_str.strip()
    while inner_str and inner_str[0] in ")]}'\n\r ":
        inner_str = inner_str[1:]

    # Parse the actual JSON array
    try:
        data = json.JSONDecoder().raw_decode(inner_str)
        return data[0]
    except json.JSONDecodeError as e:
        print(f"  ERROR: JSON decode failed at pos {e.pos}: {e.msg}", file=sys.stderr)
        print(f"  First 200 chars: {repr(inner_str[:200])}", file=sys.stderr)
        sys.exit(1)


# ── Main ──────────────────────────────────────────────────────────────────────

def main():
    query = "starbucks"
    lat = 10.956142832975036
    lng = 107.00383825397303

    i = 1
    while i < len(sys.argv):
        arg = sys.argv[i]
        if arg == "--query" and i + 1 < len(sys.argv):
            query = sys.argv[i + 1]
            i += 1
        elif arg == "--lat" and i + 1 < len(sys.argv):
            lat = float(sys.argv[i + 1])
            i += 1
        elif arg == "--lng" and i + 1 < len(sys.argv):
            lng = float(sys.argv[i + 1])
            i += 1
        elif arg == "--help":
            print("Usage: python fetch_search.py [--query Q] [--lat N] [--lng N]")
            print("Cookie source: GOOGLE_COOKIE environment variable")
            return
        i += 1

    cookie = os.environ.get("GOOGLE_COOKIE")
    if not cookie:
        print("ERROR: GOOGLE_COOKIE environment variable is required", file=sys.stderr)
        sys.exit(1)

    body = fetch_search(query, lat, lng, cookie)
    data = parse_response(body)
    places_map = find_place_ids(data)
    print(f"  Found {len(places_map)} place entries", flush=True)

    results = []
    for pid, entry in places_map.items():
        detail = extract_place_detail(entry, pid)
        results.append(detail)

    output = {"local_result": results}
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        json.dump(output, f, ensure_ascii=False, indent=2)

    print(f"  Written {len(results)} places → {OUTPUT_FILE}", flush=True)


if __name__ == "__main__":
    main()

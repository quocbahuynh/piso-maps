import json
import re
import html

VIETNAMESE_DAYS = {
    "Thứ Hai": "Monday", "Thứ Ba": "Tuesday", "Thứ Tư": "Wednesday",
    "Thứ Năm": "Thursday", "Thứ Sáu": "Friday", "Thứ Bảy": "Saturday",
    "Chủ Nhật": "Sunday",
}
VIETNAMESE_DAY_ORDER = {v: i for i, v in enumerate(VIETNAMESE_DAYS.values())}

def is_place_id(s):
    return isinstance(s, str) and s.startswith("0x") and ":" in s and len(s) >= 32

def is_phone(s):
    if not isinstance(s, str):
        return False
    if len(s) < 7 or len(s) > 15:
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
    if s.startswith("http") or s.startswith("www"):
        return True
    return False

def looks_like_domain(s):
    if not isinstance(s, str):
        return False
    if "." not in s:
        return False
    if " " in s or s.lower() != s:
        return False
    if s.startswith("0x"):
        return False
    if len(s) < 5:
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
    result = {"full": full, "street": None, "ward": None, "city": None, "country": None}
    if len(parts) >= 1:
        result["country"] = parts[-1]
    if len(parts) >= 2:
        result["city"] = parts[-2]
    if len(parts) >= 3:
        result["ward"] = parts[-3]
    if len(parts) >= 4:
        result["street"] = ", ".join(parts[:-3])
    elif len(parts) == 3:
        result["street"] = parts[0]
    return result

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

    # Fallback scanning for phone/website
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
    features_raw = extract_features(feats_entry)
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
    result["features"] = {k: v for k, v in features.items() if v}

    return result


with open("data.json", "rb") as f:
    raw = f.read()

text = raw.decode("utf-8", errors="replace")

# Find "d":" and extract the string value manually (handle invalid escapes)
import re
m = re.search(r'"d"\s*:\s*"', text)
if not m:
    print("ERROR: cannot find 'd' key in data.json")
    print(f"First 200 chars: {text[:200]}")
    exit(1)
idx = m.end()
if idx == -1:
    print("ERROR: cannot find 'd' key in data.json")
    print(f"First 200 chars: {text[:200]}")
    exit(1)

d_start = idx
# Find the closing " of the d value (tracking backslash escapes)
d_end = d_start
while d_end < len(text):
    if text[d_end] == '\\':
        d_end += 2
    elif text[d_end] == '"':
        break
    else:
        d_end += 1

d_val_raw = text[d_start:d_end]
# Sanitize invalid JSON escapes (e.g. \= is not valid JSON)
def fix_invalid_escapes(s):
    result = []
    i = 0
    while i < len(s):
        if s[i] == '\\' and i + 1 < len(s):
            n = s[i + 1]
            valid = n in '"\\/bfnrtu'
            if valid:
                result.append(s[i:i+2])
                i += 2
            else:
                result.append(n)
                i += 2
        else:
            result.append(s[i])
            i += 1
    return ''.join(result)

d_val_raw = fix_invalid_escapes(d_val_raw)
# Unescape JSON string (it's still escaped inside the outer JSON)
d_val = json.loads('"' + d_val_raw + '"')
while d_val and d_val[0] in ")]}'\n\r ":
    d_val = d_val[1:]

data = json.loads(d_val)

places_map = find_place_ids(data)
print(f"Found {len(places_map)} place entries")

results = []
for pid, entry in places_map.items():
    detail = extract_place_detail(entry, pid)
    results.append(detail)

output = {"local_result": results}

with open("extracted_search.json", "w", encoding="utf-8") as f:
    json.dump(output, f, ensure_ascii=False, indent=2)

print(f"Extracted {len(results)} places → extracted_search.json")

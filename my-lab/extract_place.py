import json
import re
import html
from urllib.parse import quote

VIETNAMESE_DAYS = {
    "Thứ Hai": "Monday", "Thứ Ba": "Tuesday", "Thứ Tư": "Wednesday",
    "Thứ Năm": "Thursday", "Thứ Sáu": "Friday", "Thứ Bảy": "Saturday",
    "Chủ Nhật": "Sunday",
}
VIETNAMESE_DAY_ORDER = {v: i for i, v in enumerate(VIETNAMESE_DAYS.values())}
VIET_DAY_NAMES = list(VIETNAMESE_DAYS.keys())

# --- Helpers (shared with search) ---

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

def hd_url(url):
    if isinstance(url, str) and "googleusercontent.com" in url:
        url = re.sub(r"=w\d+-h\d+(-n)?-k-no", "=w1920-h1080-k-no", url)
        url = re.sub(r"=k-no", "=w1920-h1080-k-no", url)
    return url

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
                    lower = item.lower().replace("mở", "mở")
                    if "mở cửa" in lower or "đóng cửa" in lower or "đang mở" in lower:
                        status_text = item
                        is_open = any(lower.startswith(x) for x in ["đang m", "mở", "mở"])
                        return
                search(item)
    search(entry)
    return is_open, status_text

def extract_hours(entry):
    hr = entry[203] if len(entry) > 203 else None
    if not isinstance(hr, list) or len(hr) == 0:
        return []
    result = []
    day_entries = hr[0] if isinstance(hr[0], list) else hr
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
            h_entry = hours_data[0]
            if isinstance(h_entry, list) and len(h_entry) > 0:
                hours_str = h_entry[0]
            elif isinstance(h_entry, str):
                hours_str = h_entry
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
    features = {"accessibility": [], "parking": [], "payments": [], "service_options": [], "other": []}
    for k in result:
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

# --- Extra place field extractors ---

def extract_popular_times(entry):
    pt = entry[84] if len(entry) > 84 else None
    if not isinstance(pt, list) or len(pt) < 1:
        return []
    days_data = pt[0] if isinstance(pt[0], list) else None
    if not days_data:
        return []
    result = []
    for di, day_block in enumerate(days_data):
        if not isinstance(day_block, list) or len(day_block) < 2:
            continue
        day_name = VIET_DAY_NAMES[di] if di < len(VIET_DAY_NAMES) else f"Day_{di}"
        day_en = VIETNAMESE_DAYS.get(day_name, day_name)
        hours_data = day_block[1] if isinstance(day_block[1], list) else day_block
        hours_list = []
        for h_item in hours_data:
            if not isinstance(h_item, list) or len(h_item) < 2:
                continue
            hour = h_item[0]
            occ = h_item[1]
            status = h_item[2] if len(h_item) > 2 and isinstance(h_item[2], str) else ""
            time_label = h_item[4] if len(h_item) > 4 and isinstance(h_item[4], str) else f"{hour} giờ"
            if isinstance(hour, (int, float)):
                hours_list.append({
                    "hour": int(hour),
                    "occupancy_percent": int(occ) if isinstance(occ, (int, float)) else 0,
                    "status": status,
                    "time_label": time_label
                })
        if hours_list:
            result.append({"day": day_en, "hours": hours_list})
    return result

def extract_plus_code(entry):
    pc = entry[183] if len(entry) > 183 else None
    if not isinstance(pc, list) or len(pc) < 3:
        return None, None
    inner = pc[2] if isinstance(pc[2], list) else None
    if not inner or len(inner) < 3:
        return None, None
    global_code = inner[1] if isinstance(inner[1], str) else (inner[1][0] if isinstance(inner[1], list) and len(inner[1]) > 0 else None)
    compound_code = inner[2] if isinstance(inner[2], str) else None
    return global_code, compound_code

def extract_photo_categories(entry):
    e171 = entry[171] if len(entry) > 171 else None
    if not isinstance(e171, list) or len(e171) == 0:
        return []
    cats = e171[0] if isinstance(e171[0], list) else e171
    result = []
    for cat_item in cats:
        if not isinstance(cat_item, list) or len(cat_item) < 4:
            continue
        cat_id = cat_item[0] if isinstance(cat_item[0], str) else (cat_item[0] if isinstance(cat_item[0], list) and len(cat_item[0]) > 0 and isinstance(cat_item[0][0], str) else None)
        cat_label = cat_item[2] if isinstance(cat_item[2], str) else None
        if not cat_label:
            continue
        media_list = cat_item[3] if isinstance(cat_item[3], list) else []
        media = []
        for m in media_list:
            if not isinstance(m, list):
                continue
            mid = m[0] if len(m) > 0 else None
            url = hd_url(m[6][0]) if len(m) > 6 and isinstance(m[6], list) and len(m[6]) > 0 else None
            # Try dimensions from m[7]
            mw = None
            mh = None
            if len(m) > 7 and isinstance(m[7], list) and len(m[7]) >= 2:
                mw = m[7][0]
                mh = m[7][1]
            elif len(m) > 5 and isinstance(m[5], list) and len(m[5]) >= 2:
                dims = m[5]
                if isinstance(dims[0], (int, float)) and isinstance(dims[1], (int, float)):
                    mw, mh = dims[0], dims[1]
            if mid is not None:
                media.append({"id": mid, "url": url, "width": mw, "height": mh})
        result.append({"category_id": cat_id, "category_label": cat_label, "media": media})
    return result

def extract_reviews(entry):
    rv = entry[175] if len(entry) > 175 else None
    if not isinstance(rv, list) or len(rv) < 10:
        return []
    container = rv[9] if isinstance(rv[9], list) else None
    if not container or len(container) < 1:
        return []
    review_list = container[0] if isinstance(container[0], list) else []
    result = []
    for r_item in review_list:
        if not isinstance(r_item, list) or len(r_item) < 1:
            continue
        rdata = r_item[0][0] if isinstance(r_item[0], list) and len(r_item[0]) > 0 and isinstance(r_item[0][0], list) else None
        if not rdata:
            continue
        review_id = rdata[0] if len(rdata) > 0 and isinstance(rdata[0], str) else None
        if not review_id:
            continue

        author_name = None
        author_pic = None
        author_info = rdata[1] if len(rdata) > 1 and isinstance(rdata[1], list) else None
        if author_info and len(author_info) > 4 and isinstance(author_info[4], list):
            author_block = author_info[4]
            if len(author_block) > 5 and isinstance(author_block[5], list):
                author_name = author_block[5][0] if isinstance(author_block[5][0], str) else None
                if len(author_block[5]) > 1 and isinstance(author_block[5][1], str) and "googleusercontent" in author_block[5][1]:
                    author_pic = author_block[5][1]

        rating = None
        rating_block = rdata[2] if len(rdata) > 2 and isinstance(rdata[2], list) else None
        if rating_block and len(rating_block) > 0 and isinstance(rating_block[0], list) and len(rating_block[0]) > 0 and isinstance(rating_block[0][0], (int, float)):
            rating = int(rating_block[0][0])

        relative_date = author_info[6] if author_info and len(author_info) > 6 and isinstance(author_info[6], str) else None

        text = None
        if len(r_item) > 1 and isinstance(r_item[1], list) and len(r_item[1]) > 0 and isinstance(r_item[1][0], list):
            rdata2 = r_item[1][0]
            if len(rdata2) > 2 and isinstance(rdata2[2], list) and len(rdata2[2]) > 15:
                text_block = rdata2[2][15]
                if isinstance(text_block, list) and len(text_block) > 0:
                    text_inner = text_block[0] if isinstance(text_block[0], list) else text_block
                    if isinstance(text_inner, list) and len(text_inner) > 0 and isinstance(text_inner[0], str) and len(text_inner[0]) > 10:
                        text = text_inner[0]

        photos = []
        if rating_block and len(rating_block) > 2 and isinstance(rating_block[2], list):
            photo_data = rating_block[2]
            for p_item in photo_data:
                if not isinstance(p_item, list) or len(p_item) < 2:
                    continue
                pid_val = p_item[0] if isinstance(p_item[0], str) else None
                purl = None
                if isinstance(p_item[1], list) and len(p_item[1]) > 6:
                    url_block = p_item[1][6]
                    if isinstance(url_block, list) and len(url_block) > 0 and isinstance(url_block[0], str):
                        purl = hd_url(url_block[0])
                if pid_val:
                    photos.append({"id": pid_val, "url": purl, "width": None, "height": None})

        result.append({
            "review_id": review_id,
            "author_name": author_name,
            "author_profile_pic": author_pic,
            "rating": rating,
            "relative_date": relative_date,
            "text": text,
            "photos": photos
        })
    return result

def extract_menu(entry):
    e171 = entry[171] if len(entry) > 171 else None
    if not isinstance(e171, list) or len(e171) == 0:
        return []
    cats = e171[0] if isinstance(e171[0], list) else e171
    for cat_item in cats:
        if not isinstance(cat_item, list) or len(cat_item) < 4:
            continue
        cat_label = cat_item[2] if isinstance(cat_item[2], str) else None
        if cat_label and "thực đơn" in cat_label.lower():
            items = cat_item[3] if isinstance(cat_item[3], list) else []
            menu = []
            for dish in items:
                if not isinstance(dish, list) or len(dish) < 1:
                    continue
                dish_name = dish[0] if isinstance(dish[0], str) else None
                if not dish_name:
                    continue
                # Media for the dish
                media = []
                # Check various indices for media
                for mi in [5, 6, 8]:
                    if len(dish) > mi and isinstance(dish[mi], list):
                        for m_item in dish[mi]:
                            if isinstance(m_item, list) and len(m_item) > 0:
                                mid = m_item[0] if isinstance(m_item[0], str) else None
                                murl = None
                                for ui in [3, 5, 6]:
                                    if len(m_item) > ui and isinstance(m_item[ui], list) and len(m_item[ui]) > 0:
                                        murl = hd_url(m_item[ui][0])
                                        break
                                if mid:
                                    media.append({"id": mid, "url": murl, "width": None, "height": None})
                menu.append({"dish_name": dish_name, "media": media})
            return menu
    return []

def extract_pricing_ranges(entry):
    if len(entry) < 5 or not isinstance(entry[4], list) or len(entry[4]) < 10:
        return []
    pr_block = entry[4][9]
    if not isinstance(pr_block, list) or len(pr_block) < 1:
        return []
    tiers = pr_block[0] if isinstance(pr_block[0], list) else pr_block
    result = []
    for tier in tiers:
        if not isinstance(tier, list) or len(tier) < 2:
            continue
        range_data = tier[0] if isinstance(tier[0], list) else None
        weight_data = tier[1] if isinstance(tier[1], list) else None
        if not range_data:
            continue
        range_key = range_data[0] if len(range_data) > 0 and isinstance(range_data[0], str) else None
        label = range_data[1] if len(range_data) > 1 and isinstance(range_data[1], str) else None
        weight = weight_data[0] if weight_data and len(weight_data) > 0 and isinstance(weight_data[0], (int, float)) else None
        if range_key:
            result.append({"label": label, "range_key": range_key, "weight": weight})
    return result

def extract_description(entry):
    if len(entry) > 32 and isinstance(entry[32], list) and len(entry[32]) > 1:
        desc_block = entry[32][1]
        if isinstance(desc_block, list) and len(desc_block) > 1:
            desc = desc_block[1]
            if isinstance(desc, str) and len(desc) > 5:
                return desc
    return None

# --- Main ---

with open("data.json", "r", encoding="utf-8") as f:
    content = f.read()

start = content.index("[")
data = json.loads(content[start:])

places = find_place_ids(data)
if not places:
    print("No place entry found")
    exit(1)

pid = list(places.keys())[0]
entry = places[pid]

# Basic fields (matching search extraction)
data_id = entry[89] if len(entry) > 89 and isinstance(entry[89], str) else None
title = entry[11] if len(entry) > 11 and isinstance(entry[11], str) else None
if title:
    title = html.unescape(title).strip()

category_raw = entry[88] if len(entry) > 88 else None
cat = None
if isinstance(category_raw, list):
    for c in category_raw:
        if isinstance(c, str) and c.startswith("SearchResult.TYPE_"):
            cat = c.replace("SearchResult.TYPE_", "").lower()
            break
        elif isinstance(c, str) and "/" not in c and len(c) < 50:
            cat = c

pricing_str = entry[4][2] if len(entry) > 4 and isinstance(entry[4], list) and len(entry[4]) > 2 else None
pmin, pmax = try_parse_pricing(pricing_str)

rating = None
reviews = None
if len(entry) > 4 and isinstance(entry[4], list):
    rating = entry[4][7] if len(entry[4]) > 7 and isinstance(entry[4][7], (int, float)) else None
    reviews = entry[4][8] if len(entry[4]) > 8 and isinstance(entry[4][8], (int, float)) else None
    if rating is not None:
        rating = round(float(rating), 1)
    if reviews is not None:
        reviews = int(reviews)

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

addr_full = entry[39] if len(entry) > 39 and isinstance(entry[39], str) else None
addr = parse_address(addr_full)
coords = find_coords_in(entry)
tz = entry[30] if len(entry) > 30 and isinstance(entry[30], str) else None
cc = entry[243] if len(entry) > 243 and isinstance(entry[243], str) else None

is_open, status_text = find_status_in_array(entry)

# Extra fields
popular_times = extract_popular_times(entry)
global_plus_code, compound_plus_code = extract_plus_code(entry)
photo_categories = extract_photo_categories(entry)
review_list = extract_reviews(entry)
menu_list = extract_menu(entry)
pricing_ranges = extract_pricing_ranges(entry)
description = extract_description(entry)

cid_for_url = pid.split(":")[1] if ":" in pid else pid
google_maps_url = f"https://www.google.com/maps/place/?cid={cid_for_url}"

place_result = {
    "place_id": pid,
    "data_id": data_id,
    "title": title,
    "description": description,
    "type": cat,
    "rating": rating,
    "reviews": reviews,
    "pricing": {"min": pmin, "max": pmax},
    "pricing_ranges": pricing_ranges,
    "contacts": contacts,
    "location": {
        "address": addr,
        "latitude": float(coords[0]) if coords else None,
        "longitude": float(coords[1]) if coords else None,
        "timezone": tz,
        "country_code": cc,
    },
    "open_state": {"is_open_now": is_open, "text": status_text},
    "opening_hours": extract_hours(entry),
    "features": extract_features(entry[100] if len(entry) > 100 else None),
    "popular_times": popular_times,
    "review_list": review_list,
    "menu": menu_list,
    "photo_categories": photo_categories,
    "global_plus_code": global_plus_code,
    "google_maps_url": google_maps_url,
}

output = {"place_result": place_result}

with open("extracted_place.json", "w", encoding="utf-8") as f:
    json.dump(output, f, ensure_ascii=False, indent=2)

print(f"Extracted place: {title}")
print(f"  reviews: {len(review_list)}, photos: {len(photo_categories)} categories, popular times: {len(popular_times)} days, menu: {len(menu_list)} items")
print(f"  plus code: {global_plus_code}")
print(f"  pricing ranges: {len(pricing_ranges)} tiers")
print(f"  description: {'yes' if description else 'no'}")
print(f"→ extracted_place.json")

import json

# Constants (from google-maps-autocomplete skill)
SUGGESTIONS_INDEX = 1     # root[0][1] = suggestions array
INFO_INDEX = 22           # suggestion[22] = info block
VALUE_INDEX = 0           # info[0][0] or info[1][0]
SUBTEXT_INDEX = 2         # info[2][0] or info[2]
LATLNG_INDEX = 11         # info[11][2], info[11][3]
CID_INDEX = 13            # info[13][0][0]

with open("data.json", "r", encoding="utf-8") as f:
    content = f.read()

start = content.index("[")
data = json.loads(content[start:])

suggestions_raw = data[0][SUGGESTIONS_INDEX]
suggestions = []

for s in suggestions_raw:
    info = s[INFO_INDEX] if len(s) > INFO_INDEX else None
    if not isinstance(info, list):
        suggestions.append({
            "value": None,
            "subtext": None,
            "latitude": None,
            "longitude": None,
            "data_id": None,
            "place_id": None
        })
        continue

    value = None
    if len(info) > 0 and isinstance(info[0], list) and len(info[0]) > 0:
        value = info[0][0]
    if value is None and len(info) > 1 and isinstance(info[1], list) and len(info[1]) > 0:
        value = info[1][0]

    subtext = None
    if len(info) > 2:
        if isinstance(info[2], list) and len(info[2]) > 0:
            subtext = info[2][0]
        elif isinstance(info[2], str):
            subtext = info[2]

    lat = None
    lng = None
    if len(info) > LATLNG_INDEX and isinstance(info[LATLNG_INDEX], list) and len(info[LATLNG_INDEX]) > 3:
        lng = info[LATLNG_INDEX][2]
        lat = info[LATLNG_INDEX][3]

    data_id = None
    place_id = None
    if len(info) > CID_INDEX and isinstance(info[CID_INDEX], list) and len(info[CID_INDEX]) > 0:
        cid_block = info[CID_INDEX]
        if isinstance(cid_block[0], list):
            if len(cid_block[0]) > 0:
                data_id = cid_block[0][0]
            if len(cid_block[0]) > 10 and isinstance(cid_block[0][10], str):
                place_id = cid_block[0][10]

    suggestions.append({
        "value": value,
        "subtext": subtext,
        "latitude": lat,
        "longitude": lng,
        "data_id": data_id,
        "place_id": place_id
    })

output = {"suggestions": suggestions}

with open("extracted_autocomplete.json", "w", encoding="utf-8") as f:
    json.dump(output, f, ensure_ascii=False, indent=2)

print(f"Extracted {len(suggestions)} autocomplete suggestions → extracted_autocomplete.json")

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using PISO.Contracts;
using PISO.Shared.DataTransferObjects.Maps;
using static PISO.GoogleMapService.GoogleMapsHelpers;

namespace PISO.GoogleMapService;

public static class GoogleMapsParser
{
    public static MapSearchResultDto ParseResponse(string raw, ILoggerManager logger)
    {
        var result = new MapSearchResultDto();

        if (raw.StartsWith(XssiPrefix))
            raw = raw[4..].Trim();

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                return result;

            var mainBlock = root[0];
            if (mainBlock.ValueKind != JsonValueKind.Array || mainBlock.GetArrayLength() < 2)
                return result;

            var suggestionsArray = mainBlock[1];
            if (suggestionsArray.ValueKind != JsonValueKind.Array)
                return result;

            foreach (var item in suggestionsArray.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Array || item.GetArrayLength() <= InfoIndex)
                    continue;

                var info = item[InfoIndex];
                if (info.ValueKind != JsonValueKind.Array || info.GetArrayLength() < 3)
                    continue;

                var suggestion = new MapSuggestionDto();

                if (info[0].ValueKind == JsonValueKind.Array && info[0].GetArrayLength() > 0)
                    suggestion.Value = WebUtility.HtmlDecode(info[0][0].GetString() ?? string.Empty);
                else if (info[1].ValueKind == JsonValueKind.Array && info[1].GetArrayLength() > 0)
                    suggestion.Value = WebUtility.HtmlDecode(info[1][0].GetString() ?? string.Empty);

                if (info[2].ValueKind == JsonValueKind.Array && info[2].GetArrayLength() > 0)
                    suggestion.Subtext = WebUtility.HtmlDecode(info[2][0].GetString() ?? string.Empty);
                else if (info[2].ValueKind == JsonValueKind.String)
                    suggestion.Subtext = WebUtility.HtmlDecode(info[2].GetString() ?? string.Empty);

                if (info.GetArrayLength() > LatLngIndex && info[LatLngIndex].ValueKind == JsonValueKind.Array && info[LatLngIndex].GetArrayLength() > 3)
                {
                    suggestion.Latitude = info[LatLngIndex][2].GetDouble();
                    suggestion.Longitude = info[LatLngIndex][3].GetDouble();
                }

                if (info.GetArrayLength() > CidIndex && info[CidIndex].ValueKind == JsonValueKind.Array && info[CidIndex].GetArrayLength() > 0)
                {
                    var cidBlock = info[CidIndex][0];
                    if (cidBlock.ValueKind == JsonValueKind.Array)
                    {
                        if (cidBlock.GetArrayLength() > 0)
                            suggestion.DataId = cidBlock[0].GetString();
                        if (cidBlock.GetArrayLength() > 10 && cidBlock[10].ValueKind == JsonValueKind.String)
                            suggestion.PlaceId = cidBlock[10].GetString();
                    }
                }

                result.Suggestions.Add(suggestion);
            }
        }
        catch (JsonException ex)
        {
            logger.LogError($"Failed to parse Google response: {ex.Message}");
        }

        return result;
    }

    public static string ExtractFirstJsonValue(string raw)
    {
        var bytes = Encoding.UTF8.GetBytes(raw);
        var reader = new Utf8JsonReader(bytes);

        if (!reader.Read())
            return raw;

        int depth = 1;
        while (depth > 0 && reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                case JsonTokenType.StartArray:
                    depth++;
                    break;
                case JsonTokenType.EndObject:
                case JsonTokenType.EndArray:
                    depth--;
                    break;
            }
        }

        return Encoding.UTF8.GetString(bytes, 0, (int)reader.BytesConsumed);
    }

    public static SearchResponseDto ParseSearchResponse(string raw, ILoggerManager logger)
    {
        var results = new Dictionary<string, PlaceDetailResponse>();
        var response = new SearchResponseDto();

        if (raw.StartsWith(XssiPrefix))
            raw = raw[4..].Trim();

        if (raw.Length == 0)
            return response;

        raw = ExtractFirstJsonValue(raw);
        if (raw.Length == 0)
            return response;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (!root.TryGetProperty("d", out var dProp))
                    return response;

                if (dProp.ValueKind == JsonValueKind.String)
                {
                    var dValue = dProp.GetString()!;
                    if (dValue.StartsWith(XssiPrefix))
                        dValue = dValue[4..].Trim();
                    if (dValue.Length == 0)
                        return response;

                    dValue = ExtractFirstJsonValue(dValue);
                    if (dValue.Length == 0)
                        return response;

                    using var dDoc = JsonDocument.Parse(dValue);
                    var payload = dDoc.RootElement;
                    if (payload.ValueKind == JsonValueKind.Array)
                        FindPlaceEntries(payload, results, logger);
                }
                else if (dProp.ValueKind == JsonValueKind.Array)
                {
                    FindPlaceEntries(dProp, results, logger);
                }
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                FindPlaceEntries(root, results, logger);
            }
        }
        catch (JsonException ex)
        {
            logger.LogError($"Failed to parse Google search response: {ex.Message}");
        }

        response.Results = results.Values.ToList();
        return response;
    }

    public static PlaceDetailFullDto? ParsePlaceFullResponse(string raw, ILoggerManager logger)
    {
        if (raw.StartsWith(XssiPrefix))
            raw = raw[4..].Trim();
        if (raw.Length == 0)
            return null;

        raw = ExtractFirstJsonValue(raw);
        if (raw.Length == 0)
            return null;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            JsonElement? dPayload = null;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (!root.TryGetProperty("d", out var dProp))
                    return null;
                if (dProp.ValueKind == JsonValueKind.String)
                {
                    var dValue = dProp.GetString()!;
                    if (dValue.StartsWith(XssiPrefix))
                        dValue = dValue[4..].Trim();
                    if (dValue.Length == 0)
                        return null;
                    dValue = ExtractFirstJsonValue(dValue);
                    if (dValue.Length == 0)
                        return null;
                    using var dDoc = JsonDocument.Parse(dValue);
                    dPayload = dDoc.RootElement.Clone();
                }
                else if (dProp.ValueKind == JsonValueKind.Array)
                {
                    dPayload = dProp.Clone();
                }
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                dPayload = root.Clone();
            }

            if (dPayload is null || dPayload.Value.ValueKind != JsonValueKind.Array)
                return null;

            var (basic, entry) = FindFirstPlaceEntry(dPayload.Value);
            if (basic is null || entry is null)
                return null;

            var result = new PlaceDetailFullDto
            {
                Id = basic.Id,
                Cid = basic.Cid,
                Name = basic.Name,
                Category = basic.Category,
                Rating = basic.Rating,
                TotalReviews = basic.TotalReviews,
                Pricing = basic.Pricing,
                Contacts = basic.Contacts,
                Location = basic.Location,
                Status = basic.Status,
                WeeklyHours = basic.WeeklyHours,
                Features = basic.Features,
                GoogleMapsUrl = string.IsNullOrEmpty(basic.Id)
                    ? ""
                    : $"https://www.google.com/maps/place/?cid={basic.Id}"
            };

            ExtractExtraPlaceFields(entry.Value, result);

            return result;
        }
        catch (JsonException ex)
        {
            logger.LogError($"Failed to parse place detail response: {ex.Message}");
            return null;
        }
    }

    public static (PlaceDetailResponse? basic, JsonElement? entry) FindFirstPlaceEntry(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
            return (null, null);

        foreach (var child in element.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.Array)
            {
                var (basic, entry) = FindFirstPlaceEntry(child);
                if (basic is not null)
                    return (basic, entry);
            }
        }

        string? placeId = null;
        var hasCoords = false;

        foreach (var child in element.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.String)
            {
                var s = child.GetString();
                if (s is not null && IsPlaceId(s))
                {
                    placeId = s;
                    if (hasCoords) break;
                }
            }
            else if (child.ValueKind == JsonValueKind.Array && !hasCoords)
            {
                hasCoords = TryGetCoordinates(child, out _, out _);
                if (hasCoords && placeId is not null) break;
            }
        }

        if (placeId is not null && hasCoords && element.GetArrayLength() >= 10)
            return (ExtractPlaceDetail(element, placeId), element.Clone());

        return (null, null);
    }

    public static void ExtractExtraPlaceFields(JsonElement entry, PlaceDetailFullDto result)
    {
        var len = entry.GetArrayLength();
        var foundFlags = 0;

        ScanAllExtractedFields(entry, result, ref foundFlags);

        if (len > 100 && entry[100].ValueKind == JsonValueKind.Array)
            ExtractFeatures(entry[100], result);

        if (len > 32 && entry[32].ValueKind == JsonValueKind.Array)
        {
            var descArr = entry[32];
            if (descArr.GetArrayLength() > 1 && descArr[1].ValueKind == JsonValueKind.Array)
            {
                var descSub = descArr[1];
                if (descSub.GetArrayLength() > 1 && descSub[1].ValueKind == JsonValueKind.String)
                {
                    var desc = descSub[1].GetString()!;
                    if (!string.IsNullOrEmpty(desc))
                        result.Description = desc;
                }
            }
        }

        ExtractPricingRanges(entry, result);
    }

    public static void ExtractPricingRanges(JsonElement element, PlaceDetailFullDto result)
    {
        if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() < 2)
            return;

        string? rangeKey = null, label = null;
        double? weight = null;

        foreach (var child in element.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.String)
            {
                var s = child.GetString()!;
                if (rangeKey is null && s.StartsWith("E:") && s.Contains("VND"))
                    rangeKey = s;
                else if (label is null && s.Length > 3 && s.Length < 30 && (s.Contains("₫") || s.Contains("$") || s.Contains("đ")))
                    label = s;
            }
            else if (child.ValueKind == JsonValueKind.Number && rangeKey is not null)
            {
                weight = child.GetDouble();
            }
        }

        if (rangeKey is not null && label is not null)
        {
            result.PricingRanges.Add(new PricingRangeDto
            {
                RangeKey = rangeKey,
                Label = label,
                Weight = weight ?? 0
            });
        }

        foreach (var child in element.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.Array)
                ExtractPricingRanges(child, result);
        }
    }

    public static bool TryExtractPlusCode(JsonElement element, PlaceDetailFullDto result)
    {
        if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() < 3)
            return false;
        if (element[2].ValueKind != JsonValueKind.Array || element[2].GetArrayLength() <= 2)
            return false;

        var idx2 = element[2];

        if (idx2[1].ValueKind == JsonValueKind.Array
            && idx2[1].GetArrayLength() > 0
            && idx2[1][0].ValueKind == JsonValueKind.String
            && idx2[1][0].GetString()!.Contains('+'))
        {
            result.GlobalPlusCode = idx2[1][0].GetString()!;
        }

        return !string.IsNullOrEmpty(result.GlobalPlusCode);
    }

    public static void ScanAllExtractedFields(JsonElement element, PlaceDetailFullDto result, ref int foundFlags)
    {
        if (element.ValueKind != JsonValueKind.Array || foundFlags == FlagAll)
            return;

        if ((foundFlags & FlagMenu) == 0 && TryExtractMenuDishes(element, result))
            foundFlags |= FlagMenu;

        if ((foundFlags & FlagMedia) == 0 && TryExtractMediaCategories(element, result))
            foundFlags |= FlagMedia;

        if ((foundFlags & FlagReviews) == 0 && TryExtractReviewContainer(element, result))
            foundFlags |= FlagReviews;

        if ((foundFlags & FlagPlusCode) == 0 && TryExtractPlusCode(element, result))
            foundFlags |= FlagPlusCode;

        if ((foundFlags & FlagPopularTimes) == 0 && TryExtractPopularTimes(element, result))
            foundFlags |= FlagPopularTimes;

        if (foundFlags == FlagAll)
            return;

        foreach (var child in element.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.Array)
                ScanAllExtractedFields(child, result, ref foundFlags);
        }
    }

    public static bool TryExtractReviewContainer(JsonElement element, PlaceDetailFullDto result)
    {
        if (element.GetArrayLength() < 2)
            return false;

        var seen = new HashSet<string>();
        var reviews = new List<ReviewDto>();
        var possible = true;

        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Array || item.GetArrayLength() < 1)
            {
                possible = false;
                break;
            }

            var review = item[0];
            if (review.ValueKind != JsonValueKind.Array || review.GetArrayLength() < 3)
            {
                possible = false;
                break;
            }

            if (review[0].ValueKind != JsonValueKind.String || string.IsNullOrEmpty(review[0].GetString()) || review[0].GetString()!.StartsWith("E:"))
            {
                possible = false;
                break;
            }

            var reviewId = review[0].GetString()!;
            if (!seen.Add(reviewId))
                continue;

            var authorName = "";
            var authorPic = "";
            var relativeDate = "";
            var rating = 0;
            var text = "";
            var photos = new List<ReviewPhotoDto>();

            if (review.GetArrayLength() > 1 && review[1].ValueKind == JsonValueKind.Array)
            {
                var authorArr = review[1];
                if (authorArr.GetArrayLength() > 4 && authorArr[4].ValueKind == JsonValueKind.Array)
                {
                    var a4 = authorArr[4];
                    if (a4.GetArrayLength() > 5 && a4[5].ValueKind == JsonValueKind.Array)
                    {
                        var a5 = a4[5];
                        if (a5.GetArrayLength() > 0 && a5[0].ValueKind == JsonValueKind.String)
                            authorName = a5[0].GetString()!;
                        if (a5.GetArrayLength() > 1 && a5[1].ValueKind == JsonValueKind.String)
                            authorPic = a5[1].GetString()!;
                    }
                }

                if (authorArr.GetArrayLength() > 6 && authorArr[6].ValueKind == JsonValueKind.String)
                    relativeDate = authorArr[6].GetString()!;

                if (authorArr.GetArrayLength() > 13 && authorArr[13].ValueKind == JsonValueKind.Array)
                {
                    var r13 = authorArr[13];
                    if (r13.GetArrayLength() > 4 && r13[4].ValueKind == JsonValueKind.Number)
                        rating = (int)r13[4].GetDouble();
                }
            }

            if (review.GetArrayLength() > 2 && review[2].ValueKind == JsonValueKind.Array)
            {
                var textArr = review[2];

                if (textArr.GetArrayLength() > 2 && textArr[2].ValueKind == JsonValueKind.Array)
                {
                    foreach (var photoItem in textArr[2].EnumerateArray())
                    {
                        if (photoItem.ValueKind != JsonValueKind.Array || photoItem.GetArrayLength() < 2)
                            continue;
                        if (photoItem[0].ValueKind != JsonValueKind.String || photoItem[1].ValueKind != JsonValueKind.Array)
                            continue;

                        var photoId = photoItem[0].GetString()!;
                        if (string.IsNullOrEmpty(photoId))
                            continue;

                        if (photoItem[1].GetArrayLength() <= 6 || photoItem[1][6].ValueKind != JsonValueKind.Array)
                            continue;

                        if (TryParseMediaUrlArray(photoItem[1][6], out var pu, out var pw, out var ph))
                        {
                            photos.Add(new ReviewPhotoDto { Id = photoId, Url = pu, Width = pw, Height = ph });
                        }
                    }
                }

                if (textArr.GetArrayLength() > 15 && textArr[15].ValueKind == JsonValueKind.Array)
                {
                    var t15 = textArr[15];
                    if (t15.GetArrayLength() > 0 && t15[0].ValueKind == JsonValueKind.Array)
                    {
                        var t00 = t15[0];
                        if (t00.GetArrayLength() > 0 && t00[0].ValueKind == JsonValueKind.String)
                            text = t00[0].GetString()!;
                    }
                }
            }

            reviews.Add(new ReviewDto
            {
                ReviewId = reviewId,
                AuthorName = authorName,
                AuthorProfilePic = authorPic,
                Rating = rating,
                RelativeDate = relativeDate,
                Text = text,
                Photos = photos
            });
        }

        if (!possible || reviews.Count == 0)
            return false;

        foreach (var r in reviews)
        {
            if (!result.Reviews.Any(er => er.ReviewId == r.ReviewId))
                result.Reviews.Add(r);
        }

        return true;
    }

    public static string RewriteImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;
        var eqIdx = url.LastIndexOf('=');
        if (eqIdx >= 0 && url.IndexOf('/', eqIdx) == -1)
            return url.Substring(0, eqIdx + 1) + "s0";
        return url;
    }

    public static bool TryParseMediaUrlArray(JsonElement urlArr, out string url, out int width, out int height)
    {
        url = "";
        width = 0;
        height = 0;

        if (urlArr.ValueKind != JsonValueKind.Array || urlArr.GetArrayLength() < 1)
            return false;
        if (urlArr[0].ValueKind != JsonValueKind.String)
            return false;

        url = RewriteImageUrl(urlArr[0].GetString()!);

        if (urlArr.GetArrayLength() > 2 && urlArr[2].ValueKind == JsonValueKind.Array)
        {
            var dims = urlArr[2];
            if (dims.GetArrayLength() > 0 && dims[0].ValueKind == JsonValueKind.Number)
                width = (int)dims[0].GetDouble();
            if (dims.GetArrayLength() > 1 && dims[1].ValueKind == JsonValueKind.Number)
                height = (int)dims[1].GetDouble();
        }

        return !string.IsNullOrEmpty(url);
    }

    public static bool TryExtractMenuDishes(JsonElement element, PlaceDetailFullDto result)
    {
        if (element.GetArrayLength() < 2)
            return false;

        var dishes = new List<MenuDishDto>();

        foreach (var dish in element.EnumerateArray())
        {
            if (dish.ValueKind != JsonValueKind.Array || dish.GetArrayLength() < 7)
                return false;

            if (dish[4].ValueKind != JsonValueKind.String || dish[6].ValueKind != JsonValueKind.Array)
                return false;

            var dishName = dish[4].GetString()!;
            if (string.IsNullOrEmpty(dishName))
                return false;

            var media = new List<MediaItemDto>();
            foreach (var photoItem in dish[6].EnumerateArray())
            {
                if (photoItem.ValueKind != JsonValueKind.Array || photoItem.GetArrayLength() < 7)
                    continue;
                if (photoItem[0].ValueKind != JsonValueKind.String || photoItem[6].ValueKind != JsonValueKind.Array)
                    continue;

                var photoId = photoItem[0].GetString()!;
                if (string.IsNullOrEmpty(photoId))
                    continue;

                if (TryParseMediaUrlArray(photoItem[6], out var pu, out var pw, out var ph))
                {
                    media.Add(new MediaItemDto { Id = photoId, Url = pu, Width = pw, Height = ph });
                }
            }

            if (media.Count == 0)
                continue;

            dishes.Add(new MenuDishDto
            {
                DishName = dishName,
                Media = media
            });
        }

        if (dishes.Count == 0)
            return false;

        result.Menu = dishes;
        return true;
    }

    public static bool TryExtractMediaCategories(JsonElement element, PlaceDetailFullDto result)
    {
        if (element.GetArrayLength() < 2)
            return false;

        var categories = new List<MediaCategoryDto>();

        foreach (var cat in element.EnumerateArray())
        {
            if (cat.ValueKind != JsonValueKind.Array || cat.GetArrayLength() < 4)
                return false;

            if (cat[0].ValueKind != JsonValueKind.String || cat[2].ValueKind != JsonValueKind.String)
                return false;

            var categoryId = cat[0].GetString()!;
            var categoryLabel = cat[2].GetString()!;

            if (string.IsNullOrEmpty(categoryId) || string.IsNullOrEmpty(categoryLabel))
                return false;

            var mediaArr = cat[3];
            if (mediaArr.ValueKind != JsonValueKind.Array)
                return false;

            var media = new List<MediaItemDto>();
            foreach (var mediaItem in mediaArr.EnumerateArray())
            {
                if (mediaItem.ValueKind != JsonValueKind.Array || mediaItem.GetArrayLength() < 7)
                    continue;
                if (mediaItem[0].ValueKind != JsonValueKind.String || mediaItem[6].ValueKind != JsonValueKind.Array)
                    continue;

                var id = mediaItem[0].GetString()!;
                if (string.IsNullOrEmpty(id))
                    continue;

                if (TryParseMediaUrlArray(mediaItem[6], out var pu, out var pw, out var ph))
                {
                    media.Add(new MediaItemDto { Id = id, Url = pu, Width = pw, Height = ph });
                }
            }

            if (media.Count == 0)
                continue;

            categories.Add(new MediaCategoryDto
            {
                CategoryId = categoryId,
                CategoryLabel = categoryLabel,
                Media = media
            });
        }

        if (categories.Count == 0)
            return false;

        result.Photos = categories;
        return true;
    }

    public static bool TryExtractPopularTimes(JsonElement element, PlaceDetailFullDto result)
    {
        if (element.GetArrayLength() != 7)
            return false;

        var dayNames = new[] { "", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" };
        var times = new List<PopularTimeDto>();

        foreach (var dayItem in element.EnumerateArray())
        {
            if (dayItem.ValueKind != JsonValueKind.Array || dayItem.GetArrayLength() < 2)
                return false;

            if (dayItem[0].ValueKind != JsonValueKind.Number)
                return false;

            var dayIndex = (int)dayItem[0].GetDouble();
            if (dayIndex < 1 || dayIndex > 7)
                return false;

            var hoursArr = dayItem[1];
            if (hoursArr.ValueKind != JsonValueKind.Array)
                return false;

            var hours = new List<PopularTimeHourDto>();
            foreach (var hourItem in hoursArr.EnumerateArray())
            {
                if (hourItem.ValueKind == JsonValueKind.Number)
                {
                    hours.Add(new PopularTimeHourDto
                    {
                        OccupancyPercent = (int)hourItem.GetDouble()
                    });
                }
                else if (hourItem.ValueKind == JsonValueKind.Array && hourItem.GetArrayLength() >= 2)
                {
                    var hour = hourItem[0].ValueKind == JsonValueKind.Number ? (int)hourItem[0].GetDouble() : 0;
                    var occupancy = hourItem[1].ValueKind == JsonValueKind.Number ? (int)hourItem[1].GetDouble() : 0;
                    var status = hourItem.GetArrayLength() > 2 && hourItem[2].ValueKind == JsonValueKind.String
                        ? hourItem[2].GetString()!
                        : "";
                    var timeLabel = hourItem.GetArrayLength() > 4 && hourItem[4].ValueKind == JsonValueKind.String
                        ? hourItem[4].GetString()!
                        : "";

                    hours.Add(new PopularTimeHourDto
                    {
                        Hour = hour,
                        OccupancyPercent = occupancy,
                        Status = status,
                        TimeLabel = timeLabel
                    });
                }
                else
                {
                    return false;
                }
            }

            times.Add(new PopularTimeDto { Day = dayNames[dayIndex], Hours = hours });
        }

        if (times.Count != 7)
            return false;

        result.PopularTimes = times;
        return true;
    }

    public static void FindPlaceEntries(JsonElement element, Dictionary<string, PlaceDetailResponse> results, ILoggerManager logger)
    {
        if (element.ValueKind != JsonValueKind.Array)
            return;

        var len = element.GetArrayLength();

        for (int i = 0; i < len; i++)
        {
            if (results.Count >= MaxSearchResults)
                return;

            if (element[i].ValueKind == JsonValueKind.Array)
                FindPlaceEntries(element[i], results, logger);
        }

        if (len < 10 || results.Count >= MaxSearchResults)
            return;

        string? placeId = null;
        var hasCoords = false;

        for (int i = 0; i < len; i++)
        {
            var child = element[i];
            if (child.ValueKind == JsonValueKind.String)
            {
                var s = child.GetString();
                if (s is not null && IsPlaceId(s))
                {
                    placeId = s;
                    if (hasCoords) break;
                }
            }
            else if (child.ValueKind == JsonValueKind.Array && !hasCoords)
            {
                hasCoords = TryGetCoordinates(child, out _, out _);
                if (hasCoords && placeId is not null) break;
            }
        }

        if (placeId is not null && hasCoords && !results.ContainsKey(placeId))
        {
            var detail = ExtractPlaceDetail(element, placeId);
            if (!string.IsNullOrEmpty(detail.Name) || !string.IsNullOrEmpty(detail.Cid))
            {
                logger.LogDebug($"Extracted place: id={detail.Id}, name={detail.Name}");
                results[placeId] = detail;
            }
            else
            {
                logger.LogDebug($"Extraction produced no name/cid for {placeId}");
            }
        }
    }

    public static bool TryGetCoordinates(JsonElement element, out double lat, out double lng)
    {
        lat = 0;
        lng = 0;
        if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() < 4)
            return false;

        var e0 = element[0];
        var e1 = element[1];
        var e2 = element[2];
        var e3 = element[3];

        if (e0.ValueKind != JsonValueKind.Null || e1.ValueKind != JsonValueKind.Null)
            return false;
        if (e2.ValueKind != JsonValueKind.Number || e3.ValueKind != JsonValueKind.Number)
            return false;

        lat = e2.GetDouble();
        lng = e3.GetDouble();
        return true;
    }

    public static PlaceDetailResponse ExtractPlaceDetail(JsonElement entry, string placeId)
    {
        const int nameIdx = 11;
        const int tzIdx = 30;
        const int addrIdx = 39;
        const int cidIdx = 89;
        const int ccIdx = 243;

        var result = new PlaceDetailResponse { Id = placeId };
        var len = entry.GetArrayLength();

        if (len > nameIdx && entry[nameIdx].ValueKind == JsonValueKind.String)
            result.Name = WebUtility.HtmlDecode(entry[nameIdx].GetString()!.Trim());

        if (len > tzIdx && entry[tzIdx].ValueKind == JsonValueKind.String)
            result.Location.Timezone = entry[tzIdx].GetString()!;

        if (len > addrIdx && entry[addrIdx].ValueKind == JsonValueKind.String)
        {
            var raw = WebUtility.HtmlDecode(entry[addrIdx].GetString()!.Trim());
            result.Location.Address = ParseAddress(raw);
        }

        if (len > cidIdx && entry[cidIdx].ValueKind == JsonValueKind.String)
            result.Cid = entry[cidIdx].GetString()!;

        if (len > ccIdx && entry[ccIdx].ValueKind == JsonValueKind.String)
            result.Location.CountryCode = entry[ccIdx].GetString()!;

        if (len > 7 && entry[7].ValueKind == JsonValueKind.Array && entry[7].GetArrayLength() > 0 && entry[7][0].ValueKind == JsonValueKind.String)
        {
            var domain = entry[7][0].GetString()!;
            if (LooksLikeDomain(domain))
                result.Contacts.Website = domain;
        }

        if (len > 88 && entry[88].ValueKind == JsonValueKind.Array)
        {
            foreach (var item in entry[88].EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var s = item.GetString()!;
                    if (s.StartsWith("SearchResult.TYPE_"))
                    {
                        var raw = s["SearchResult.TYPE_".Length..];
                        result.Category = raw;
                        break;
                    }
                }
            }
        }

        if (len > 203 && entry[203].ValueKind == JsonValueKind.Array)
            FindStatusInArray(entry[203], result);

        if (len > 100 && entry[100].ValueKind == JsonValueKind.Array)
            ExtractFeatures(entry[100], result);

        string? pricingStr = null;
        if (len > 4 && entry[4].ValueKind == JsonValueKind.Array)
        {
            var e4 = entry[4];
            if (e4.GetArrayLength() > 2 && e4[2].ValueKind == JsonValueKind.String)
                pricingStr = e4[2].GetString();
        }

        if (pricingStr is not null && TryParsePricing(pricingStr, out var pMin, out var pMax))
        {
            result.Pricing.Min = pMin;
            result.Pricing.Max = pMax;
        }

        if (len > 4 && entry[4].ValueKind == JsonValueKind.Array)
        {
            var e4 = entry[4];
            var e4len = e4.GetArrayLength();

            if (e4len > 7 && e4[7].ValueKind == JsonValueKind.Number)
                result.Rating = Math.Round(e4[7].GetDouble(), 1);

            if (e4len > 8 && e4[8].ValueKind == JsonValueKind.Number)
                result.TotalReviews = (int)e4[8].GetDouble();
        }

        if (len > 178 && entry[178].ValueKind == JsonValueKind.Array)
        {
            if (result.Pricing.Min == 0 && result.Pricing.Max == 0)
                FindPricingInArray(entry[178], result);
            FindPhoneInArray(entry[178], result);
        }

        FindCoordinatesInEntry(entry, result);
        ExtractDetailsFromSubArrays(entry, result);

        return result;
    }

    public static void FindCoordinatesInEntry(JsonElement entry, PlaceDetailResponse result)
    {
        foreach (var child in entry.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.Array && TryGetCoordinates(child, out var lat, out var lng))
            {
                result.Location.Latitude = lat;
                result.Location.Longitude = lng;
                return;
            }
        }
    }

    public static void ExtractDetailsFromSubArrays(JsonElement entry, PlaceDetailResponse result)
    {
        foreach (var child in entry.EnumerateArray())
        {
            if (child.ValueKind != JsonValueKind.Array || child.GetArrayLength() < 2)
                continue;

            var len = child.GetArrayLength();
            for (int i = 0; i < len; i++)
            {
                var item = child[i];
                if (item.ValueKind == JsonValueKind.String)
                {
                    var s = item.GetString()!;
                    if (string.IsNullOrEmpty(s))
                        continue;

                    if (string.IsNullOrEmpty(result.Contacts.Website) && IsWebsite(s))
                        result.Contacts.Website = s;
                    else if (string.IsNullOrEmpty(result.Contacts.Phone) && IsPhone(s))
                        result.Contacts.Phone = s;
                    else if (result.Pricing.Min == 0 && result.Pricing.Max == 0 && TryParsePricing(s, out var pMin, out var pMax))
                    {
                        result.Pricing.Min = pMin;
                        result.Pricing.Max = pMax;
                    }
                }
                else if (item.ValueKind == JsonValueKind.Number)
                {
                    var val = item.GetDouble();
                    if (val >= 1.0 && val <= 5.0 && result.Rating == 0)
                    {
                        result.Rating = Math.Round(val, 1);
                        if (i > 0 && child[i - 1].ValueKind == JsonValueKind.Number)
                            result.TotalReviews = (int)child[i - 1].GetDouble();
                        else if (i + 1 < len && child[i + 1].ValueKind == JsonValueKind.Number)
                            result.TotalReviews = (int)child[i + 1].GetDouble();
                    }
                }
            }
        }
    }

    public static void ExtractFeatures(JsonElement element, FeatureDto features)
    {
        foreach (var child in element.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.String)
            {
                var s = child.GetString()!;
                if (s.StartsWith("/geo/type/establishment_poi/"))
                {
                    var feature = s.Split('/').Last();
                    if (feature.StartsWith("has_wheelchair_accessible_"))
                        AddFeature(features.Accessibility, feature);
                    else if (feature.StartsWith("has_parking_") || feature.StartsWith("parking_") ||
                             feature is "has_free_parking" or "has_street_parking" or "has_valet_parking"
                                 or "has_ev_charging" or "has_parking_lot_free" or "has_parking_valet")
                        AddFeature(features.Parking, feature);
                    else if (feature.StartsWith("accepts_") || feature.StartsWith("pay_"))
                        AddFeature(features.Payments, feature);
                    else if (feature.StartsWith("has_delivery_") || feature.StartsWith("has_dine_in_") ||
                             feature.StartsWith("has_takeout_") || feature.StartsWith("has_no_contact_") ||
                             feature.StartsWith("has_curbside_") || feature.StartsWith("has_online_") ||
                             feature.StartsWith("has_reservation_") || feature.StartsWith("has_service_") ||
                             feature is "has_dine_in" or "has_delivery" or "has_takeout" or "has_no_contact_delivery"
                                 or "has_curbside_pickup" or "has_online_order" or "has_reservation")
                        AddFeature(features.ServiceOptions, feature);
                    else
                        AddFeature(features.Other, feature);
                }
            }
            else if (child.ValueKind == JsonValueKind.Array)
            {
                ExtractFeatures(child, features);
            }
        }
    }

    private static void AddFeature(List<string> list, string feature)
    {
        if (!list.Contains(feature))
            list.Add(feature);
    }

    public static void ExtractFeatures(JsonElement element, PlaceDetailResponse result)
        => ExtractFeatures(element, result.Features);

    public static void ExtractFeatures(JsonElement element, PlaceDetailFullDto result)
        => ExtractFeatures(element, result.Features);

    public static void FindPhoneInArray(JsonElement element, PlaceDetailResponse result)
    {
        foreach (var child in element.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.String)
            {
                var s = child.GetString()!;
                if (string.IsNullOrEmpty(result.Contacts.Phone) && IsPhone(s))
                    result.Contacts.Phone = s;
            }
            else if (child.ValueKind == JsonValueKind.Array)
            {
                FindPhoneInArray(child, result);
            }
        }
    }

    public static void FindPricingInArray(JsonElement element, PlaceDetailResponse result)
    {
        foreach (var child in element.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.String)
            {
                var s = child.GetString()!;
                if (result.Pricing.Min == 0 && result.Pricing.Max == 0 &&
                    s.Any(c => char.GetUnicodeCategory(c) == UnicodeCategory.CurrencySymbol) &&
                    TryParsePricing(s, out var min, out var max))
                {
                    result.Pricing.Min = min;
                    result.Pricing.Max = max;
                }
            }
            else if (child.ValueKind == JsonValueKind.Array)
            {
                FindPricingInArray(child, result);
            }
        }
    }

    public static void FindStatusInArray(JsonElement element, PlaceDetailResponse result)
        => FindStatusInArrayCore(element, result.WeeklyHours, result.Status);

    public static void FindStatusInArray(JsonElement element, PlaceDetailFullDto result)
        => FindStatusInArrayCore(element, result.WeeklyHours, result.Status);

    private static void FindStatusInArrayCore(JsonElement element, List<HourItemDto> weeklyHours, StatusDto status)
    {
        if (element.GetArrayLength() > 0 && element[0].ValueKind == JsonValueKind.Array)
        {
            foreach (var dayEntry in element[0].EnumerateArray())
            {
                if (dayEntry.ValueKind != JsonValueKind.Array || dayEntry.GetArrayLength() < 4)
                    continue;
                var dayName = dayEntry[0];
                if (dayName.ValueKind != JsonValueKind.String)
                    continue;
                var name = dayName.GetString()!;
                var matchedDay = VietnameseDays.FirstOrDefault(d => name.Contains(d, StringComparison.Ordinal));
                if (matchedDay is null)
                    continue;

                var hours = "";
                var hoursArr = dayEntry[3];
                if (hoursArr.ValueKind == JsonValueKind.Array && hoursArr.GetArrayLength() > 0)
                {
                    var firstSlot = hoursArr[0];
                    if (firstSlot.ValueKind == JsonValueKind.Array && firstSlot.GetArrayLength() > 0 && firstSlot[0].ValueKind == JsonValueKind.String)
                        hours = firstSlot[0].GetString()!;
                }

                weeklyHours.Add(new HourItemDto { Day = matchedDay, Hours = hours });
            }

            weeklyHours.Sort((a, b) =>
                VietnameseDayOrder.GetValueOrDefault(a.Day, 99)
                    .CompareTo(VietnameseDayOrder.GetValueOrDefault(b.Day, 99)));
        }

        FindOpenStatusRecursive(element, status);
    }

    public static void FindOpenStatusRecursive(JsonElement element, PlaceDetailResponse result)
        => FindOpenStatusRecursive(element, result.Status);

    public static void FindOpenStatusRecursive(JsonElement element, PlaceDetailFullDto result)
        => FindOpenStatusRecursive(element, result.Status);

    private static void FindOpenStatusRecursive(JsonElement element, StatusDto status)
    {
        foreach (var child in element.EnumerateArray())
        {
            if (child.ValueKind == JsonValueKind.String)
            {
                var s = child.GetString()!;
                if (s.Contains("mở cửa", StringComparison.Ordinal) ||
                    s.Contains("Mở cửa", StringComparison.Ordinal) ||
                    s.Contains("Mở cửa", StringComparison.Ordinal) ||
                    s.Contains("Đóng cửa", StringComparison.Ordinal))
                {
                    status.IsOpenNow =
                        s.StartsWith("Đang", StringComparison.Ordinal) ||
                        s.StartsWith("Mở", StringComparison.Ordinal) ||
                        s.StartsWith("Mở", StringComparison.Ordinal);
                    if (string.IsNullOrEmpty(status.Text))
                        status.Text = s;
                }
            }
            else if (child.ValueKind == JsonValueKind.Array)
            {
                FindOpenStatusRecursive(child, status);
            }
        }
    }
}

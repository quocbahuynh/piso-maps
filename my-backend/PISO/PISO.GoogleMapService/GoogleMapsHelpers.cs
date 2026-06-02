using System.Globalization;
using System.Text.RegularExpressions;
using PISO.Shared.DataTransferObjects.Maps;

namespace PISO.GoogleMapService;

public static class GoogleMapsHelpers
{
    public const string XssiPrefix = ")]}'";
    public const int InfoIndex = 22;
    public const int LatLngIndex = 11;
    public const int CidIndex = 13;
    public const int ThumbnailIndex = 24;

    public const int FlagMenu = 1;
    public const int FlagMedia = 2;
    public const int FlagReviews = 4;
    public const int FlagPlusCode = 8;
    public const int FlagPopularTimes = 16;
    public const int FlagAll = 31;

    public const int MaxSearchResults = 20;


    public static readonly Dictionary<string, int> VietnameseDayOrder = new()
    {
        ["Thứ Hai"] = 0,
        ["Thứ Ba"] = 1,
        ["Thứ Tư"] = 2,
        ["Thứ Năm"] = 3,
        ["Thứ Sáu"] = 4,
        ["Thứ Bảy"] = 5,
        ["Chủ Nhật"] = 6,
    };

    public static readonly HashSet<string> VietnameseDays = new(VietnameseDayOrder.Keys);

    public static bool IsPlaceId(string s) =>
        s.StartsWith("0x") && s.Contains(':') && s.Length >= 32;

    public static bool IsWebsite(string s)
    {
        if (s.Contains("googleusercontent.com")) return false;
        if (s.Contains("/url?q=")) return false;
        if (s.StartsWith("http")) return true;
        return LooksLikeDomain(s);
    }

    public static bool IsPhone(string s) =>
        s.Length >= 7 &&
        s.Length <= 15 &&
        s.Any(char.IsDigit) &&
        !s.Any(char.IsLetter) &&
        !s.Any(c => char.GetUnicodeCategory(c) == UnicodeCategory.CurrencySymbol) &&
        !s.Contains(':') &&
        !s.Contains('–') &&
        !s.Contains('−');

    public static bool LooksLikeDomain(string s) =>
        !string.IsNullOrEmpty(s) &&
        s.Contains('.') &&
        !s.Contains(' ') &&
        !s.Any(char.IsUpper) &&
        s.Any(char.IsLetter) &&
        !s.StartsWith("0x") &&
        s.Length >= 5;

    public static AddressDto ParseAddress(string full)
    {
        var result = new AddressDto { Full = full };
        var parts = full.Split(',', StringSplitOptions.TrimEntries);
        var n = parts.Length;
        if (n == 0) return result;

        result.Country = parts[n - 1];
        if (n == 1) return result;

        result.City = parts[n - 2];
        if (n == 2) return result;

        result.Ward = parts[n - 3];
        if (n == 3) return result;

        result.Street = string.Join(", ", parts, 0, n - 3);
        return result;
    }

    public static bool TryParsePricing(string s, out double min, out double max)
    {
        min = 0;
        max = 0;

        if (string.IsNullOrEmpty(s) || !s.Any(char.IsDigit))
            return false;

        s = s.Replace("\xa0", " ").Trim();

        double multiplier = 1;
        if (s.Contains('N') || s.ToLowerInvariant().Contains("nghìn"))
            multiplier = 1000;
        else if (s.Contains('M') || s.ToLowerInvariant().Contains("triệu"))
            multiplier = 1000000;

        s = Regex.Replace(s, "[₫$€£¥\u20ab]", "");
        s = Regex.Replace(s, "[Nn]ghìn|[NM]", "").Trim();
        s = Regex.Replace(s, @"\.(?=\d{3})", "");

        var matches = Regex.Matches(s, @"[\d,]+");
        var nums = new List<double>();
        foreach (Match m in matches)
        {
            var clean = m.Value.Replace(",", "");
            if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                nums.Add(val);
        }

        if (nums.Count == 0)
            return false;

        if (nums.Count >= 2)
        {
            min = nums[0] * multiplier;
            max = nums[1] * multiplier;
        }
        else
        {
            min = max = nums[0] * multiplier;
        }
        return true;
    }
}

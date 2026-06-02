using System.Text.Json.Serialization;

namespace PISO.Shared.DataTransferObjects.Maps;

/// <summary>A dish item on a place's menu.</summary>
public class MenuDishDto
{
    /// <summary>Name of the dish.</summary>
    [JsonPropertyName("dish_name")]
    public string DishName { get; set; } = string.Empty;

    /// <summary>Media items (photos) associated with the dish.</summary>
    [JsonPropertyName("media")]
    public List<MediaItemDto> Media { get; set; } = new();
}

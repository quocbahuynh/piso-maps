using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PISO.Shared.DataTransferObjects;

namespace PISO.Presentation.Controllers;

[Route("api")]
[ApiController]
public class RootController : ControllerBase
{
    private readonly LinkGenerator _linkGenerator;

    public RootController(LinkGenerator linkGenerator) => _linkGenerator = linkGenerator;

    [HttpGet(Name = "GetRoot")]
    public IActionResult GetRoot()
    {
        var list = new List<Link>
        {
            new()
            {
                Href = _linkGenerator.GetUriByName(HttpContext, nameof(GetRoot), new {})!,
                Rel = "self",
                Method = "GET"
            },
            new()
            {
                Href = _linkGenerator.GetUriByName(HttpContext, "SignUpOrSignIn", new {})!,
                Rel = "users_signup",
                Method = "POST"
            },
            new()
            {
                Href = _linkGenerator.GetUriByName(HttpContext, "GetProfile", new {})!,
                Rel = "users_profile",
                Method = "GET"
            },
            new()
            {
                Href = _linkGenerator.GetUriByName(HttpContext, "RegenerateApiKey", new {})!,
                Rel = "users_regenerate_key",
                Method = "POST"
            },
            new()
            {
                Href = _linkGenerator.GetUriByName(HttpContext, "GetLogs", new {})!,
                Rel = "users_logs",
                Method = "GET"
            },
            new()
            {
                Href = _linkGenerator.GetUriByName(HttpContext, "GetDailyUsage", new {})!,
                Rel = "users_daily_usage",
                Method = "GET"
            },
            new()
            {
                Href = _linkGenerator.GetUriByName(HttpContext, "Autocomplete", new {})!,
                Rel = "maps_autocomplete",
                Method = "GET"
            },
            new()
            {
                Href = _linkGenerator.GetUriByName(HttpContext, "Search", new {})!,
                Rel = "maps_search",
                Method = "GET"
            },
            new()
            {
                Href = _linkGenerator.GetUriByName(HttpContext, "PlaceDetail", new {})!,
                Rel = "maps_place_detail",
                Method = "GET"
            }
        };

        return Ok(list);
    }
}

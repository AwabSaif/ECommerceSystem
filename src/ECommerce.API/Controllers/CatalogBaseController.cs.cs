using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace ECommerce.API.Controllers;

[Route("api/catalog/[controller]")]
[ApiController]
public class CatalogBaseController : ControllerBase
{
    // This property retrieves the current user's ID from the claims in the JWT token.
    public string CurrentUserID
    {
        get
        {
            return User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value ?? string.Empty;
        }
    }

    // This property retrieves the current user's name from the claims in the JWT token.
    public string CurrentUserName
    {
        get
        {
            return User.Identity?.Name ?? "not found";
        }
    }
    public bool IsRTL
    {
        get
        {
            var culture = Request.Headers["Accept-Language"].ToString();
            return string.IsNullOrEmpty(culture) || culture.ToLower().StartsWith("ar") ;
        }
    }
}

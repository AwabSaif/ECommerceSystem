using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Auth.Server.Data;
using Auth.Server.Helpers;
using Auth.Server.ViewModels;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Auth.Server.Controllers;

public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.HasPromptValue(PromptValues.Login))
        {
            var prompt = string.Join(" ", request.GetPromptValues().Remove(PromptValues.Login));
            var parameters = Request.HasFormContentType
                ? Request.Form.Where(p => p.Key != Parameters.Prompt).ToList()
                : Request.Query.Where(p => p.Key != Parameters.Prompt).ToList();
            parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));
            return Challenge(
                new AuthenticationProperties { RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters) },
                IdentityConstants.ApplicationScheme);
        }

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (result == null || !result.Succeeded ||
            (request.MaxAge != null && result.Properties?.IssuedUtc != null &&
             DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
        {
            if (request.HasPromptValue(PromptValues.None))
                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                },
                IdentityConstants.ApplicationScheme);
        }

        var user = await _userManager.GetUserAsync(result.Principal!) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        var subject = (await _userManager.GetUserIdAsync(user))?.ToString() ?? "";
        var clientId = await _applicationManager.GetIdAsync(application);
        var authorizations = await ToListAsync(_authorizationManager.FindAsync(
            subject,
            clientId,
            Statuses.Valid,
            AuthorizationTypes.Permanent,
            request.GetScopes()));

        switch (await _applicationManager.GetConsentTypeAsync(application))
        {
            case ConsentTypes.External when !authorizations.Any():
                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The logged in user is not allowed to access this client application."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Any():
            case ConsentTypes.Explicit when authorizations.Any() && !request.HasPromptValue(PromptValues.Consent):
                var principal = await _signInManager.CreateUserPrincipalAsync(user);
                principal.SetScopes(request.GetScopes());
                principal.SetResources(await ToListAsync(_scopeManager.ListResourcesAsync(principal.GetScopes())));

                var authorization = authorizations.LastOrDefault();
                if (authorization == null)
                    authorization = await _authorizationManager.CreateAsync(principal,
                        subject,
                        clientId?.ToString() ?? "",
                        AuthorizationTypes.Permanent,
                        principal.GetScopes());

                principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
                foreach (var claim in principal.Claims)
                    claim.SetDestinations(GetDestinations(claim, principal));
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            case ConsentTypes.Explicit when request.HasPromptValue(PromptValues.None):
            case ConsentTypes.Systematic when request.HasPromptValue(PromptValues.None):
                return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Interactive user consent is required."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            default:
                return View(new AuthorizeViewModel
                {
                    ApplicationName = await _applicationManager.GetDisplayNameAsync(application) ?? "",
                    Scope = request.Scope
                });
        }
    }

    [Authorize]
    [FormValueRequired("submit.Accept")]
    [HttpPost("~/connect/authorize")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        var user = await _userManager.GetUserAsync(User) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        var subject = (await _userManager.GetUserIdAsync(user))?.ToString() ?? "";
        var authorizations = await ToListAsync(_authorizationManager.FindAsync(
            subject,
            await _applicationManager.GetIdAsync(application),
            Statuses.Valid,
            AuthorizationTypes.Permanent,
            request.GetScopes()));

        if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
            return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "The logged in user is not allowed to access this client application."
            }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        principal.SetScopes(request.GetScopes());
        principal.SetResources(await ToListAsync(_scopeManager.ListResourcesAsync(principal.GetScopes())));

        var authorization = authorizations.LastOrDefault();
        if (authorization == null)
        {
            var clientIdAccept = await _applicationManager.GetIdAsync(application);
            authorization = await _authorizationManager.CreateAsync(principal,
                subject,
                clientIdAccept?.ToString() ?? "",
                AuthorizationTypes.Permanent,
                principal.GetScopes());
        }

        principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
        foreach (var claim in principal.Claims)
            claim.SetDestinations(GetDestinations(claim, principal));
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize]
    [FormValueRequired("submit.Deny")]
    [HttpPost("~/connect/authorize")]
    [ValidateAntiForgeryToken]
    public IActionResult Deny() => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    [HttpGet("~/connect/logout")]
    public IActionResult Logout() => View();

    [ActionName(nameof(Logout))]
    [HttpPost("~/connect/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        await _signInManager.SignOutAsync();
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            return await HandleExchangeCodeGrantType();
        if (request.IsClientCredentialsGrantType())
            return await HandleExchangeClientCredentialsGrantType(request);
        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    private async Task<IActionResult> HandleExchangeClientCredentialsGrantType(OpenIddictRequest request)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!)
            ?? throw new InvalidOperationException("The application details cannot be found in the database.");

        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            Claims.Name,
            Claims.Role);
        identity.AddClaim(Claims.Subject, (await _applicationManager.GetClientIdAsync(application)) ?? "");
        identity.AddClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application) ?? "");

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(request.GetScopes());
        principal.SetResources(await ToListAsync(_scopeManager.ListResourcesAsync(principal.GetScopes())));
        foreach (var claim in principal.Claims)
            claim.SetDestinations(GetDestinations(claim, principal));
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> HandleExchangeCodeGrantType()
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = result.Principal;
        var user = principal != null ? await _userManager.GetUserAsync(principal) : null;
        if (user == null)
            return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
            }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!await _signInManager.CanSignInAsync(user))
            return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
            }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        foreach (var claim in principal!.Claims)
            claim.SetDestinations(GetDestinations(claim, principal));
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
            list.Add(item);
        return list;
    }

    private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Profile)) yield return Destinations.IdentityToken;
                yield break;
            case Claims.Email:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Email)) yield return Destinations.IdentityToken;
                yield break;
            case Claims.Role:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Roles)) yield return Destinations.IdentityToken;
                yield break;
            case "AspNet.Identity.SecurityStamp":
                yield break;
            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}

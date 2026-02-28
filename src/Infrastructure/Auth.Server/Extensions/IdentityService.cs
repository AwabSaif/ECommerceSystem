using Microsoft.AspNetCore.Identity;
using Auth.Server.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Auth.Server.Extensions;

public static class IdentityService
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
            options.ClaimsIdentity.EmailClaimType = Claims.Email;
            options.SignIn.RequireConfirmedAccount = false;
        });

        return services;
    }
}

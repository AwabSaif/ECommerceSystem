using System.Globalization;
using OpenIddict.Abstractions;
using Auth.Server.Data;
using Microsoft.EntityFrameworkCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Auth.Server.Extensions;

public class OpenIddictWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public OpenIddictWorker(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await context.Database.MigrateAsync(cancellationToken);

        await RegisterApplicationsAsync(scope.ServiceProvider, cancellationToken);
        await RegisterScopesAsync(scope.ServiceProvider, cancellationToken);
    }

    private static async Task RegisterApplicationsAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("angularclient", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "angularclient",
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "Angular client PKCE",
                PostLogoutRedirectUris = { new Uri("https://localhost:4200") },
                RedirectUris = { new Uri("https://localhost:4200") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.EndSession,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Revocation,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + "dataEventRecords"
                },
                Requirements = { Requirements.Features.ProofKeyForCodeExchange }
            }, cancellationToken);
        }

        if (await manager.FindByClientIdAsync("rs_dataEventRecordsApi", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "rs_dataEventRecordsApi",
                ClientSecret = "dataEventRecordsSecret",
                DisplayName = "DataEventRecords API",
                Permissions = { Permissions.Endpoints.Introspection }
            }, cancellationToken);
        }

        if (await manager.FindByClientIdAsync("CC", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "CC",
                ClientSecret = "cc_secret",
                DisplayName = "Client Credentials API",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + "dataEventRecords"
                }
            }, cancellationToken);
        }
    }

    private static async Task RegisterScopesAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

        if (await manager.FindByNameAsync("dataEventRecords", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictScopeDescriptor
            {
                DisplayName = "DataEventRecords API access",
                Name = "dataEventRecords",
                Resources = { "rs_dataEventRecordsApi" }
            }, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

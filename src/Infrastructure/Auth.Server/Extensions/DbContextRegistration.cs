using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Auth.Server.Data;

namespace Auth.Server.Extensions;

public static class DbContextRegistration
{
    public static IServiceCollection AddAuthDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection",
        Action<DbContextOptionsBuilder>? additionalConfiguration = null)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySqlOptions => mySqlOptions
                    .MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName)
                    .EnableRetryOnFailure(3));

            additionalConfiguration?.Invoke(options);
        });

        return services;
    }
}

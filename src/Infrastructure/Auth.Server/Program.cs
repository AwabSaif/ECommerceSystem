using Auth.Server.Extensions;
using Microsoft.EntityFrameworkCore;
using Auth.Server.Data;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) => config
        .WriteTo.Console()
        .ReadFrom.Configuration(context.Configuration));

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAuthDbContext(builder.Configuration, "DefaultConnection", options => options.UseOpenIddict());
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddIdentityService();
    builder.Services.AddOpenIddictService(builder.Configuration);

    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
        ?? new[] { "https://localhost:4200", "https://localhost:4204" };
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowOrigins", policy =>
        {
            policy.AllowCredentials()
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseStatusCodePagesWithReExecute("~/Home/Error");
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("AllowOrigins");
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapControllers();
    app.MapDefaultControllerRoute();
    app.MapRazorPages();

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name != "StopTheHostException" && ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.CloseAndFlush();
}

using OpenIddict.Validation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Data;
using Modules.Orders.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string not found.");

// Configure Entity Framework Core to use MySQL with the provided connection string.
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Swagger Setings 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the OpenIddict validation services.
builder.Services.AddOpenIddict()
     .AddValidation(options =>
     {
         // Note: the validation handler uses OpenID Connect discovery
         // to retrieve the address of the introspection endpoint.
         options.SetIssuer("https://localhost:5000/");
         options.AddAudiences("ecommerce-api");
         options.UseIntrospection()
                .SetClientId("ecommerce-api")
                .SetClientSecret("ecommerce-secret");
         options.UseAspNetCore();

         options.UseSystemNetHttp();
         options.UseAspNetCore();
     });

// Register the authentication and authorization services.
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

// Register MediatR and scan the assembly for handlers (like CreateProductCommandHandler) --- IGNORE ---
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Modules.Catalog.Data.CatalogDbContext).Assembly);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllers();

app.MapGet("/", (IWebHostEnvironment env) => 
    $"ECommerce !!!!\n\nRunning in \"{env.EnvironmentName}\" Environment");

// Run the application
app.Run();

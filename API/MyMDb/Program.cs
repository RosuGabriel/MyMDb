using Microsoft.EntityFrameworkCore;
using MyMDb.Data;
using MyMDb.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureApplicationServices(builder);
builder.Services.ConfigureAuthentication(builder);
builder.Services.ConfigureSwagger();
builder.Services.ConfigureCors(builder);
builder.Services.ConfigureStaticFiles(builder);
var isLinuxDevelopment = builder.Services.ConfigureDatabase(builder);

// App configuration
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (isLinuxDevelopment)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to apply database migrations");
        throw;
    }
}

app.ConfigureMiddlewares();

app.Run(builder.Configuration["ConnectionDetails:ServerAddress"]);

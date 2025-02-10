using Microsoft.EntityFrameworkCore;
using MyMDb.Data;

namespace MyMDb.StartupExtensions
{
    public static class DatabaseConfiguration
    {
        public static void ConfigureDatabase(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var isLinux = Environment.OSVersion.Platform == PlatformID.Unix;
            var connectionString = isLinux
                ? builder.Configuration.GetConnectionString("LinuxDefaultConnection")
                : builder.Configuration.GetConnectionString("WindowsDefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Database connection string is not configured!");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }
    }
}

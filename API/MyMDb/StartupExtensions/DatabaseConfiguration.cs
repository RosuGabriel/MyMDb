using Microsoft.EntityFrameworkCore;
using MyMDb.Data;

namespace MyMDb.StartupExtensions
{
    public static class DatabaseConfiguration
    {
        public static bool ConfigureDatabase(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var isLinux = Environment.OSVersion.Platform == PlatformID.Unix;
            var isDevelopment = builder.Environment.IsDevelopment();
            string connectionString;

            if (isLinux)
            {
                if (isDevelopment)
                {
                    Console.WriteLine("The app is running on Linux in development mode.");
                    connectionString = builder.Configuration.GetConnectionString("DevLinuxConnection")!;
                }
                else
                {
                    Console.WriteLine("The app is running on Linux in production mode.");
                    connectionString = builder.Configuration.GetConnectionString("ProdLinuxConnection")!;
                }
            }
            else
            {
                Console.WriteLine("The app is running on Windows.");
                connectionString = builder.Configuration.GetConnectionString("DevWindowsConnection")!;
            }

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Database connection string is not configured!");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });
            });

            return isLinux && isDevelopment;
        }
    }
}

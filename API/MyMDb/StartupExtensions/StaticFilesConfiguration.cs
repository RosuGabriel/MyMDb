using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;

namespace MyMDb.StartupExtensions
{
    public static class StaticFilesConfiguration
    {
        public static void ConfigureStaticFiles(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 50L * 1024 * 1024 * 1024; // 50 GB
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 50L * 1024 * 1024 * 1024;
            });
        }

        public static void ConfigureStaticFileRoutes(this WebApplication app, WebApplicationBuilder builder)
        {
            if (builder.Environment.IsDevelopment())
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), builder.Configuration["Paths:RootDev"]!);
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(rootPath),
                    RequestPath = "/static"
                });
            }
            else
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), builder.Configuration["Paths:Root"]!);
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(rootPath),
                    RequestPath = "/mymdb/static"
                });
            }
        }
    }
}

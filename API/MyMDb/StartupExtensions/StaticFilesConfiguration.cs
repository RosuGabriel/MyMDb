using Microsoft.AspNetCore.Http.Features;

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
    }
}

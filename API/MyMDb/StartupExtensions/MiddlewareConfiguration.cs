using Microsoft.Extensions.FileProviders;
using MyMDb.Helpers;

namespace MyMDb.StartupExtensions
{
    public static class MiddlewareConfiguration
    {
        public static void ConfigureMiddlewares(this WebApplication app)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Protect static files if configured
            if (int.Parse(app.Configuration["ProtectStaticFiles"]!) == 1)
            {
                app.UseMiddleware<ProtectedStaticFilesMiddleware>("/mymdb/static");
            }

            // Enable CORS policy
            app.UseCors("AllowMyClient");

            // Map controllers
            app.MapControllers();

            // Environment-specific configurations
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();

                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), app.Configuration["Paths:RootDev"]!);
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(rootPath),
                    RequestPath = "/static"
                });
            }
            else
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), app.Configuration["Paths:Root"]!);
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(rootPath),
                    RequestPath = "/mymdb/static"
                });
            }
        }
    }
}

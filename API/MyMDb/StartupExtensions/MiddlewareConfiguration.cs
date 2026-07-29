using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
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

            var static_files_url = "/mymdb/static";
            app.UseMiddleware<ProtectedStaticFilesMiddleware>(static_files_url);

            app.UseCors("AllowMyClient");

            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), app.Configuration["Paths:Root"]!);

            var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
            fileExtensionContentTypeProvider.Mappings[".vtt"] = "text/vtt";

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(rootPath),
                RequestPath = static_files_url,
                ContentTypeProvider = fileExtensionContentTypeProvider,
                ServeUnknownFileTypes = false,
                OnPrepareResponse = ctx =>
                {
                    // Enable range requests for video streaming
                    ctx.Context.Response.Headers.Append("Accept-Ranges", "bytes");
                }
            });
        }
    }
}

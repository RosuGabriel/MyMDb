using MyMDb.RepositoryInterfaces;
using MyMDb.Repositories;
using MyMDb.ServiceInterfaces;
using MyMDb.Services;
using MyMDb.Data;
using MyMDb.Models;
using Microsoft.AspNetCore.Identity;

namespace MyMDb.StartupExtensions
{
    public static class ServicesConfiguration
    {
        public static void ConfigureApplicationServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            // Add controllers with JSON options
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                options.JsonSerializerOptions.WriteIndented = true;
            });

            // Repositories
            services.AddScoped<IMediaRepository, MediaRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IMediaAttributeRepository, MediaAttributeRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IContinueWatchingRepository, ContinueWatchingRepository>();

            // Services
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFileProcessingService, FileProcessingService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IContinueWatchingService, ContinueWatchingService>();

            // Identity
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints()
            .AddDefaultTokenProviders();

            // AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}

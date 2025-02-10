namespace MyMDb.StartupExtensions
{
    public static class CorsConfiguration
    {
        public static void ConfigureCors(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var allowedClients = builder.Configuration.GetSection("ConnectionDetails:AllowedClients").Get<string[]>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowMyClient", policyBuilder =>
                {
                    policyBuilder.WithOrigins(allowedClients!)
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                });
            });
        }
    }
}

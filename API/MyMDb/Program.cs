using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyMDb.Data;
using MyMDb.Models;
using MyMDb.Repositories;
using MyMDb.RepositoryInterfaces;
using MyMDb.ServiceInterfaces;
using MyMDb.Services;
using MyMDb.Helpers;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Serilog;



// ------------------------------ Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ProtectedStaticFilesMiddleware>();

var isLinux = Environment.OSVersion.Platform == PlatformID.Unix;

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure Swagger OpenApi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            },
            Scheme = "Bearer",
            Name = "Bearer",
            In = ParameterLocation.Header,
        },
        new List<string>()
    }
});
});

// Max dimension of file upload
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50L * 1024 * 1024 * 1024; // 50 GB
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50L * 1024 * 1024 * 1024;
});

// Database connection based on OS
System.String? connectionString;
if (isLinux)
{
    connectionString = builder.Configuration.GetConnectionString("LinuxDefaultConnection") ??
        throw new InvalidOperationException("Connection string 'DefaultConnection for Linux not found!'");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("WindowsDefaultConnection") ??
        throw new InvalidOperationException("Connection string 'DefaultConnection for Windows not found!'");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Repositories
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMediaAttributeRepository, MediaAttributeRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Services
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>( options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddApiEndpoints()
.AddDefaultTokenProviders();

var keyString = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(keyString))
{
    throw new InvalidOperationException("JWT key is not configured.");
}

var key = Encoding.ASCII.GetBytes(keyString);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy =>
        policy.RequireRole("admin"));

    options.AddPolicy("user", policy =>
        policy.RequireRole("user"));
});

builder.Services.Configure<IdentityOptions>(options => 
{

});

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var allowedClients = builder.Configuration.GetSection("ConnectionDetails:AllowedClients").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyClient",
            builder =>
            {
                builder.WithOrigins(allowedClients!)
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
});

// ------------------------------ App
var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (int.Parse(builder.Configuration["ProtectStaticFiles"]!) == 1)
{
    app.UseMiddleware<ProtectedStaticFilesMiddleware>("/mymdb/static");
}

app.UseCors("AllowMyClient");

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
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

var serverAddress = builder.Configuration["ConnectionDetails:ServerAddress"];
app.Run(serverAddress);

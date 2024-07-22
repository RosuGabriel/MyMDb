using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyMDb.Data;
using MyMDb.Models;
using MyMDb.Repositories;
using MyMDb.RepositoryInterfaces;
using MyMDb.ServiceInterfaces;
using MyMDb.Services;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.Extensions.FileProviders;



// ------------------------------ Builder
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection not found!'");

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


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Repositories
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();

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

builder.Logging.ClearProviders();
builder.Services.AddLogging();


// ------------------------------ App
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseStaticFiles();
}
else
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), builder.Configuration["ConnectionDetails:RootLocation"]!)),
        RequestPath = ""
    });
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowMyClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<AppUser>();

app.MapControllers();

var serverAddress = builder.Configuration["ConnectionDetails:ServerAddress"]; 
app.Run(serverAddress);
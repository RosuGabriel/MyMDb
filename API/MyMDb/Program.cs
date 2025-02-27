using MyMDb.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureApplicationServices(builder);
builder.Services.ConfigureAuthentication(builder);
builder.Services.ConfigureSwagger();
builder.Services.ConfigureCors(builder);
builder.Services.ConfigureStaticFiles(builder);
builder.Services.ConfigureDatabase(builder);

// App configuration
var app = builder.Build();

app.ConfigureMiddlewares();

app.Run(builder.Configuration["ConnectionDetails:ServerAddress"]);

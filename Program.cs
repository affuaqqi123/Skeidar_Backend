using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Configuration;
using WebApi.DAL;
using System.IO;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using WebApi.Model;
using Microsoft.Extensions.Options;
using System.Security.Principal;
using Microsoft.AspNetCore.ResponseCompression;
using WebApi.Middlewares;
using WebAPI.Middlewares;
using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Serilog.Sinks.ApplicationInsights.Formatters;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;


//private static string[]? _compressMimeTypes;
//string[] _compressMimeTypes = new string[] {};
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CourseDbConnection")));

 builder.Services.Configure<WebApi.Model.EmailModel>(builder.Configuration.GetSection("EConfiguration"));

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);

//Configure services for localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");


// For Identity
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddDefaultTokenProviders();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options=>
    {
        options.Password.RequireUppercase = false;
        // Customize allowed characters for the username
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ "; // Add space as an allowed character
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
//_compressMimeTypes = new[]
//{
//     "text/css",
//     "application/javascript",
//     "application/json",
//     "application/zip",
//     "application/pdf",
//     "image/jpeg",
//     "image/gif",
//     "image/png",
//     "image/svg+xml",
//     "image/avif",
//     "image/webp",
//     "video/mp4",
//     "video/webm",
//     "model/gltf-binary",
//     "model/vnd.usdz+zip"
//};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        //ClockSkew = TimeSpan.FromSeconds(0),
        ValidIssuer = builder.Configuration["JwtToken:Issuer"],
        ValidAudience = builder.Configuration["JwtToken:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtToken:SecretKey"]))
    };
});


// Add services to the container.

builder.Services.AddControllers();
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Services.AddSingleton(Log.Logger);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddResponseCompression(options =>
//{
//    // Normally this would open up for BREACH attacks.
//    // Compression middleware configured below is used only for assets.
//    options.EnableForHttps = true;
//    options.Providers.Add<GzipCompressionProvider>();
//    options.MimeTypes = _compressMimeTypes;
//});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});




builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder =>
        {
            builder.AllowAnyMethod()
                   .AllowAnyHeader()
                   .WithOrigins("http://localhost:3000")
                   .AllowCredentials();
        });
});
var app = builder.Build();
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ErrorLoggingMiddleware>();

app.UseCors("CorsPolicy");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "assets")),
    RequestPath = "/assets"
});

//Configure localization
var supportedCultures = new[] {
    new CultureInfo("en-US"),
    //new CultureInfo("fr-FR"),
    new CultureInfo("nb-NO"),
    //new CultureInfo("nn-NO")
    // Add more cultures as needed
};
//app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});


app.UseDefaultFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

//app.MapGet("/localization/{key}", (IStringLocalizer<Program> localizer, string key) =>
//{
//    var localizedString = localizer[key];
//    return Results.Ok(localizedString);
//}).AllowAnonymous();


app.MapControllers();

app.Run();


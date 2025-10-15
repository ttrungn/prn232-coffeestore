using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PRN232.Lab1.CoffeeStore.API.Infrastructure;
using PRN232.Lab1.CoffeeStore.API.Utils;

namespace PRN232.Lab1.CoffeeStore.API;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddProblemDetails();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
            c.DescribeAllParametersInCamelCase();
            c.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    []
                }
            });
        });
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
                options.Filters.Add(new FormatFilterAttribute());
                options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "application/xml");
                options.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/json");
            })
            .AddXmlSerializerFormatters()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireAudience = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        configuration["JwtSettings:SecretKey"]
                        ?? throw new ArgumentException("JwtSettings:Key is not found")))
                };
            });
        services.AddResponseCompression(options =>
        {
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            // options.MimeTypes = ["application/json"];
        });
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.SmallestSize;
        });
        services.AddCors(options =>
        {
            options.AddPolicy("AllowLocationHeader", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Location", "X-Pagination");
            });
        });
        services.AddScoped<ApplicationDbContextInitializer>();

        //Rate Limiter
        var globalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

            return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = configuration.GetValue<int>("RateLimiter:PermitLimit"),
                Window = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimiter:WindowM")),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = configuration.GetValue<int>("RateLimiter:QueueLimit")
            });
        });

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = globalLimiter;
            options.AddPolicy("auth-fixed", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = configuration.GetValue<int>("RateLimiterLogin:PermitLimit"),
                        Window = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimiterLogin:WindowM")),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = configuration.GetValue<int>("RateLimiterLogin:QueueLimit")
                    }));
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }
}

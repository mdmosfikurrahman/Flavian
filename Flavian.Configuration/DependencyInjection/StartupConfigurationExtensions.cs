using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Flavian.Configuration.Middlewares;
using Flavian.Persistence.Data;
using Flavian.Shared.Config;
using Flavian.Shared.Dto;
using Flavian.Shared.Swagger;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Flavian.Configuration.DependencyInjection;

public static class StartupConfigurationExtensions
{
    public static void ConfigureProjectSettings(this WebApplicationBuilder builder, IConfiguration config)
    {
        builder.ConfigureDatabase();
        builder.ConfigureApiVersioning();
        builder.ConfigureMvcAndSwagger();
        builder.ConfigureGraphQl();
        builder.ConfigureAppSettings();
        builder.ConfigureCors();
        builder.ConfigureForwardedHeaders();
        builder.Services.RegisterApplicationServices(config);
        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
        builder.ConfigureJwtSettings();
        builder.ConfigureAuthSecurity();
        builder.ConfigureJwtAuthentication();
        builder.Services.AddDistributedMemoryCache();
        builder.ConfigureRateLimiting();
    }

    private static void ConfigureDatabase(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Environment.IsDevelopment()
            ? builder.Configuration.GetConnectionString("DefaultConnection")
            : Environment.GetEnvironmentVariable("ConnectionString");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                builder.Environment.IsDevelopment()
                    ? "Connection string 'DefaultConnection' is not configured in appsettings."
                    : "Environment variable 'ConnectionString' is not set.");
        }

        builder.Services.AddDbContext<FlavianDbContext>(options =>
            options.UseSqlServer(connectionString));
    }

    private static void ConfigureApiVersioning(this WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    private static void ConfigureMvcAndSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithGlobalRoutePrefix("api")
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.UseInlineDefinitionsForEnums();

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your valid token."
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
                        }
                    },
                    []
                }
            });
        });

        builder.Services.AddHttpContextAccessor();
    }

    private static void ConfigureGraphQl(this WebApplicationBuilder builder)
    {
        var assembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Flavian.Application");

        var queryResolvers = assembly!.GetTypes().Where(type =>
            type is { IsClass: true, IsAbstract: false, Namespace: not null } &&
            type.Namespace.StartsWith("Flavian.Application.Resolvers") &&
            type.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), true)
                .Any(attr => ((ExtendObjectTypeAttribute)attr).Name == "Query")
        );

        var mutationResolvers = assembly.GetTypes().Where(type =>
            type is { IsClass: true, IsAbstract: false, Namespace: not null } &&
            type.Namespace.StartsWith("Flavian.Application.Resolvers") &&
            type.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), true)
                .Any(attr => ((ExtendObjectTypeAttribute)attr).Name == "Mutation")
        );

        var schemaBuilder = builder.Services.AddGraphQLServer()
            .AddQueryType(descriptor => descriptor.Name("Query"))
            .AddMutationType(descriptor => descriptor.Name("Mutation"));

        foreach (var resolver in queryResolvers)
        {
            schemaBuilder.AddTypeExtension(resolver);
        }

        foreach (var resolver in mutationResolvers)
        {
            schemaBuilder.AddTypeExtension(resolver);
        }
    }

    private static void ConfigureAppSettings(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SchemaSettings>(builder.Configuration.GetSection("SchemaSettings"));
    }

    private static void ConfigureJwtSettings(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
    }

    private static void ConfigureAuthSecurity(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AuthSecurityOptions>(
            builder.Configuration.GetSection("AuthSecurity"));
    }

    private static void ConfigureJwtAuthentication(this WebApplicationBuilder builder)
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Jwt settings are missing.");

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = ApiResponseHelper.Unauthorized("Authentication", "You are not authorized.");
                        await context.Response.WriteAsJsonAsync(response);
                    },

                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = ApiResponseHelper.Forbidden("Authorization", "Access is forbidden.");
                        await context.Response.WriteAsJsonAsync(response);
                    }
                };
            });

        builder.Services.AddAuthorization();
    }

    private static void ConfigureCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", cors =>
            {
                cors
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
                    .AllowCredentials();
            });
        });
    }

    private static void ConfigureForwardedHeaders(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
    }

    private static void ConfigureRateLimiting(this WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();

        builder.Services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            {
                var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
        });
    }
}

using System.IO.Compression;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using ITI.ERP.Application.Validators.Trade;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using ITI.ERP.Api.Middleware;
using ITI.ERP.Infrastructure;
using ITI.ERP.Infrastructure.BackgroundJobs;
using ITI.ERP.Infrastructure.Persistence;
using ITI.ERP.Infrastructure.Persistence.Seeds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/erp-api-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting ITI ERP API host");

    builder.Host.UseSerilog();

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateTradeRequestValidator>();

    builder.Services.AddControllers();

    builder.Services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ITI ERP API",
            Version = "v1",
            Description = "ITI ERP System API"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token"
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
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddInfrastructure(builder.Configuration);

    var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? Environment.GetEnvironmentVariable("JwtSettings__Secret")
    ?? throw new InvalidOperationException("JwtSettings:Secret is not configured. Set it via environment variable or user-secrets.");

    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
        throw new InvalidOperationException($"JwtSettings:Secret must be at least 32 characters. Current length: {jwtSecret.Length}");

    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "ITI.ERP.Api";
    var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "ITI.ERP.Client";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("JWT token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        var permissions = new[]
        {
            "Users.View", "Users.Manage", "Users.ResetPassword", "Users.Unlock","Users.Edit","Users.ToggleStatus","Users.CreateTradeHead",
            "Student.View", "Student.Create", "Student.Edit", "Student.Delete", "Student.Archive", "Student.Transfer", "Student.Photo", "Student.Import",
            "Institute.View", "Institute.Edit", "Institute.Delete",
            "AcademicSession.View", "AcademicSession.Create", "AcademicSession.Edit", "AcademicSession.Delete", "AcademicSession.Lock", "AcademicSession.Activate",
            "Trade.View", "Trade.Create", "Trade.Edit", "Trade.Archive", "Trade.AssignHead", "Trade.Import",
            "Attendance.View", "Attendance.Mark", "Attendance.Unlock", "Attendance.Export",
            "Practical.View", "Practical.Create", "Practical.Edit", "Practical.Delete", "Practical.Lock", "Practical.Unlock",
            "Reports.View", "Reports.Export",
            "Settings.Manage",
            "Dashboard.View",
            "Holiday.View", "Holiday.Create", "Holiday.Delete",
            "Batch.View", "Batch.Create", "Batch.Edit", "Batch.Archive"
        };

        foreach (var permission in permissions)
        {
            options.AddPolicy(permission, policy =>
                policy.RequireClaim("permission", permission));
        }

        options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        options.AddPolicy("Permission", policy =>
            policy.Requirements.Add(new ITI.ERP.Infrastructure.Authorization.PermissionRequirement(string.Empty)));
    });

    builder.Services.AddHangfire(config =>
        config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddHangfireServer();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy.SetIsOriginAllowed(_ => true)
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });
    }
    else
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();

        if (allowedOrigins.Length == 0)
        {
            var envOrigins = Environment.GetEnvironmentVariable("AllowedOrigins");
            if (!string.IsNullOrWhiteSpace(envOrigins))
                allowedOrigins = envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });
    }

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;

        options.AddFixedWindowLimiter("fixed", limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 10;
        });

        options.AddSlidingWindowLimiter("sliding", limiterOptions =>
        {
            limiterOptions.PermitLimit = 60;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.SegmentsPerWindow = 6;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 5;
        });

        options.AddFixedWindowLimiter("forgotPassword", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(15);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("resetPassword", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(15);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("verifyToken", limiterOptions =>
        {
            limiterOptions.PermitLimit = 10;
            limiterOptions.Window = TimeSpan.FromMinutes(15);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("setup", limiterOptions =>
        {
            limiterOptions.PermitLimit = 3;
            limiterOptions.Window = TimeSpan.FromHours(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("login", limiterOptions =>
        {
            limiterOptions.PermitLimit = 10;
            limiterOptions.Window = TimeSpan.FromMinutes(5);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });
        
        options.AddFixedWindowLimiter("refreshToken", limiterOptions =>
        {
            limiterOptions.PermitLimit = 20;
            limiterOptions.Window = TimeSpan.FromMinutes(5);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });
        
        options.AddPolicy("adminReset", context =>
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "anonymous";
            return RateLimitPartition.GetFixedWindowLimiter(
                userId,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0
                });
        });

        options.OnRejected = async (context, ct) =>
        {
            Log.Warning("Rate limit exceeded for {Ip} on {Path}",
                context.HttpContext.Connection.RemoteIpAddress,
                context.HttpContext.Request.Path);

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((int)retryAfter.TotalSeconds).ToString();
            }

            var body = JsonSerializer.Serialize(new
            {
                error = "Too many requests. Please try again later."
            });
            await context.HttpContext.Response.WriteAsync(body, ct);
        };
    });

    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "postgresql",
            tags: new[] { "db", "ready" });

    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ITI ERP API v1");
        });
    }

    app.UseForwardedHeaders();

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseCors("AllowFrontend");

    app.UseAuthentication();

    app.UseRateLimiter();

    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration.TotalMilliseconds,
                    description = e.Value.Description
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            };
            await context.Response.WriteAsJsonAsync(result);
        }
    });

    app.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
        DashboardTitle = "ITI ERP Background Jobs",
        IsReadOnlyFunc = _ => false,
        Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
    });

    await ApplicationDbContextSeed.SeedAsync(app.Services.GetRequiredService<IServiceScopeFactory>());
    

    app.Services.RegisterRecurringJobs();

    await app.RunAsync();
}
catch (HostAbortedException)
{
    // Expected during EF Core design-time operations (dotnet ef).
    // The host is intentionally aborted — not an application error.
}
catch (Exception ex)
{
    Log.Fatal(ex, "ITI ERP API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; } = 15;
    public int RefreshTokenExpiryInDays { get; set; } = 7;
}

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.IsInRole("Admin");
    }
}

public partial class Program { }

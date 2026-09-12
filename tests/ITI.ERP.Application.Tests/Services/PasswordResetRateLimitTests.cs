using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Configuration;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Application.Services;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Infrastructure.Persistence;
using ITI.ERP.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Moq;
using Xunit;

namespace ITI.ERP.Application.Tests.Services;

[Collection("RateLimitTests")]
public class PasswordResetRateLimitTests : IAsyncLifetime
{
    private RateLimitWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new RateLimitWebApplicationFactory();
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ForgotPassword_Allows5RequestsPer15Min()
    {
        for (int i = 0; i < 5; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
                new { email = $"user{i}@example.com" });
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests,
                "rate limiter should allow the request through");
        }
    }

    [Fact]
    public async Task ForgotPassword_Rejects6thRequestWith429()
    {
        for (int i = 0; i < 6; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
                new { email = "ratelimit-test@example.com" });
            if (i < 5)
                response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
            else
            {
                response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
                var body = await response.Content.ReadFromJsonAsync<RateLimitErrorResponse>();
                body.Should().NotBeNull();
                body!.Error.Should().Be("Too many requests. Please try again later.");
            }
        }
    }

    [Fact]
    public async Task ResetPassword_Allows5RequestsPer15Min()
    {
        for (int i = 0; i < 5; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password",
                new { token = $"fake-token-{i}", newPassword = "NewPass1!" });
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }
    }

    [Fact]
    public async Task ResetPassword_Rejects6thRequestWith429()
    {
        for (int i = 0; i < 6; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password",
                new { token = $"fake-token-{i}", newPassword = "NewPass1!" });
            if (i < 5)
                response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
            else
                response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        }
    }

    [Fact]
    public async Task VerifyToken_Allows10RequestsPer15Min()
    {
        for (int i = 0; i < 10; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/v1/auth/verify-reset-token",
                new { token = $"fake-token-{i}" });
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }
    }

    [Fact]
    public async Task VerifyToken_Rejects11thRequestWith429()
    {
        for (int i = 0; i < 11; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/v1/auth/verify-reset-token",
                new { token = $"fake-token-{i}" });
            if (i < 10)
                response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
            else
                response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        }
    }

    [Fact]
    public async Task AdminReset_Allows20RequestsPer15Min()
    {
        var token = RateLimitWebApplicationFactory.GenerateTestJwt();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        for (int i = 0; i < 20; i++)
        {
            var userId = Guid.NewGuid();
            var response = await _client.PostAsJsonAsync($"/api/v1/users/{userId}/send-password-reset", new { });
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }
    }

    [Fact]
    public async Task AdminReset_Rejects21stRequestWith429()
    {
        var token = RateLimitWebApplicationFactory.GenerateTestJwt();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        for (int i = 0; i < 21; i++)
        {
            var userId = Guid.NewGuid();
            var response = await _client.PostAsJsonAsync($"/api/v1/users/{userId}/send-password-reset", new { });
            if (i < 20)
                response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
            else
                response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        }
    }

    [Fact]
    public async Task AdminReset_DifferentAdminsHaveSeparateQuotas()
    {
        using var client1 = _factory.CreateClient();
        using var client2 = _factory.CreateClient();

        var token1 = RateLimitWebApplicationFactory.GenerateTestJwt(Guid.NewGuid());
        var token2 = RateLimitWebApplicationFactory.GenerateTestJwt(Guid.NewGuid());

        client1.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        client2.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        for (int i = 0; i < 20; i++)
        {
            var r1 = await client1.PostAsJsonAsync($"/api/v1/users/{Guid.NewGuid()}/send-password-reset", new { });
            r1.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        for (int i = 0; i < 20; i++)
        {
            var r2 = await client2.PostAsJsonAsync($"/api/v1/users/{Guid.NewGuid()}/send-password-reset", new { });
            r2.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }
    }

    // ─────────────────────────────────────────────
    // C. Rejection response format
    // ─────────────────────────────────────────────

    [Fact]
    public async Task Rejection_Returns429StatusCode()
    {
        for (int i = 0; i < 6; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
                new { email = "rejection-test@example.com" });
        }

        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
            new { email = "rejection-test@example.com" });
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Rejection_ReturnsJsonErrorBody()
    {
        for (int i = 0; i < 6; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
                new { email = "json-body-test@example.com" });
        }

        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
            new { email = "json-body-test@example.com" });
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("application/json");

        var body = await response.Content.ReadFromJsonAsync<RateLimitErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Should().Be("Too many requests. Please try again later.");
    }

    [Fact]
    public async Task Rejection_IncludesRetryAfterHeader()
    {
        for (int i = 0; i < 6; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
                new { email = "retry-after-test@example.com" });
        }

        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password",
            new { email = "retry-after-test@example.com" });
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        response.Headers.TryGetValues("Retry-After", out var values).Should().BeTrue();
        var retryAfter = values!.First();
        int.TryParse(retryAfter, out var seconds).Should().BeTrue();
        seconds.Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────
    // D. Application-layer email throttle (unit tests - bypass DI)
    // ─────────────────────────────────────────────

    private static (ApplicationDbContext db, UserService sut, Mock<IEmailService> emailMock) CreateServiceForUnitTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        var db = new ApplicationDbContext(options, new FakeDateTime(), new FakeCurrentUserService());
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        var currentUserMock = new Mock<ICurrentUserService>();
        var auditMock = new Mock<IAuditService>();
        var emailMock = new Mock<IEmailService>();

        var emailOptions = new Mock<IOptions<EmailOptions>>();
        emailOptions.Setup(x => x.Value).Returns(new EmailOptions
        {
            Provider = "Console",
            FrontendBaseUrl = "http://localhost:3000"
        });

        var sut = new UserService(db, currentUserMock.Object, auditMock.Object, emailMock.Object, emailOptions.Object);
        return (db, sut, emailMock);
    }

    private static Institute CreateInstitute(ApplicationDbContext db)
    {
        var institute = new Institute
        {
            Id = Guid.NewGuid(),
            Name = "Test Institute",
            GRNumber = "GR" + Guid.NewGuid().ToString("N")[..6],
            IsActive = true
        };
        db.Institutes.Add(institute);
        db.SaveChanges();
        return institute;
    }

    [Fact]
    public async Task ForgotPassword_RecentTokenExists_DoesNotSendAnotherEmail()
    {
        var (db, sut, emailMock) = CreateServiceForUnitTest();
        try
        {
            var institute = CreateInstitute(db);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "throttle-user",
                Email = "throttle@test.com",
                FirstName = "Throttle",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
                IsActive = true,
                InstituteId = institute.Id
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = "existing-hash",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow.AddSeconds(-30),
                UsedAt = null,
                InstituteId = institute.Id
            };
            db.PasswordResetTokens.Add(token);
            await db.SaveChangesAsync();

            await sut.ForgotPasswordAsync("throttle@test.com", CancellationToken.None);

            emailMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never());
        }
        finally
        {
            db.Database.CloseConnection();
            db.Dispose();
        }
    }

    [Fact]
    public async Task ForgotPassword_ExpiredToken_AllowsNewEmail()
    {
        var (db, sut, emailMock) = CreateServiceForUnitTest();
        try
        {
            var institute = CreateInstitute(db);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "expired-user",
                Email = "expired@test.com",
                FirstName = "Expired",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
                IsActive = true,
                InstituteId = institute.Id
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = "expired-hash",
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                CreatedAt = DateTime.UtcNow.AddMinutes(-16),
                UsedAt = null,
                InstituteId = institute.Id
            };
            db.PasswordResetTokens.Add(token);
            await db.SaveChangesAsync();

            await sut.ForgotPasswordAsync("expired@test.com", CancellationToken.None);

            emailMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once());
        }
        finally
        {
            db.Database.CloseConnection();
            db.Dispose();
        }
    }

    [Fact]
    public async Task ForgotPassword_UsedToken_AllowsNewEmail()
    {
        var (db, sut, emailMock) = CreateServiceForUnitTest();
        try
        {
            var institute = CreateInstitute(db);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "used-user",
                Email = "used@test.com",
                FirstName = "Used",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
                IsActive = true,
                InstituteId = institute.Id
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = "used-hash",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow.AddSeconds(-30),
                UsedAt = DateTime.UtcNow.AddSeconds(-25),
                InstituteId = institute.Id
            };
            db.PasswordResetTokens.Add(token);
            await db.SaveChangesAsync();

            await sut.ForgotPasswordAsync("used@test.com", CancellationToken.None);

            emailMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once());
        }
        finally
        {
            db.Database.CloseConnection();
            db.Dispose();
        }
    }

    [Fact]
    public async Task ForgotPassword_NoExistingTokens_AllowsEmail()
    {
        var (db, sut, emailMock) = CreateServiceForUnitTest();
        try
        {
            var institute = CreateInstitute(db);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "fresh-user",
                Email = "fresh@test.com",
                FirstName = "Fresh",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
                IsActive = true,
                InstituteId = institute.Id
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            await sut.ForgotPasswordAsync("fresh@test.com", CancellationToken.None);

            emailMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once());
        }
        finally
        {
            db.Database.CloseConnection();
            db.Dispose();
        }
    }

    private class RateLimitErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}

// ─────────────────────────────────────────────
// Custom WebApplicationFactory
// ─────────────────────────────────────────────

public class RateLimitWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string JwtSecret = "TestSecretKeyForRateLimitTests1234567890!";
    private const string JwtIssuer = "TestIssuer";
    private const string JwtAudience = "TestAudience";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("JwtSettings:Secret", JwtSecret);
        builder.UseSetting("JwtSettings:Issuer", JwtIssuer);
        builder.UseSetting("JwtSettings:Audience", JwtAudience);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, MockEmailService>();

            services.RemoveAll<ICurrentUserService>();
            services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

            services.Configure<JwtSettings>(options =>
            {
                options.Secret = JwtSecret;
                options.Issuer = JwtIssuer;
                options.Audience = JwtAudience;
            });

            services.Configure<EmailOptions>(options =>
            {
                options.Provider = "Console";
                options.FrontendBaseUrl = "http://localhost:3000";
            });
        });
    }

    public static string GenerateTestJwt(Guid? userId = null)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString()),
            new Claim(ClaimTypes.Name, "testadmin"),
            new Claim("instituteId", Guid.NewGuid().ToString()),
            new Claim("role", "SuperAdmin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private class MockEmailService : IEmailService
    {
        public Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, string? instituteName, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}

internal class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
    public Guid? InstituteId => null;
    public Guid? AcademicSessionId => null;
    public Guid? TradeId => null;
    public Guid? BatchId => null;
    public string? UserName => null;
    public bool IsAuthenticated => false;
    public bool HasRole(string role) => false;
}

internal class FakeDateTime : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}

[CollectionDefinition("RateLimitTests", DisableParallelization = true)]
public class RateLimitTestsCollectionDefinition;

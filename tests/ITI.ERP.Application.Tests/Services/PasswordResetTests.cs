using FluentAssertions;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Configuration;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Application.Services;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Infrastructure.Persistence;
using ITI.ERP.Infrastructure.Services;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ITI.ERP.Application.Tests.Services;

public class PasswordResetTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IAuditService> _auditMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly UserService _sut;

    public PasswordResetTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _db = new ApplicationDbContext(options, new FakeDateTime(), new FakeCurrentUserService());
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        _currentUserMock = new Mock<ICurrentUserService>();
        _auditMock = new Mock<IAuditService>();
        _emailMock = new Mock<IEmailService>();

        var emailOptions = new Mock<IOptions<EmailOptions>>();
        emailOptions.Setup(x => x.Value).Returns(new EmailOptions
        {
            Provider = "Console",
            FrontendBaseUrl = "http://localhost:3000"
        });

        _sut = new UserService(_db, _currentUserMock.Object, _auditMock.Object, _emailMock.Object, emailOptions.Object);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    private User CreateUser(string email = "test@example.com", bool isActive = true)
    {
        var institute = new Institute
        {
            Id = Guid.NewGuid(),
            Name = "Test Institute",
            GRNumber = "GR" + Guid.NewGuid().ToString("N")[..6],
            IsActive = true
        };
        _db.Institutes.Add(institute);
        _db.SaveChanges();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = email,
            FirstName = "Test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass1!"),
            IsActive = isActive,
            InstituteId = institute.Id
        };
        _db.Users.Add(user);
        _db.SaveChanges();
        return user;
    }

    private void SetupSuperAdmin()
    {
        var institute = _db.Institutes.FirstOrDefault();
        if (institute is null)
        {
            institute = new Institute
            {
                Id = Guid.NewGuid(),
                Name = "System Institute",
                GRNumber = "GR00000",
                IsActive = true
            };
            _db.Institutes.Add(institute);
            _db.SaveChanges();
        }

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "superadmin",
            Email = "admin@system.com",
            FirstName = "Super",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1!"),
            IsActive = true,
            InstituteId = institute.Id
        };
        _db.Users.Add(adminUser);
        _db.SaveChanges();

        _currentUserMock.Setup(x => x.HasRole(RoleConstants.Admin)).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(adminUser.Id);
    }

    [Fact]
    public async Task ForgotPassword_ExistingUser_CreatesTokenAndSendsEmail()
    {
        var user = CreateUser();

        var result = await _sut.ForgotPasswordAsync("test@example.com", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _emailMock.Verify(x => x.SendPasswordResetEmailAsync(
            "test@example.com", "Test", It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_NonExistentEmail_ReturnsSuccess_NoEmailSent()
    {
        var result = await _sut.ForgotPasswordAsync("nonexistent@example.com", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _emailMock.Verify(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_InactiveUser_NoEmailSent()
    {
        CreateUser(isActive: false);

        var result = await _sut.ForgotPasswordAsync("test@example.com", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _emailMock.Verify(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_InvalidatesPreviousUnusedTokens()
    {
        var user = CreateUser();

        var oldToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("oldtoken"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _db.PasswordResetTokens.Add(oldToken);
        await _db.SaveChangesAsync();

        await _sut.ForgotPasswordAsync("test@example.com", CancellationToken.None);

        _db.ChangeTracker.Clear();
        var tokenInDb = await _db.PasswordResetTokens.AsNoTracking().FirstOrDefaultAsync(t => t.Id == oldToken.Id);
        tokenInDb!.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyResetToken_NonExistentToken_ReturnsFailure()
    {
        var result = await _sut.VerifyResetTokenAsync("nonexistent", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid or expired reset token.");
    }

    [Fact]
    public async Task VerifyResetToken_ExpiredToken_ReturnsFailure()
    {
        var user = CreateUser();
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("expiredtoken"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
        };
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        var result = await _sut.VerifyResetTokenAsync("expiredtoken", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyResetToken_UsedToken_ReturnsFailure()
    {
        var user = CreateUser();
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("usedtoken"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            UsedAt = DateTime.UtcNow
        };
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        var result = await _sut.VerifyResetTokenAsync("usedtoken", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyResetToken_ValidToken_ReturnsSuccess()
    {
        var user = CreateUser();
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("validtoken"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        var result = await _sut.VerifyResetTokenAsync("validtoken", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordWithToken_InvalidToken_ReturnsFailure()
    {
        var result = await _sut.ResetPasswordWithTokenAsync("invalid", "NewPass1!", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPasswordWithToken_ValidToken_UpdatesPasswordHash()
    {
        var user = CreateUser();
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("goodtoken"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        var oldHash = user.PasswordHash;

        var result = await _sut.ResetPasswordWithTokenAsync("goodtoken", "NewPass1!", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updatedUser = await _db.Users.FindAsync(user.Id);
        updatedUser!.PasswordHash.Should().NotBe(oldHash);
        BCrypt.Net.BCrypt.Verify("NewPass1!", updatedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordWithToken_MarksTokenAsUsed()
    {
        var user = CreateUser();
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("usetoken"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        await _sut.ResetPasswordWithTokenAsync("usetoken", "NewPass1!", CancellationToken.None);

        _db.ChangeTracker.Clear();
        var usedToken = await _db.PasswordResetTokens.AsNoTracking().FirstOrDefaultAsync(t => t.Id == token.Id);
        usedToken!.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ResetPasswordWithToken_InvalidatesRefreshTokens()
    {
        var user = CreateUser();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = "sometoken",
            JwtId = "jwt1",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        _db.RefreshTokens.Add(refreshToken);

        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("revoke"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        await _sut.ResetPasswordWithTokenAsync("revoke", "NewPass1!", CancellationToken.None);

        var rt = await _db.RefreshTokens.FindAsync(refreshToken.Id);
        rt!.IsActive.Should().BeFalse();
        rt.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ResetPasswordWithToken_ResetsLockout()
    {
        var user = CreateUser();
        user.IsLocked = true;
        user.FailedLoginAttempts = 5;
        user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        await _db.SaveChangesAsync();

        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("unlock"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        await _sut.ResetPasswordWithTokenAsync("unlock", "NewPass1!", CancellationToken.None);

        var updated = await _db.Users.FindAsync(user.Id);
        updated!.IsLocked.Should().BeFalse();
        updated.FailedLoginAttempts.Should().Be(0);
        updated.LockedUntil.Should().BeNull();
    }

    [Fact]
    public async Task ResetPasswordWithToken_CannotReuseToken()
    {
        var user = CreateUser();
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("singleuse"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        var firstResult = await _sut.ResetPasswordWithTokenAsync("singleuse", "NewPass1!", CancellationToken.None);
        firstResult.IsSuccess.Should().BeTrue();

        var secondResult = await _sut.ResetPasswordWithTokenAsync("singleuse", "NewPass2!", CancellationToken.None);
        secondResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SendPasswordReset_SuperAdmin_WorksWithAnyInstitute()
    {
        SetupSuperAdmin();
        var user = CreateUser();

        var result = await _sut.SendPasswordResetAsync(user.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _emailMock.Verify(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordReset_NoEmail_ReturnsFailure()
    {
        SetupSuperAdmin();
        var user = CreateUser();
        user.Email = null;
        await _db.SaveChangesAsync();

        var result = await _sut.SendPasswordResetAsync(user.Id, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("email");
    }

    [Fact]
    public async Task ForgotPassword_AuditLogsNeverContainPasswords()
    {
        var user = CreateUser();

        await _sut.ForgotPasswordAsync("test@example.com", CancellationToken.None);

        _auditMock.Verify(x => x.LogAsync(
            AuditAction.PasswordResetRequested,
            nameof(User),
            user.Id,
            It.IsAny<object?>(),
            It.IsAny<object?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetPasswordWithToken_CompletedAuditLogged()
    {
        var user = CreateUser();
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken("audit"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        await _sut.ResetPasswordWithTokenAsync("audit", "NewPass1!", CancellationToken.None);

        _auditMock.Verify(x => x.LogAsync(
            AuditAction.PasswordResetCompleted,
            nameof(User),
            user.Id,
            It.IsAny<object?>(),
            It.IsAny<object?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static string HashToken(string token)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private class FakeDateTime : IDateTime
    {
        public DateTime Now => DateTime.UtcNow;
    }

    private class FakeCurrentUserService : ICurrentUserService
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
}

using FluentAssertions;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Auth;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Application.Services;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Infrastructure.Persistence;
using ITI.ERP.Infrastructure.Services;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ITI.ERP.Application.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly AuthService _sut;

    private const int ConfiguredExpiryMinutes = 15;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _db = new ApplicationDbContext(options, new FakeDateTime(), new FakeCurrentUserService());
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        _currentUserMock = new Mock<ICurrentUserService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();

        _jwtTokenServiceMock.Setup(j => j.GenerateAccessToken(
                It.IsAny<User>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()))
            .Returns("fake.jwt.token");

        _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken())
            .Returns(() => Guid.NewGuid().ToString("N"));

        _jwtTokenServiceMock.Setup(j => j.GetAccessTokenExpiryInMinutes())
            .Returns(ConfiguredExpiryMinutes);

        _passwordHasherMock = new Mock<IPasswordHasher>();
        _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns((string pwd) => BCrypt.Net.BCrypt.HashPassword(pwd, 12));
        _passwordHasherMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string pwd, string hash) => BCrypt.Net.BCrypt.Verify(pwd, hash));
        _passwordHasherMock.Setup(p => p.IsRehashNeeded(It.IsAny<string>()))
            .Returns(false);

        _sut = new AuthService(_db, _currentUserMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    #region Helpers

    private (Institute institute, User user, Role role, AcademicSession session) CreateTestUser(
        string role = RoleConstants.InstituteAdmin,
        string username = "testuser",
        string password = "TestPass1!")
    {
        var institute = new Institute
        {
            Id = Guid.NewGuid(),
            Name = "Test Institute",
            GRNumber = "GR" + Guid.NewGuid().ToString("N")[..6],
            IsActive = true
        };
        _db.Institutes.Add(institute);

        var roleEntity = new Role
        {
            Id = Guid.NewGuid(),
            Name = role,
            Description = $"{role} role"
        };
        _db.Roles.Add(roleEntity);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = $"{username}@test.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            InstituteId = institute.Id
        };
        _db.Users.Add(user);

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = roleEntity.Id,
            InstituteId = institute.Id,
            IsActive = true
        };
        _db.UserRoles.Add(userRole);

        var session = new AcademicSession
        {
            Id = Guid.NewGuid(),
            InstituteId = institute.Id,
            SessionYear = "2026-27",
            StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2027, 3, 31),
            IsActive = true
        };
        _db.AcademicSessions.Add(session);

        _db.SaveChanges();

        return (institute, user, roleEntity, session);
    }

    #endregion

    #region Login - ExpiresAt

    [Fact]
    public async Task Login_ReturnsExpiresAt_MatchesJwtExpiry()
    {
        var (institute, user, _, _) = CreateTestUser();

        var request = new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = user.Username,
            Password = "TestPass1!"
        };

        var result = await _sut.LoginAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var (response, _) = result.Value!;
        var expectedExpiry = DateTime.UtcNow.AddMinutes(ConfiguredExpiryMinutes);
        response.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Login_ExpiresAt_DoesNotUseHardcoded60Minutes()
    {
        var (institute, user, _, _) = CreateTestUser();

        var request = new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = user.Username,
            Password = "TestPass1!"
        };

        var result = await _sut.LoginAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var (response, _) = result.Value!;
        var hardcodedExpiry = DateTime.UtcNow.AddMinutes(60);
        response.ExpiresAt.Should().NotBeCloseTo(hardcodedExpiry, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region RefreshToken - ExpiresAt and Rotation

    [Fact]
    public async Task RefreshToken_ReturnsExpiresAt_MatchesJwtExpiry()
    {
        var (institute, user, _, _) = CreateTestUser();

        var loginResult = await _sut.LoginAsync(new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = user.Username,
            Password = "TestPass1!"
        }, CancellationToken.None);

        var (_, loginRefreshToken) = loginResult.Value!;

        var refreshResult = await _sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = loginRefreshToken.RawToken
        }, CancellationToken.None);

        refreshResult.IsSuccess.Should().BeTrue();
        var (response, _) = refreshResult.Value!;
        var expectedExpiry = DateTime.UtcNow.AddMinutes(ConfiguredExpiryMinutes);
        response.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RefreshToken_RevokesOldToken()
    {
        var (institute, user, _, _) = CreateTestUser();

        var loginResult = await _sut.LoginAsync(new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = user.Username,
            Password = "TestPass1!"
        }, CancellationToken.None);

        var (_, loginRefreshToken) = loginResult.Value!;
        var oldToken = await _db.RefreshTokens
            .FirstAsync(rt => rt.TokenHash == loginRefreshToken.RawToken);

        await _sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = loginRefreshToken.RawToken
        }, CancellationToken.None);

        var refreshedOldToken = await _db.RefreshTokens
            .FirstAsync(rt => rt.Id == oldToken.Id);

        refreshedOldToken.IsActive.Should().BeFalse();
        refreshedOldToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshToken_NewTokenDiffersFromOld()
    {
        var (institute, user, _, _) = CreateTestUser();

        var loginResult = await _sut.LoginAsync(new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = user.Username,
            Password = "TestPass1!"
        }, CancellationToken.None);

        var (_, loginRefreshToken) = loginResult.Value!;

        var refreshResult = await _sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = loginRefreshToken.RawToken
        }, CancellationToken.None);

        var (_, newRefreshToken) = refreshResult.Value!;
        newRefreshToken.RawToken.Should().NotBe(loginRefreshToken.RawToken);
    }

    #endregion

    #region SwitchSession - ExpiresAt and Revocation

    [Fact]
    public async Task SwitchSession_ReturnsExpiresAt_MatchesJwtExpiry()
    {
        var (institute, user, _, session) = CreateTestUser();

        _currentUserMock.Setup(c => c.UserId).Returns(user.Id);

        var result = await _sut.SwitchSessionAsync(session.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var (response, _) = result.Value!;
        var expectedExpiry = DateTime.UtcNow.AddMinutes(ConfiguredExpiryMinutes);
        response.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SwitchSession_RevokesPreviousRefreshToken()
    {
        var (institute, user, _, session) = CreateTestUser();

        var loginResult = await _sut.LoginAsync(new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = user.Username,
            Password = "TestPass1!"
        }, CancellationToken.None);

        var (_, loginRefreshToken) = loginResult.Value!;
        var oldToken = await _db.RefreshTokens
            .FirstAsync(rt => rt.TokenHash == loginRefreshToken.RawToken);

        _currentUserMock.Setup(c => c.UserId).Returns(user.Id);

        await _sut.SwitchSessionAsync(session.Id, CancellationToken.None);

        var refreshedOldToken = await _db.RefreshTokens
            .FirstAsync(rt => rt.Id == oldToken.Id);

        refreshedOldToken.IsActive.Should().BeFalse();
        refreshedOldToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SwitchSession_NewRefreshToken_IsActive()
    {
        var (institute, user, _, session) = CreateTestUser();

        _currentUserMock.Setup(c => c.UserId).Returns(user.Id);

        var result = await _sut.SwitchSessionAsync(session.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var (_, refreshToken) = result.Value!;

        var dbToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshToken.RawToken);

        dbToken.Should().NotBeNull();
        dbToken!.IsActive.Should().BeTrue();
        dbToken.UserId.Should().Be(user.Id);
    }

    #endregion

    #region Logout

    [Fact]
    public async Task Logout_RevokesAllUserTokens()
    {
        var (institute, user, _, _) = CreateTestUser();

        await _sut.LoginAsync(new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = user.Username,
            Password = "TestPass1!"
        }, CancellationToken.None);

        await _sut.LoginAsync(new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = user.Username,
            Password = "TestPass1!"
        }, CancellationToken.None);

        var activeTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.IsActive)
            .ToListAsync();

        activeTokens.Should().HaveCount(2);

        await _sut.LogoutAsync(user.Id, CancellationToken.None);

        var tokensAfterLogout = await _db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.IsActive)
            .ToListAsync();

        tokensAfterLogout.Should().BeEmpty();
    }

    #endregion

    #region Login - Failure Cases

    [Fact]
    public async Task Login_InvalidPassword_ReturnsFailure()
    {
        var (institute, user, _, _) = CreateTestUser();

        var result = await _sut.LoginAsync(new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = user.Username,
            Password = "WrongPassword1!"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Incorrect email or password");
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsFailure()
    {
        var (institute, _, _, _) = CreateTestUser();

        var result = await _sut.LoginAsync(new LoginRequest
        {
            GRNumber = institute.GRNumber,
            Username = "nonexistent",
            Password = "TestPass1!"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region SwitchSession - Failure Cases

    [Fact]
    public async Task SwitchSession_InvalidSession_ReturnsFailure()
    {
        var (institute, user, _, _) = CreateTestUser();

        _currentUserMock.Setup(c => c.UserId).Returns(user.Id);

        var result = await _sut.SwitchSessionAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Request failed.");
    }

    [Fact]
    public async Task SwitchSession_Unauthenticated_ReturnsFailure()
    {
        _currentUserMock.Setup(c => c.UserId).Returns((Guid?)null);

        var result = await _sut.SwitchSessionAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("You are not authorized");
    }

    #endregion

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

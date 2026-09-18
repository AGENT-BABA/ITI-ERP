using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Application.Location;
using ITI.ERP.Application.Services;
using ITI.ERP.Infrastructure.Authentication;
using ITI.ERP.Infrastructure.Authorization;
using ITI.ERP.Application.Configuration;
using ITI.ERP.Infrastructure.Email;
using ITI.ERP.Infrastructure.FileStorage;
using ITI.ERP.Infrastructure.Persistence;
using ITI.ERP.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITI.ERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddScoped<ICalculationService, CalculationService>();
        services.AddScoped<ITradeAccessService, TradeAccessService>();

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        services.AddScoped<LocalFileStorageService>();
        services.AddScoped<IFileStorageService, FileStorageFactory>();

        services.AddScoped<IInstituteService, InstituteService>();
        services.AddScoped<IAcademicSessionService, AcademicSessionService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ITradeService, TradeService>();
        services.AddScoped<IPracticalService, PracticalService>();
        services.AddScoped<IYearlyPracticalService, YearlyPracticalService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();
        services.AddScoped<ITradeMasterService, TradeMasterService>();
        services.AddScoped<IStudentImportService, StudentImportService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IBatchService, BatchService>();

        services.AddTransient<ILocationService, LocationService>();

        services.Configure<EmailOptions>(configuration.GetSection("Email"));

        var emailProvider = configuration["Email:Provider"] ?? "Console";
        if (string.Equals(emailProvider, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, ConsoleEmailService>();
        }

        return services;
    }
}
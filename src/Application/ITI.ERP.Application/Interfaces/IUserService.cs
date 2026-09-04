using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.User;

namespace ITI.ERP.Application.Interfaces;

public interface IUserService
{
    Task<Result<PaginatedList<UserDto>>> GetUsersAsync(PaginationRequest request, CancellationToken ct);
    Task<Result<UserDto>> GetUserByIdAsync(Guid id, CancellationToken ct);
    Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request, CancellationToken ct);
    Task<Result<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct);
    Task<Result> ToggleUserStatusAsync(Guid id, CancellationToken ct);
    Task<Result> SendPasswordResetAsync(Guid userId, CancellationToken ct);
    Task<Result> UnlockUserAsync(Guid id, CancellationToken ct);
    Task<Result> DeleteUserAsync(Guid id, CancellationToken ct);
    Task<Result<List<TradeHeadDto>>> GetTradeHeadsByInstituteAsync(Guid instituteId, CancellationToken ct);
    Task<Result> ForgotPasswordAsync(string email, CancellationToken ct);
    Task<Result> VerifyResetTokenAsync(string token, CancellationToken ct);
    Task<Result> ResetPasswordWithTokenAsync(string token, string newPassword, CancellationToken ct);
}

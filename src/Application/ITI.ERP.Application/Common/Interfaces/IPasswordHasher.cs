namespace ITI.ERP.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    bool IsRehashNeeded(string hashedPassword);
}

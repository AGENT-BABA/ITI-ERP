using System.Security.Cryptography;
using System.Text;
using ITI.ERP.Application.Common.Interfaces;

namespace ITI.ERP.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    private const string Argon2Prefix = "$argon2id$";
    private const int BCryptWorkFactor = 12;

    private const int Argon2MemorySize = 65536; // 64 MB
    private const int Argon2Parallelism = 4;
    private const int Argon2Iterations = 3;
    private const int Argon2SaltSize = 16;
    private const int Argon2HashSize = 32;

    public string HashPassword(string password)
    {
        var salt = new byte[Argon2SaltSize];
        RandomNumberGenerator.Fill(salt);

        using var argon2 = new Konscious.Security.Cryptography.Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = Argon2Parallelism,
            MemorySize = Argon2MemorySize,
            Iterations = Argon2Iterations
        };

        var hash = argon2.GetBytes(Argon2HashSize);
        return FormatArgon2Hash(salt, hash);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (hashedPassword.StartsWith(Argon2Prefix))
            return VerifyArgon2(password, hashedPassword);

        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public bool IsRehashNeeded(string hashedPassword)
    {
        return !hashedPassword.StartsWith(Argon2Prefix);
    }

    private static bool VerifyArgon2(string password, string storedHash)
    {
        // Format: $argon2id$v=19$m=65536,t=3,p=4$<salt>$<hash>
        var parts = storedHash.Split('$');
        if (parts.Length < 6)
            return false;

        var salt = Convert.FromBase64String(parts[4]);
        var expectedHash = Convert.FromBase64String(parts[5]);

        // parts[2] = "v=19", parts[3] = "m=65536,t=3,p=4"
        var paramPairs = parts[3].Split(',');
        var memorySize = int.Parse(paramPairs[0].Split('=')[1]); // "m=65536" → 65536
        var iterations = int.Parse(paramPairs[1].Split('=')[1]);  // "t=3" → 3
        var parallelism = int.Parse(paramPairs[2].Split('=')[1]); // "p=4" → 4

        using var argon2 = new Konscious.Security.Cryptography.Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = parallelism,
            MemorySize = memorySize,
            Iterations = iterations
        };

        var computedHash = argon2.GetBytes(expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
    }

    private static string FormatArgon2Hash(byte[] salt, byte[] hash)
    {
        return $"{Argon2Prefix}v=19$m={Argon2MemorySize},t={Argon2Iterations},p={Argon2Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}

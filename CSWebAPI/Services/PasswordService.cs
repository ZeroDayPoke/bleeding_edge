// Services/PasswordService.cs
using System.Security.Cryptography;
using BCrypt.Net;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password, string salt)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, salt, false, HashType.SHA384);
    }

    public string GenerateSalt()
    {
        byte[] saltBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword, string salt)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

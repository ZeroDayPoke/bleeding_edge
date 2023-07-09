using System;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string? PasswordHash { get; set; }

    public string? Salt { get; set; }

    public string VerificationToken { get; set; } = Guid.NewGuid().ToString();

    public bool IsEmailVerified { get; set; }

    public List<UserRole>? UserRoles { get; set; }

    public void SetPassword(string password)
    {
        Salt = BCrypt.Net.BCrypt.GenerateSalt();
        PasswordHash = HashPassword(password, Salt);
    }

    public bool VerifyPassword(string password)
    {
        return PasswordHash != null && Salt != null && VerifyPassword(password, PasswordHash, Salt);
    }

    private string GenerateSalt()
    {
        byte[] saltBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    private string HashPassword(string password, string salt)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, salt, false, HashType.SHA384);
    }

    private bool VerifyPassword(string password, string hashedPassword, string salt)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public override string ToString()
    {
        return $"User: {Id}, {Username}, {Email}";
    }
}

public interface IPasswordService
{
    string HashPassword(string password, string salt);
    string GenerateSalt();
    bool VerifyPassword(string password, string hashedPassword, string salt);
}

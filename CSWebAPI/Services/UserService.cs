// Services/UserService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserAsync(int userId);
    Task<User?> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
    Task<bool> ChangeUserPasswordAsync(int id, ChangePasswordModel changePasswordModel);
    Task<string?> AuthenticateAsync(string username, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
}

public class UserService : IUserService
{
    private const int ITERATION_COUNT = 10000;
    private const int NUM_BYTES_REQUESTED = 256 / 8;
    private readonly MyDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IOptions<AppSettings> _appSettings;

    public UserService(MyDbContext context, IEmailService emailService, IOptions<AppSettings> appSettings)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users?.ToListAsync() ?? new List<User>();
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        if (_context.Users != null)
        {
            return await _context.Users.FindAsync(userId);
        }
        return null;
    }

    public async Task<User?> CreateUserAsync(User user)
    {
        if (_context.Users != null)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: user.PasswordHash ?? string.Empty,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            user.PasswordHash = hashedPassword;
            user.Salt = Convert.ToBase64String(salt);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (user.Email != null)
            {
                _emailService.SendVerificationEmail(user.Email, user.VerificationToken);
            }

            return user;
        }
        return null;
    }

    public async Task UpdateUserAsync(User user)
    {
        var existingUser = await _context.Users?.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existingUser != null)
        {
            user.PasswordHash = existingUser.PasswordHash;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ChangeUserPasswordAsync(int id, ChangePasswordModel changePasswordModel)
    {
        if (_context.Users == null)
        {
            return false;
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null || user.PasswordHash == null || user.Salt == null ||
            changePasswordModel.OldPassword == null || changePasswordModel.NewPassword == null)
        {
            return false;
        }

        var oldPasswordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: changePasswordModel.OldPassword,
            salt: Encoding.UTF8.GetBytes(user.Salt),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        if (oldPasswordHash != user.PasswordHash)
        {
            return false;
        }

        var newPasswordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: changePasswordModel.NewPassword,
            salt: Encoding.UTF8.GetBytes(user.Salt),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        user.PasswordHash = newPasswordHash;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<string?> AuthenticateAsync(string username, string password)
    {
        if (_context.Users != null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.Salt == null || Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Convert.FromBase64String(user.Salt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8)) != user.PasswordHash)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Value.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        return null;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        if (_context.Users != null)
        {
            return await _context.Users.FindAsync(id);
        }
        return null;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        if (_context.Users != null)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
        return null;
    }
}

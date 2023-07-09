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
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
    Task<bool> ChangeUserPasswordAsync(int id, ChangePasswordModel changePasswordModel);
    Task<string?> AuthenticateAsync(string username, string password);
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

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> CreateUserAsync(User user)
    {
        if (_context.Users != null)
        {
            // Generate a unique salt for the user
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password using PBKDF2
            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: user.PasswordHash,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            user.PasswordHash = hashedPassword;
            user.Salt = Convert.ToBase64String(salt);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send the verification email
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
        if (_context.Users != null)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser != null)
            {
                // Exclude the PasswordHash property from the update
                user.PasswordHash = existingUser.PasswordHash;
                user.Salt = existingUser.Salt;

                _context.Entry(existingUser).CurrentValues.SetValues(user);
                await _context.SaveChangesAsync();
            }
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
        var user = await _context.Users.FindAsync(id);


        if (user == null)
        {
            return false;
        }

        if (user.PasswordHash == null || changePasswordModel.OldPassword == null || changePasswordModel.NewPassword == null)
        {
            return false;
        }

        if (user.Salt == null)
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
        _context.Users?.Update(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public string Authenticate(string username, string password)
    {
        // Validate the user's credentials. This is a simplified example;
        // in a real application, you'd want to hash the password and compare
        // it to the stored hash.
        var user = _context.Users.SingleOrDefault(u => u.Username == username);
        if (user == null)
        {
            return null;
        }

        // Hash the provided password and compare it with the stored hash
        var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.UTF8.GetBytes(user.Salt),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        if (hashedPassword != user.PasswordHash)
        {
            return null;
        }

        // If the user's credentials are valid, generate a JWT.
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Value.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
    public async Task<string?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            throw new ArgumentException("Username does not exist.");
        }

        var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.UTF8.GetBytes(user.Salt),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        if (hashedPassword != user.PasswordHash)
        {
            throw new ArgumentException("Invalid password.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Value.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}

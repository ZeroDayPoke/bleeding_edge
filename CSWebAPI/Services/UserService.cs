using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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

    // Existing methods...

    public async Task AddFavoriteStrainAsync(int userId, int strainId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var strain = await _context.Strains.FindAsync(strainId);
        if (strain == null)
        {
            throw new Exception("Strain not found");
        }

        user.FavoriteStrains.Add(strain);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFavoriteStrainAsync(int userId, int strainId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var strain = await _context.Strains.FindAsync(strainId);
        if (strain == null)
        {
            throw new Exception("Strain not found");
        }

        user.FavoriteStrains.Remove(strain);
        await _context.SaveChangesAsync();
    }

    public async Task<List<int>> GetFavoriteStrainsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var favoriteStrainIds = new List<int>();
        foreach (var strain in user.FavoriteStrains)
        {
            favoriteStrainIds.Add(strain.Id);
        }

        return favoriteStrainIds;
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            throw new Exception("No user found with that email address");
        }

        var resetToken = Guid.NewGuid().ToString();
        user.ResetToken = resetToken;
        user.ResetTokenExpiration = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        _emailService.SendResetPasswordEmail(user.Email, resetToken);
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiration > DateTime.UtcNow);
        if (user == null)
        {
            throw new Exception("Invalid or expired reset token");
        }

        user.SetPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpiration = null;
        await _context.SaveChangesAsync();
    }

    public async Task RequestEmailVerificationAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            throw new Exception("No user found with that email address");
        }

        var verificationToken = Guid.NewGuid().ToString();
        user.VerificationToken = verificationToken;
        user.VerificationTokenExpiration = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        _emailService.SendVerificationEmail(user.Email, verificationToken);
    }

    public async Task VerifyEmailAsync(string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token && u.VerificationTokenExpiration > DateTime.UtcNow);
        if (user == null)
        {
            throw new Exception("Invalid or expired verification token");
        }

        user.IsVerified = true;
        user.VerificationToken = null;
        user.VerificationTokenExpiration = null;
        await _context.SaveChangesAsync();
    }
}

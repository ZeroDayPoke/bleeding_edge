using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string PasswordHash { get; set; }

    public string Salt { get; set; }

    public string VerificationToken { get; set; }

    public bool IsEmailVerified { get; set; }

    public List<UserRole> UserRoles { get; set; }

    public override string ToString()
    {
        return $"User: {Id}, {Username}, {Email}";
    }
}

public enum RoleName
{
    ADMIN,
    GUEST
}

public class Role
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Role name is required.")]
    public RoleName Name { get; set; }

    public List<UserRole> UserRoles { get; set; }

    public override string ToString()
    {
        return $"Role: {Id} - {Name}";
    }
}

public class UserRole
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; }
}

// Models/Password.cs
using System.ComponentModel.DataAnnotations;

public class ChangePasswordModel
{
    [Required]
    public string? OldPassword { get; set; }

    [Required]
    public string? NewPassword { get; set; }
}

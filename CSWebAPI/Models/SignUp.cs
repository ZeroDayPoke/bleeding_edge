using System.ComponentModel.DataAnnotations;

public class SignUpModel
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

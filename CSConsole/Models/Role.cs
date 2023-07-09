using System.ComponentModel.DataAnnotations;

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

    public List<UserRole>? UserRoles { get; set; }

    public override string ToString()
    {
        return $"Role: {Id} - {Name}";
    }
}

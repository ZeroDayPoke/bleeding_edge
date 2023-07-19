public class OperatingHour
{
    public string Day { get; set; }
    public string Open { get; set; }
    public string Close { get; set; }
}

public class Store
{
    public int Id { get; set; }
    public int ManagerId { get; set; }
    public User Manager { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public List<OperatingHour> OperatingHours { get; set; }
    public string ImageFilename { get; set; }
    public string Description { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Facebook { get; set; }
    public string Instagram { get; set; }
    public float? AverageRating { get; set; }
    public ICollection<Strain> Strains { get; set; }
    public ICollection<Review> Reviews { get; set; }
}

public enum Subtype
{
    Indica,
    Sativa,
    Hybrid,
    Unknown
}

public class Strain
{
    public int Id { get; set; }
    public int CultivatorId { get; set; }
    public User Cultivator { get; set; }
    public string TerpeneProfile { get; set; }
    public string ImageFilename { get; set; }
    public string Name { get; set; }
    public Subtype Subtype { get; set; }
    public float? ThcConcentration { get; set; }
    public float? CbdConcentration { get; set; }
    public ICollection<User> FavoritedByUsers { get; set; }
    public ICollection<Store> Stores { get; set; }
    public ICollection<Review> Reviews { get; set; }
}

public class Review
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int Rating { get; set; }

    // Navigation properties
    public int UserId { get; set; }
    public User User { get; set; }
    public int StoreId { get; set; }
    public Store Store { get; set; }
    public int StrainId { get; set; }
    public Strain Strain { get; set; }
}

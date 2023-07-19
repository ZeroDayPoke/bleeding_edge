using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<User>? Users { get; set; }
    public DbSet<Role>? Roles { get; set; }
    public DbSet<UserRole>? UserRoles { get; set; }
    public DbSet<Strain>? Strains { get; set; }
    public DbSet<Store>? Stores { get; set; }
    public DbSet<Review>? Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
            .Property(e => e.Name)
            .HasConversion<string>();

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        // Strain to Store relationship
        modelBuilder.Entity<Strain>()
            .HasMany(s => s.Stores)
            .WithMany(st => st.Strains)
            .UsingEntity(j => j.ToTable("StrainStore"));

        // Strain to Review relationship
        modelBuilder.Entity<Strain>()
            .HasMany(s => s.Reviews)
            .WithOne(r => r.Strain)
            .HasForeignKey(r => r.StrainId);

        // Store to Review relationship
        modelBuilder.Entity<Store>()
            .HasMany(s => s.Reviews)
            .WithOne(r => r.Store)
            .HasForeignKey(r => r.StoreId);

        // User to Strain relationship
        modelBuilder.Entity<User>()
            .HasMany(u => u.Strains)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId);
    }
}

public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        optionsBuilder.UseMySql(configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 33)));

        return new MyDbContext(optionsBuilder.Options);
    }
}

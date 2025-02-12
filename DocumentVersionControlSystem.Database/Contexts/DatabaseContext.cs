namespace DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class DatabaseContext : DbContext
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<Version> Versions { get; set; }

    private readonly string _connectionString;

    public DatabaseContext()
    {
        _connectionString = LoadConnectionString();
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        _connectionString = LoadConnectionString();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }

    private string LoadConnectionString()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        return config.GetConnectionString("DefaultConnection")
            ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DocVerControlDB;Integrated Security=True";
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>()
            .HasKey(d => d.Id);

        modelBuilder.Entity<Document>()
            .Property(d => d.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Document>()
            .Property(d => d.LastModifiedDate)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Document>()
            .Property(d => d.VersionCount)
            .HasDefaultValue(0);

        modelBuilder.Entity<Version>()
            .HasKey(v => v.Id);

        modelBuilder.Entity<Version>()
            .Property(v => v.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        base.OnModelCreating(modelBuilder);
    }
}

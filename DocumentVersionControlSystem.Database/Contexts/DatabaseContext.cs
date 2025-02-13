namespace DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

public class DatabaseContext : DbContext
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<Version> Versions { get; set; }

    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }
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

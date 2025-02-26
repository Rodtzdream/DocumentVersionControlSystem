namespace DocumentVersionControlSystem.Database.Contexts;

using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
            optionsBuilder.UseSqlite($"Data Source={AppPaths.DatabaseFilePath};");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>()
            .HasKey(d => d.Id);

        modelBuilder.Entity<Document>()
            .Property(d => d.CreationDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Document>()
            .Property(d => d.LastModifiedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Document>()
            .Property(d => d.VersionCount)
            .HasDefaultValue(0);

        modelBuilder.Entity<Version>()
            .HasKey(v => v.Id);

        modelBuilder.Entity<Version>()
            .Property(v => v.CreationDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        base.OnModelCreating(modelBuilder);
    }
}

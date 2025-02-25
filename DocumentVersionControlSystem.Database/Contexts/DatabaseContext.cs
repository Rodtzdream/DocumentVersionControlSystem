namespace DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
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
            string appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DocumentVersionControlSystem");
            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
            }
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(appFolderPath, "database.db")};");
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

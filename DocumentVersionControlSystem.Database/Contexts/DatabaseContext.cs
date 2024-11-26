using DocumentVersionControlSystem.Database.Models;
using Microsoft.EntityFrameworkCore;

public class DatabaseContext : DbContext
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentVersionControlSystem.Database.Models.Version> Versions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DocVerControlDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
        base.OnConfiguring(optionsBuilder);
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

        modelBuilder.Entity<DocumentVersionControlSystem.Database.Models.Version>()
            .HasKey(v => v.Id);

        modelBuilder.Entity<DocumentVersionControlSystem.Database.Models.Version>()
            .Property(v => v.CreationDate)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<DocumentVersionControlSystem.Database.Models.Version>()
            .HasOne(v => v.Document)
            .WithMany(d => d.Versions)
            .HasForeignKey(v => v.DocumentId);

        base.OnModelCreating(modelBuilder);
    }
}
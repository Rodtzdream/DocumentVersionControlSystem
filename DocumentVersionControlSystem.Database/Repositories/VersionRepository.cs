namespace DocumentVersionControlSystem.Database.Repositories;
using DocumentVersionControlSystem.Database.Contexts;
using Microsoft.EntityFrameworkCore;

public class VersionRepository
{
    private readonly DatabaseContext _context;

    public VersionRepository(DatabaseContext context)
    {
        _context = context;
    }

    public void AddVersion(Models.Document document, Models.Version version)
    {
        document.VersionCount++;
        _context.Documents.Update(document);
        _context.Versions.Add(version);
        _context.SaveChanges();
    }

    public Models.Version? GetVersionById(int id)
    {
        return _context.Versions.Find(id);
    }

    public List<Models.Version> GetAllVersions()
    {
        return _context.Versions.AsNoTracking().ToList();
    }

    public void DeleteVersion(Models.Version version)
    {
        _context.Versions.Remove(version);
        _context.SaveChanges();
    }

    public List<Models.Version> GetVersionsByDocumentId(int documentId)
    {
        return _context.Versions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.CreationDate)
            .AsNoTracking()
            .ToList();
    }

    public Models.Version GetLatestVersionByDocumentId(int documentId)
    {
        return _context.Versions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.CreationDate)
            .FirstOrDefault();
    }

    public void UpdateVersion(Models.Version version)
    {
        _context.Versions.Update(version);
        _context.SaveChanges();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}

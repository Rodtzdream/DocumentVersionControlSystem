namespace DocumentVersionControlSystem.Database.Repositories;
using DocumentVersionControlSystem.Database.Contexts;

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
        _context.Versions.Add(version);
        _context.SaveChanges();
    }

    public Models.Version GetVersionById(int id)
    {
        return _context.Versions.Find(id);
    }

    public List<Models.Version> GetAllVersions()
    {
        return _context.Versions.ToList();
    }

    public void DeleteVersion(Models.Version version)
    {
        _context.Versions.Remove(version);
        _context.SaveChanges();
    }

    public List<Models.Version> GetVersionsByDocumentId(int documentId)
    {
        var versions = _context.Versions.Where(v => v.DocumentId == documentId).ToList();
        return versions.OrderByDescending(v => v.CreationDate).ToList();
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
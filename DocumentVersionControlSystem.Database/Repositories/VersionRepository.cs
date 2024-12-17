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
        if (document.Versions == null)
        {
           document.Versions = new List<Models.Version>();
        }

        document.Versions.Add(version);
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
        return _context.Versions.Where(v => v.DocumentId == documentId).ToList();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
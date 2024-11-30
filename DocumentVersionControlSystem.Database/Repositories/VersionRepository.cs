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
        _context.Versions.Add(version);
        document.Versions.Add(version);
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

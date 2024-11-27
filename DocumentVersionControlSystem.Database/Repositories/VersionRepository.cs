namespace DocumentVersionControlSystem.Database.Repositories;

public class VersionRepository
{
    private readonly DatabaseContext _context;

    public VersionRepository(DatabaseContext context)
    {
        _context = context;
    }

    public void AddVersion(DocumentVersionControlSystem.Database.Models.Version version)
    {
        _context.Versions.Add(version);
    }

    public DocumentVersionControlSystem.Database.Models.Version GetVersionById(int id)
    {
        return _context.Versions.Find(id);
    }

    public List<DocumentVersionControlSystem.Database.Models.Version> GetAllVersions()
    {
        return _context.Versions.ToList();
    }

    public void DeleteVersion(DocumentVersionControlSystem.Database.Models.Version version)
    {
        _context.Versions.Remove(version);
    }

    public List<DocumentVersionControlSystem.Database.Models.Version> GetVersionsByDocumentId(int documentId)
    {
        return _context.Versions.Where(v => v.DocumentId == documentId).ToList();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}

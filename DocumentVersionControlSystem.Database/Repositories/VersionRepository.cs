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

    public Models.Version? GetVersionById(int id)
    {
        return _context.Versions.AsNoTracking().FirstOrDefault(v => v.Id == id);
    }

    public List<Models.Version> GetVersionsByDocumentId(int documentId)
    {
        return _context.Versions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.CreationDate)
            .AsNoTracking()
            .ToList();
    }

    public Models.Version? GetLatestVersionByDocumentId(int documentId)
    {
        return _context.Versions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.CreationDate)
            .AsNoTracking()
            .FirstOrDefault();
    }

    public void AddVersion(Models.Document document, Models.Version version)
    {
        document.VersionCount++;
        var trackedDocument = _context.Documents.Local.FirstOrDefault(d => d.Id == document.Id);
        if (trackedDocument != null)
        {
            _context.Entry(trackedDocument).State = EntityState.Detached;
        }
        _context.Entry(document).State = EntityState.Modified;
        _context.Versions.Add(version);
        _context.SaveChanges();
    }

    public void UpdateVersionDescription(Models.Version version, string newDescription)
    {
        var existingVersion = _context.Versions.Find(version.Id);
        if (existingVersion != null)
        {
            existingVersion.VersionDescription = newDescription;
            _context.SaveChanges();
        }
    }

    public void DeleteVersion(Models.Document document, Models.Version version)
    {
        var existingVersion = _context.Versions.Find(version.Id);
        if (existingVersion != null)
        {
            var existingDocument = _context.Documents.Local.FirstOrDefault(d => d.Id == document.Id);
            if (existingDocument != null)
            {
                _context.Entry(existingDocument).State = EntityState.Detached;
            }

            document.VersionCount--;
            _context.Entry(document).State = EntityState.Modified;
            _context.Versions.Remove(existingVersion);
            _context.SaveChanges();
        }
    }
}

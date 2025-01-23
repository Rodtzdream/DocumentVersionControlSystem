using DocumentVersionControlSystem.Database.Contexts;
namespace DocumentVersionControlSystem.Database.Repositories;

public class DocumentRepository
{
    private readonly DatabaseContext _context;
    private readonly object _lock = new object();

    public DocumentRepository(DatabaseContext context)
    {
        _context = context;
    }

    public void AddDocument(Models.Document document)
    {
        lock (_lock)
        {
            _context.Documents.Add(document);
            _context.SaveChanges();
        }
    }

    public Models.Document? GetDocumentById(int id)
    {
        lock (_lock)
        {
            return _context.Documents.Find(id);
        }
    }

    public List<Models.Document> GetAllDocuments()
    {
        lock (_lock)
        {
            return _context.Documents.ToList();
        }
    }

    public void UpdateDocument(Models.Document document)
    {
        lock (_lock)
        {
            _context.Documents.Update(document);
            _context.SaveChanges();
        }
    }

    public void DeleteDocument(Models.Document document)
    {
        lock (_lock)
        {
            _context.Documents.Remove(document);
            if(document.VersionCount > 0)
                _context.Versions.RemoveRange(_context.Versions.Where(v => v.DocumentId == document.Id));
        }
    }

    public List<Models.Document> GetDocumentsByName(string name)
    {
        lock (_lock)
        {
            return _context.Documents.Where(d => d.Name == name).ToList();
        }
    }

    public Models.Document? GetDocumentByPath(string path)
    {
        lock (_lock)
        {
            return _context.Documents.FirstOrDefault(d => d.FilePath == path);
        }
    }

    public void SaveChanges()
    {
        lock (_lock)
        {
            _context.SaveChanges();
        }
    }
}

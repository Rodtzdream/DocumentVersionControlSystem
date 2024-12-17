using DocumentVersionControlSystem.Database.Contexts;
namespace DocumentVersionControlSystem.Database.Repositories;

public class DocumentRepository
{
    private readonly DatabaseContext _context;

    public DocumentRepository(DatabaseContext context)
    {
        _context = context;
    }

    public void AddDocument(Models.Document document)
    {
        _context.Documents.Add(document);
        _context.SaveChanges();
    }

    public Models.Document GetDocumentById(int id)
    {
        return _context.Documents.Find(id);
    }

    public List<Models.Document> GetAllDocuments()
    {
        return _context.Documents.ToList();
    }

    public void UpdateDocument(Models.Document document)
    {
        _context.Documents.Update(document);
        _context.SaveChanges();
    }

    public void DeleteDocument(Models.Document document)
    {
        _context.Documents.Remove(document);
        _context.Versions.RemoveRange(_context.Versions.Where(v => v.DocumentId == document.Id));
        _context.SaveChanges();
    }

    public List<Models.Document> GetDocumentsByName(string name)
    {
        return _context.Documents.Where(d => d.Name == name).ToList();
    }

    public Models.Document GetDocumentByPath(string path)
    {
        return _context.Documents.FirstOrDefault(d => d.FilePath == path);
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
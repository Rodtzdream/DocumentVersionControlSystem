namespace DocumentVersionControlSystem.Database.Repositories;

public class DocumentRepository
{
    private readonly DatabaseContext _context;

    public DocumentRepository(DatabaseContext context)
    {
        _context = context;
    }

    public void AddDocument(DocumentVersionControlSystem.Database.Models.Document document)
    {
        _context.Documents.Add(document);
    }

    public DocumentVersionControlSystem.Database.Models.Document GetDocumentById(int id)
    {
        return _context.Documents.Find(id);
    }

    public List<DocumentVersionControlSystem.Database.Models.Document> GetAllDocuments()
    {
        return _context.Documents.ToList();
    }

    public void UpdateDocument(DocumentVersionControlSystem.Database.Models.Document document)
    {
        _context.Documents.Update(document);
    }

    public void DeleteDocument(DocumentVersionControlSystem.Database.Models.Document document)
    {
        _context.Documents.Remove(document);
    }

    public List<DocumentVersionControlSystem.Database.Models.Document> GetDocumentsByName(string name)
    {
        return _context.Documents.Where(d => d.Name == name).ToList();
    }

    public DocumentVersionControlSystem.Database.Models.Document GetDocumentByPath(string path)
    {
        return _context.Documents.FirstOrDefault(d => d.FilePath == path);
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}

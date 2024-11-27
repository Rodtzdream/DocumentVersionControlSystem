namespace DocumentVersionControlSystem.DocumentManagement;

public class DocumentManager
{
    private readonly DocumentVersionControlSystem.Database.Repositories.DocumentRepository _documentRepository;
    private readonly DocumentVersionControlSystem.Database.Repositories.VersionRepository _versionRepository;
    private readonly DocumentVersionControlSystem.DiffManager.DiffManager _diffManager;

    public DocumentManager(DocumentVersionControlSystem.Database.Repositories.DocumentRepository documentRepository, DocumentVersionControlSystem.Database.Repositories.VersionRepository versionRepository, DocumentVersionControlSystem.DiffManager.DiffManager diffManager)
    {
        _documentRepository = documentRepository;
        _versionRepository = versionRepository;
        _diffManager = diffManager;
    }

    public void AddDocument(DocumentVersionControlSystem.Database.Models.Document document)
    {
        _documentRepository.AddDocument(document);
        _documentRepository.SaveChanges();
    }

    public void UpdateDocument(DocumentVersionControlSystem.Database.Models.Document document)
    {
        _documentRepository.UpdateDocument(document);
        _documentRepository.SaveChanges();
    }

    public void DeleteDocument(DocumentVersionControlSystem.Database.Models.Document document)
    {
        _documentRepository.DeleteDocument(document);
        _documentRepository.SaveChanges();
    }

    public DocumentVersionControlSystem.Database.Models.Document GetDocumentById(int id)
    {
        return _documentRepository.GetDocumentById(id);
    }

    public List<DocumentVersionControlSystem.Database.Models.Document> GetAllDocuments()
    {
        return _documentRepository.GetAllDocuments();
    }

    public List<DocumentVersionControlSystem.Database.Models.Document> GetDocumentsByName(string name)
    {
        return _documentRepository.GetDocumentsByName(name);
    }

    public void AddVersionToDocument(int documentId, string versionDescription, string FilePath)
    {
        var document = _documentRepository.GetDocumentById(documentId);
        var version = new DocumentVersionControlSystem.Database.Models.Version
        {
            DocumentId = documentId,
            Document = document,
            VersionDescription = versionDescription,
            FilePath = FilePath,
            CreationDate = DateTime.Now
        };
        _versionRepository.AddVersion(version);
        _versionRepository.SaveChanges();
    }

    public void AddDocument(string name, string FilePath)
    {
        var document = new DocumentVersionControlSystem.Database.Models.Document
        {
            Name = name,
            CreationDate = DateTime.Now,
            LastModifiedDate = DateTime.Now,
            FilePath = FilePath
        };
        _documentRepository.AddDocument(document);
        _documentRepository.SaveChanges();
    }

    public void UpdateDocument(int documentId, string name)
    {
        var document = _documentRepository.GetDocumentById(documentId);
        document.Name = name;
        document.LastModifiedDate = DateTime.Now;
        _documentRepository.UpdateDocument(document);
        _documentRepository.SaveChanges();
    }

    public void DeleteDocument(int documentId)
    {
        var document = _documentRepository.GetDocumentById(documentId);
        _documentRepository.DeleteDocument(document);
        _documentRepository.SaveChanges();
    }
}

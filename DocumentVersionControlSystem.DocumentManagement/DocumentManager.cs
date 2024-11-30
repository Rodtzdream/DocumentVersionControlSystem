﻿namespace DocumentVersionControlSystem.DocumentManagement;

public class DocumentManager
{
    private readonly Database.Repositories.DocumentRepository _documentRepository;
    private readonly Database.Repositories.VersionRepository _versionRepository;
    private readonly DiffManager.DiffManager _diffManager;
    private readonly Logging.Logger _logger;
    private readonly List<FileChangeWatcher> _fileWatchers = new();

    public DocumentManager(Database.Repositories.DocumentRepository documentRepository, Database.Repositories.VersionRepository versionRepository, DiffManager.DiffManager diffManager, Logging.Logger logger)
    {
        _documentRepository = documentRepository;
        _versionRepository = versionRepository;
        _diffManager = diffManager;
        _logger = logger;
    }

    public void InitializeFileWatchers()
    {
        var documents = _documentRepository.GetAllDocuments();
        foreach (var document in documents)
        {
            var watcher = new FileChangeWatcher(
                document.FilePath,
                (oldPath, newPath) => HandleFileRenamedOrMoved(oldPath, newPath),
                (path) => HandleFileDeleted(path)
            );

            _fileWatchers.Add(watcher);
        }
    }

    private void HandleFileRenamedOrMoved(string oldPath, string newPath)
    {
        var document = _documentRepository.GetDocumentByPath(oldPath);
        if (document != null)
        {
            document.FilePath = newPath;
            document.Name = Path.GetFileName(newPath);

            _documentRepository.UpdateDocument(document);
            _documentRepository.SaveChanges();
        }
    }

    private void HandleFileDeleted(string path)
    {
        var document = _documentRepository.GetDocumentByPath(path);
        if (document != null)
        {
            _documentRepository.DeleteDocument(document);
            _documentRepository.SaveChanges();
        }
    }

    public void StopAllWatchers()
    {
        foreach (var watcher in _fileWatchers)
            watcher.Stop();
        _fileWatchers.Clear();
    }

    public void AddDocument(Database.Models.Document document)
    {
        _documentRepository.AddDocument(document);
        _documentRepository.SaveChanges();
        _logger.LogInformation($"Document {document.Id} added");
    }

    public void UpdateDocument(Database.Models.Document document)
    {
        _documentRepository.UpdateDocument(document);
        _documentRepository.SaveChanges();
        _logger.LogInformation($"Document {document.Id} updated");
    }

    public void DeleteDocument(Database.Models.Document document)
    {
        _documentRepository.DeleteDocument(document);
        _documentRepository.SaveChanges();
        _logger.LogInformation($"Document {document.Id} deleted");
    }

    public Database.Models.Document GetDocumentById(int id)
    {
        return _documentRepository.GetDocumentById(id);
    }

    public List<Database.Models.Document> GetAllDocuments()
    {
        return _documentRepository.GetAllDocuments();
    }

    public List<Database.Models.Document> GetDocumentsByName(string name)
    {
        return _documentRepository.GetDocumentsByName(name);
    }

    public void AddVersionToDocument(int documentId, string versionDescription, string FilePath)
    {
        var document = _documentRepository.GetDocumentById(documentId);
        var version = new Database.Models.Version
        {
            DocumentId = documentId,
            Document = document,
            VersionDescription = versionDescription,
            FilePath = FilePath,
            CreationDate = DateTime.Now
        };
        _versionRepository.AddVersion(document, version);
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {version.Id} added to document {document.Id}");
    }

    public void AddDocument(string name, string FilePath)
    {
        var document = new Database.Models.Document
        {
            Name = name,
            CreationDate = DateTime.Now,
            LastModifiedDate = DateTime.Now,
            FilePath = FilePath
        };
        _documentRepository.AddDocument(document);
        _documentRepository.SaveChanges();
        _logger.LogInformation($"Document {document.Id} added");
    }

    public void UpdateDocument(int documentId, string name)
    {
        var document = _documentRepository.GetDocumentById(documentId);
        document.Name = name;
        document.LastModifiedDate = DateTime.Now;
        _documentRepository.UpdateDocument(document);
        _documentRepository.SaveChanges();
        _logger.LogInformation($"Document {document.Id} updated");
    }

    public void DeleteDocument(int documentId)
    {
        var document = _documentRepository.GetDocumentById(documentId);
        _documentRepository.DeleteDocument(document);
        _documentRepository.SaveChanges();
        _logger.LogInformation($"Document {document.Id} deleted");
    }
}

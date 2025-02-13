namespace DocumentVersionControlSystem.DocumentManagement;

using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;

public class DocumentManager
{
    private readonly Database.Repositories.DocumentRepository _documentRepository;
    private readonly Logging.Logger _logger;
    private readonly List<FileChangeWatcher> _fileWatchers;
    private bool _isFileInternallyRenamed;

    public DocumentManager(Logging.Logger logger, DatabaseContext databaseContext)
    {
        _fileWatchers = new List<FileChangeWatcher>();
        _documentRepository = new Database.Repositories.DocumentRepository(databaseContext);
        _logger = logger;
        _isFileInternallyRenamed = false;
    }

    public void InitializeFileWatchers()
    {
        var documents = _documentRepository.GetAllDocuments();
        foreach (var document in documents)
        {
            var watcher = new FileChangeWatcher(
                document.FilePath,
                HandleFileRenamedOrMoved,
                HandleFileDeleted
            );

            _fileWatchers.Add(watcher);
        }
    }

    private void HandleFileRenamedOrMoved(string oldPath, string newPath)
    {
        if (_isFileInternallyRenamed)
            return;

        _isFileInternallyRenamed = true;

        var document = _documentRepository.GetDocumentByPath(oldPath);
        if (document != null)
        {
            string newName = Path.GetFileNameWithoutExtension(newPath);

            Directory.Move(Path.Combine("Documents", document.Name), Path.Combine("Documents", newName));

            document.FilePath = newPath;
            document.Name = newName;
            document.LastModifiedDate = DateTime.Now;

            _documentRepository.UpdateDocument(document);
            _logger.LogInformation($"Document {document.Id} renamed or moved");
        }

        _isFileInternallyRenamed = false;
    }

    private void HandleFileDeleted(string path)
    {
        var document = _documentRepository.GetDocumentByPath(path);
        if (document != null)
        {
            _documentRepository.DeleteDocument(document);
            _logger.LogInformation($"Document {document.Id} deleted");
        }
    }

    public void StopAllWatchers()
    {
        foreach (var watcher in _fileWatchers)
            watcher.Stop();
        _fileWatchers.Clear();
    }

    public List<Document> GetAllDocuments()
    {
        return _documentRepository.GetAllDocuments();
    }

    public List<Document> GetDocumentsByName(string name)
    {
        return _documentRepository.GetDocumentsByName(name);
    }

    public bool AddDocument(string filePath)
    {
        if (GetDocumentsByName(Path.GetFileNameWithoutExtension(filePath)).Count != 0)
        {
            _logger.LogError($"Document {filePath} already exists");
            return false;
        }

        var fileInfo = new FileInfo(filePath);

        var document = new Document
        {
            Name = Path.GetFileNameWithoutExtension(fileInfo.Name),
            FilePath = filePath,
            CreationDate = fileInfo.CreationTime,
            LastModifiedDate = fileInfo.LastWriteTime
        };

        var documentDirectory = Path.Combine("Documents", document.Name);
        if (!Directory.Exists(documentDirectory))
        {
            Directory.CreateDirectory(documentDirectory);
        }

        _documentRepository.AddDocument(document);
        _logger.LogInformation($"Document {document.Id} added");
        return true;
    }

    public void RenameDocument(Document document, string newName)
    {
        _isFileInternallyRenamed = true;

        try
        {
            var oldFilePath = document.FilePath;
            var fileDirectory = Path.GetDirectoryName(oldFilePath) ?? string.Empty;
            var newFilePath = Path.Combine(fileDirectory, newName) + ".txt";

            var currentDocumentPath = Path.Combine("Documents", document.Name);
            var newDocumentPath = Path.Combine("Documents", newName);

            if (!File.Exists(oldFilePath))
            {
                _logger.LogError($"RenameDocument: File {oldFilePath} does not exist.");
                return;
            }

            if (File.Exists(newFilePath))
            {
                _logger.LogError($"RenameDocument: File {newFilePath} already exists.");
                return;
            }

            File.Move(oldFilePath, newFilePath);

            Directory.Move(currentDocumentPath, newDocumentPath);

            document.FilePath = newFilePath;
            document.Name = newName;
            document.LastModifiedDate = DateTime.Now;

            _documentRepository.UpdateDocument(document);

            _logger.LogInformation($"Document {document.Id} renamed successfully to {newName}.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"RenameDocument: Failed to rename document {document.Id}. Error: {ex.Message}");
        }

        _isFileInternallyRenamed = false;
    }

    public void DeleteDocument(Document document)
    {
        _documentRepository.DeleteDocument(document);

        var documentDirectory = Path.Combine("Documents", document.Name);
        if (Directory.Exists(documentDirectory))
        {
            Directory.Delete(documentDirectory, true);
        }

        _logger.LogInformation($"Document {document.Id} deleted");
    }
}

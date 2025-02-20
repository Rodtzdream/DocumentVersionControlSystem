namespace DocumentVersionControlSystem.DocumentManagement;

using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;

public class DocumentManager
{
    private readonly Database.Repositories.DocumentRepository _documentRepository;
    private readonly Logging.Logger _logger;
    private readonly List<FileChangeWatcher> _fileWatchers;
    private bool _isFileInternallyRenamed;
    private bool _isFileExternallyDeleted;

    public DocumentManager(Logging.Logger logger, DatabaseContext databaseContext)
    {
        _fileWatchers = new List<FileChangeWatcher>();
        _documentRepository = new Database.Repositories.DocumentRepository(databaseContext);
        _logger = logger;

        _isFileInternallyRenamed = false;
        _isFileExternallyDeleted = false;
    }

    public void InitializeFileWatchers()
    {
        var documents = _documentRepository.GetAllDocuments();
        foreach (var document in documents)
        {
            var watcher = new FileChangeWatcher(
                document.FilePath,
                HandleFileRenamed,
                HandleFileDeleted
            );

            _fileWatchers.Add(watcher);
        }
    }

    private void HandleFileRenamed(string oldPath, string newPath)
    {
        if (_isFileInternallyRenamed)
            return;

        try
        {
            _isFileInternallyRenamed = true;
            RecoverDocument(oldPath, newPath, true);
        }
        finally
        {
            _isFileInternallyRenamed = false;
        }
    }

    private void HandleFileDeleted(string path)
    {
        _isFileExternallyDeleted = true;
    }

    public void StopAllWatchers()
    {
        foreach (var watcher in _fileWatchers)
            watcher.Stop();
        _fileWatchers.Clear();
    }

    public bool IsFileExternallyDeleted()
    {
        return _isFileExternallyDeleted;
    }

    public void SetFileExternallyDeleted(bool value)
    {
        _isFileExternallyDeleted = value;
    }

    public List<Document> GetAllDocuments()
    {
        return _documentRepository.GetAllDocuments();
    }

    public List<Document> GetDocumentsByName(string name)
    {
        return _documentRepository.GetDocumentsByName(name);
    }

    public List<Document> VerifyDocumentsIntegrity()
    {
        var documents = _documentRepository.GetAllDocuments();
        var nonExistentDocuments = new List<Document>();
        foreach (var document in documents)
        {
            if (!File.Exists(document.FilePath))
            {
                nonExistentDocuments.Add(document);
            }
        }
        return nonExistentDocuments;
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
            var newFilePath = Path.Combine(fileDirectory, newName + ".txt");

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

            _documentRepository.RenameDocument(document, newFilePath);

            _logger.LogInformation($"Document {document.Id} renamed successfully to {newName}.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"RenameDocument: Failed to rename document {document.Id}. Error: {ex.Message}");
        }
        finally
        {
            _isFileInternallyRenamed = false;
        }
    }

    public void RecoverDocument(string oldFilePath, string newFilePath, bool isRenamed = false)
    {
        var document = _documentRepository.GetDocumentByPath(oldFilePath);
        if (document == null)
        {
            _logger.LogError($"RecoverDocument: Document with path {oldFilePath} does not exist.");
            return;
        }

        var newName = Path.GetFileNameWithoutExtension(newFilePath);

        if (document.Name != newName)
        {
            try
            {
                Directory.Move(Path.Combine("Documents", document.Name), Path.Combine("Documents", newName));
            }
            catch (Exception ex)
            {
                _logger.LogError($"RecoverDocument: Failed to move directory. Error: {ex.Message}");
                return;
            }
        }

        document.FilePath = newFilePath;
        document.Name = newName;
        document.LastModifiedDate = DateTime.Now;

        _documentRepository.UpdateDocument(document);
        _logger.LogInformation(isRenamed
            ? $"Document {document.Id} renamed."
            : $"Document {document.Id} recovered successfully.");
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

    public void DeleteDocument(string filePath)
    {
        var document = _documentRepository.GetDocumentByPath(filePath);
        if (document == null)
        {
            _logger.LogError($"DeleteDocument: Document with path {filePath} does not exist.");
            return;
        }
        DeleteDocument(document);
    }
}

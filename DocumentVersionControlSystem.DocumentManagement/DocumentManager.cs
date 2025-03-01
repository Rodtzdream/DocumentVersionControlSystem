namespace DocumentVersionControlSystem.DocumentManagement;

using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.FileStorage;

public class DocumentManager
{
    private readonly Database.Repositories.DocumentRepository _documentRepository;
    private readonly IFileStorageManager _fileStorageManager;
    private readonly Logging.Logger _logger;
    private readonly List<FileChangeWatcher> _fileWatchers;
    private readonly string _appFolderPath;
    private bool _isFileInternallyRenamed;
    private bool _isFileExternallyDeleted;

    // Constructor
    public DocumentManager(string appFolderPath, DatabaseContext databaseContext, IFileStorageManager fileStorageManager, Logging.Logger logger)
    {
        _appFolderPath = appFolderPath;
        _documentRepository = new Database.Repositories.DocumentRepository(databaseContext);
        _fileStorageManager = fileStorageManager;
        _logger = logger;
        _fileWatchers = new List<FileChangeWatcher>();

        _isFileInternallyRenamed = false;
        _isFileExternallyDeleted = false;
    }

    // Methods for initializing and stopping file watchers
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

    public void StopAllWatchers()
    {
        foreach (var watcher in _fileWatchers)
            watcher.Stop();
        _fileWatchers.Clear();
    }

    // Methods for handling file changes
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

    // Methods for getting and setting file change flags
    public bool IsFileExternallyDeleted()
    {
        return _isFileExternallyDeleted;
    }

    public void SetFileExternallyDeleted(bool value)
    {
        _isFileExternallyDeleted = value;
    }

    // Methods for getting data
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
            if (!_fileStorageManager.FileExists(document.FilePath))
            {
                nonExistentDocuments.Add(document);
            }
        }
        return nonExistentDocuments;
    }

    // Methods for adding, renaming documents
    public bool AddDocument(string filePath)
    {
        if (GetDocumentsByName(Path.GetFileNameWithoutExtension(filePath)).Count != 0)
        {
            _logger.LogWarning($"Document {filePath} already exists");
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

        string documentDirectory = Path.Combine(_appFolderPath, "Documents", document.Name);
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

            var currentDocumentPath = Path.Combine(_appFolderPath, "Documents", document.Name);
            var newDocumentPath = Path.Combine(_appFolderPath, "Documents", newName);

            if (!_fileStorageManager.FileExists(oldFilePath))
            {
                _logger.LogWarning($"RenameDocument: File {oldFilePath} does not exist.");
                return;
            }

            if (_fileStorageManager.FileExists(newFilePath))
            {
                _logger.LogWarning($"RenameDocument: File {newFilePath} already exists.");
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
            _logger.LogWarning($"RecoverDocument: Document with path {oldFilePath} does not exist.");
            return;
        }

        var newName = Path.GetFileNameWithoutExtension(newFilePath);

        if (document.Name != newName)
        {
            try
            {
                if (!Directory.Exists(Path.Combine(_appFolderPath, "Documents", newName)))
                {
                    Directory.Move(Path.Combine(_appFolderPath, "Documents", document.Name), Path.Combine(_appFolderPath, "Documents", newName));
                }
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

    // Methods for deleting documents
    public void DeleteDocument(Document document)
    {
        _documentRepository.DeleteDocument(document);

        var documentDirectory = Path.Combine(_appFolderPath, "Documents", document.Name);
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
            _logger.LogWarning($"DeleteDocument: Document with path {filePath} does not exist.");
            return;
        }
        DeleteDocument(document);
    }
}

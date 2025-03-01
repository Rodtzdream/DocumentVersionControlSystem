namespace DocumentVersionControlSystem.VersionControl;

using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.Database.Repositories;
using DocumentVersionControlSystem.DiffManager;
using DocumentVersionControlSystem.FileStorage;

public class VersionControlManager
{
    private readonly VersionRepository _versionRepository;
    private readonly DocumentRepository _documentRepository;
    private readonly IDiffManager _diffManager;
    private readonly IFileStorageManager _fileStorageManager;
    private readonly Logging.Logger _logger;
    private readonly string _appFolderPath;

    // Constructor
    public VersionControlManager(string appFolderPath, Logging.Logger logger, IFileStorageManager fileStorageManager, IDiffManager diffManager, DatabaseContext databaseContext)
    {
        _appFolderPath = appFolderPath;
        _logger = logger;
        _versionRepository = new VersionRepository(databaseContext);
        _documentRepository = new DocumentRepository(databaseContext);
        _fileStorageManager = fileStorageManager;
        _diffManager = diffManager;
    }

    // Methods for getting data
    public Version GetVersionById(int id)
    {
        return _versionRepository.GetVersionById(id);
    }

    public List<Version> GetVersionsByDocumentId(int documentId)
    {
        return _versionRepository.GetVersionsByDocumentId(documentId);
    }

    // Methods for adding, updating data
    public bool CreateNewVersion(Document document, string versionDescription)
    {
        var latestVersion = _versionRepository.GetLatestVersionByDocumentId(document.Id);

        if (document.VersionCount > 0 && !_diffManager.IsFileChanged(document.FilePath, latestVersion.FilePath))
        {
            return false;
        }

        var documentDirectory = Path.Combine(_appFolderPath, "Documents", document.Name);
        string versionFilePath = Path.Combine(documentDirectory, $"{DateTime.Now:yyyyMMddHHmmss}.txt");

        Version version = new()
        {
            DocumentId = document.Id,
            VersionDescription = versionDescription,
            FilePath = versionFilePath,
            CreationDate = DateTime.Now
        };

        _versionRepository.AddVersion(document, version);
        _fileStorageManager.CopyFile(document.FilePath, versionFilePath);
        _logger.LogInformation($"New version {version.Id} created for document {document.Id}");

        return true;
    }

    public bool SwitchToVersionAndDeleteNewer(int documentId, int versionId, string description)
    {
        var version = GetVersionById(versionId);
        var document = _documentRepository.GetDocumentById(documentId) ?? throw new InvalidOperationException($"Document with ID {documentId} not found.");

        if (document.VersionCount == 1)
        {
            return false;
        }

        _fileStorageManager.CopyFile(version.FilePath, document.FilePath);

        var versionsToDelete = _versionRepository.GetVersionsByDocumentId(document.Id)
            .Where(v => v.CreationDate > version.CreationDate)
            .ToList();

        foreach (var v in versionsToDelete)
        {
            _versionRepository.DeleteVersion(document, v);
            _fileStorageManager.DeleteFile(v.FilePath);
        }

        _documentRepository.UpdateDocumentLastModifiedDate(document, DateTime.Now);
        _versionRepository.UpdateVersionDescription(version, description);
        _logger.LogInformation($"Switched to version {version.Id} for document {document.Id}");

        return true;
    }

    public Version SwitchToVersionAndSaveAsLatest(int documentId, int versionId, string description)
    {
        var version = GetVersionById(versionId);
        var document = _documentRepository.GetDocumentById(documentId) ?? throw new InvalidOperationException($"Document with ID {documentId} not found.");

        if (!_diffManager.IsFileChanged(document.FilePath, version.FilePath))
        {
            _logger.LogError($"Version {versionId} is the same as the latest version.");
            return version;
        }

        var documentDirectory = Path.Combine(_appFolderPath, "Documents", document.Name);
        string versionFilePath = Path.Combine(documentDirectory, $"{DateTime.Now:yyyyMMddHHmmss}.txt");

        _fileStorageManager.CopyFile(version.FilePath, document.FilePath);

        var newVersion = new Version
        {
            DocumentId = document.Id,
            VersionDescription = description,
            FilePath = versionFilePath,
            CreationDate = DateTime.Now
        };

        _versionRepository.AddVersion(document, newVersion);
        _fileStorageManager.CopyFile(document.FilePath, versionFilePath);
        _documentRepository.UpdateDocumentLastModifiedDate(document, DateTime.Now);
        _logger.LogInformation($"Switched to version {version.Id} and saved as latest for document {document.Id}");

        return newVersion;
    }

    public void ChangeVersionDescription(int versionId, string newDescription)
    {
        _versionRepository.UpdateVersionDescription(GetVersionById(versionId), newDescription);
        _logger.LogInformation($"Version {versionId} description changed to '{newDescription}'");
    }

    // Method for deleting data
    public void DeleteVersion(int versionId)
    {
        var version = GetVersionById(versionId);
        var document = _documentRepository.GetDocumentById(version.DocumentId) ?? throw new InvalidOperationException($"Document with ID {version.DocumentId} not found.");

        _versionRepository.DeleteVersion(document, version);
        _fileStorageManager.DeleteFile(version.FilePath);
        _logger.LogInformation($"Version {version.Id} deleted");
    }
}

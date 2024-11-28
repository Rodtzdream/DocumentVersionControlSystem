namespace DocumentVersionControlSystem.VersionControl;

public class VersionControlManager
{
    private readonly DocumentVersionControlSystem.Database.Repositories.VersionRepository _versionRepository;
    private readonly FileStorage.FileStorageManager _fileStorageManager;
    private readonly DiffManager.DiffManager _diffManager;
    private readonly Logging.Logger _logger;

    public VersionControlManager(DocumentVersionControlSystem.Database.Repositories.VersionRepository versionRepository, FileStorage.FileStorageManager fileStorageManager, DiffManager.DiffManager diffManager, Logging.Logger logger)
    {
        _versionRepository = versionRepository;
        _fileStorageManager = fileStorageManager;
        _diffManager = diffManager;
        _logger = logger;
    }

    public void AddVersion(DocumentVersionControlSystem.Database.Models.Version version)
    {
        _versionRepository.AddVersion(version);
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {version.Id} added");
    }

    public void DeleteVersion(DocumentVersionControlSystem.Database.Models.Version version)
    {
        _versionRepository.DeleteVersion(version);
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {version.Id} deleted");
    }

    public DocumentVersionControlSystem.Database.Models.Version GetVersionById(int id)
    {
        return _versionRepository.GetVersionById(id);
    }

    public List<DocumentVersionControlSystem.Database.Models.Version> GetAllVersions()
    {
        return _versionRepository.GetAllVersions();
    }

    public List<DocumentVersionControlSystem.Database.Models.Version> GetVersionsByDocumentId(int documentId)
    {
        return _versionRepository.GetVersionsByDocumentId(documentId);
    }

    public void CreateNewVersion(Database.Models.Document document, string newFilePath, string versionDescription)
    {
        var oldFilePath = document.FilePath;
        var oldText = _fileStorageManager.ReadFile(oldFilePath);
        var newText = _fileStorageManager.ReadFile(newFilePath);
        var diff = _diffManager.GetDiffText(oldText, newText);
        var version = new Database.Models.Version
        {
            DocumentId = document.Id,
            Document = document,
            VersionDescription = versionDescription,
            FilePath = newFilePath,
            CreationDate = DateTime.Now
        };
        document.Versions.Add(version);
        document.LastModifiedDate = DateTime.Now;
        _fileStorageManager.CopyFile(newFilePath, oldFilePath);
        _logger.LogInformation($"New version {version.Id} created for document {document.Id}");
    }

    public string GetVersionDifference(int oldVersionId, int newVersionId)
    {
        var oldVersion = _versionRepository.GetVersionById(oldVersionId);
        var newVersion = _versionRepository.GetVersionById(newVersionId);
        return _diffManager.GetDiff(oldVersion.FilePath, newVersion.FilePath);
    }

    public void SwitchToVersionAndDeleteNewer(Database.Models.Document document, Database.Models.Version version)
    {
        var oldFilePath = document.FilePath;
        var newFilePath = version.FilePath;
        _fileStorageManager.CopyFile(newFilePath, oldFilePath);
        document.Versions.RemoveAll(v => v.CreationDate > version.CreationDate);
        document.LastModifiedDate = DateTime.Now;
        _logger.LogInformation($"Switched to version {version.Id} for document {document.Id}");
    }

    public void SwitchToVersionAndSaveAsLatest(Database.Models.Document document, Database.Models.Version version)
    {
        var oldFilePath = document.FilePath;
        var newFilePath = version.FilePath;
        _fileStorageManager.CopyFile(newFilePath, oldFilePath);
        document.LastModifiedDate = DateTime.Now;
        var newVersion = new Database.Models.Version
        {
            DocumentId = document.Id,
            Document = document,
            VersionDescription = version.VersionDescription,
            FilePath = oldFilePath,
            CreationDate = DateTime.Now
        };
        document.Versions.Add(newVersion);
        _logger.LogInformation($"Switched to version {version.Id} and saved as latest for document {document.Id}");
    }

    public void ChangeVersionDescription(Database.Models.Version version, string newDescription)
    {
        version.VersionDescription = newDescription;
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {version.Id} description changed to {newDescription}");
    }

    public void DeleteVersion(Database.Models.Document document, Database.Models.Version version)
    {
        document.Versions.Remove(version);
        _fileStorageManager.DeleteFile(version.FilePath);
        _versionRepository.DeleteVersion(version);
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {version.Id} deleted from document {document.Id}");
    }

    public void DeleteVersion(int versionId)
    {
        var version = _versionRepository.GetVersionById(versionId);
        _versionRepository.DeleteVersion(version);
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {versionId} deleted");
    }
}

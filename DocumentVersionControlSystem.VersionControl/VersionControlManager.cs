namespace DocumentVersionControlSystem.VersionControl;
using DocumentVersionControlSystem.Database.Models;

public class VersionControlManager
{
    private readonly Database.Repositories.VersionRepository _versionRepository;
    private readonly DiffManager.DiffManager _diffManager;
    private readonly Logging.Logger _logger;

    public VersionControlManager(Database.Repositories.VersionRepository versionRepository, DiffManager.DiffManager diffManager, Logging.Logger logger)
    {
        _versionRepository = versionRepository;
        _diffManager = diffManager;
        _logger = logger;
    }

    public Version GetVersionById(int id)
    {
        return _versionRepository.GetVersionById(id);
    }

    public List<Version> GetAllVersions()
    {
        return _versionRepository.GetAllVersions();
    }

    public List<Version> GetVersionsByDocumentId(int documentId)
    {
        return _versionRepository.GetVersionsByDocumentId(documentId);
    }

    public void CreateNewVersion(Document document, string newFilePath, string versionDescription)
    {
        var oldFilePath = document.FilePath;
        var oldText = FileStorage.FileStorageManager.ReadFile(oldFilePath);
        var newText = FileStorage.FileStorageManager.ReadFile(newFilePath);
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
        FileStorage.FileStorageManager.CopyFile(newFilePath, oldFilePath);
        _logger.LogInformation($"New version {version.Id} created for document {document.Id}");
    }

    public string GetVersionDifference(int oldVersionId, int newVersionId)
    {
        var oldVersion = _versionRepository.GetVersionById(oldVersionId);
        var newVersion = _versionRepository.GetVersionById(newVersionId);
        return _diffManager.GetDiff(oldVersion.FilePath, newVersion.FilePath);
    }

    public void SwitchToVersionAndDeleteNewer(Document document, Version version)
    {
        var oldFilePath = document.FilePath;
        var newFilePath = version.FilePath;
        FileStorage.FileStorageManager.CopyFile(newFilePath, oldFilePath);
        document.Versions.RemoveAll(v => v.CreationDate > version.CreationDate);
        document.LastModifiedDate = DateTime.Now;
        _logger.LogInformation($"Switched to version {version.Id} for document {document.Id}");
    }

    public void SwitchToVersionAndSaveAsLatest(Document document, Version version)
    {
        var oldFilePath = document.FilePath;
        var newFilePath = version.FilePath;
        FileStorage.FileStorageManager.CopyFile(newFilePath, oldFilePath);
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

    public void ChangeVersionDescription(Version version, string newDescription)
    {
        version.VersionDescription = newDescription;
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {version.Id} description changed to {newDescription}");
    }

    public void DeleteVersion(Version version)
    {
        _versionRepository.DeleteVersion(version);
        _versionRepository.SaveChanges();
        FileStorage.FileStorageManager.DeleteFile(version.FilePath);
        _logger.LogInformation($"Version {version.Id} deleted");
    }

    public void DeleteVersion(Document document, Version version)
    {
        document.Versions.Remove(version);
        FileStorage.FileStorageManager.DeleteFile(version.FilePath);
        _versionRepository.DeleteVersion(version);
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {version.Id} deleted from document {document.Id}");
    }

    public void DeleteVersion(int versionId)
    {
        var version = _versionRepository.GetVersionById(versionId);
        FileStorage.FileStorageManager.DeleteFile(version.FilePath);
        _versionRepository.DeleteVersion(version);
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {versionId} deleted");
    }
}

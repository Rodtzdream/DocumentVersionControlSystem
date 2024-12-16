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

    public void CreateNewVersion(Document document, string versionDescription)
    {
        Version version;
        string oldFilePath, newFilePath;

        if (document.Versions == null)
        {
            document.Versions = new List<Version>();
        }

        if (document.Versions.Count == 0)
        {
            oldFilePath = document.FilePath;
            newFilePath = document.FilePath + ".v1";
            version = new Version
            {
                DocumentId = document.Id,
                VersionDescription = versionDescription,
                FilePath = newFilePath,
                CreationDate = DateTime.Now
            };
        }
        else
        {
            oldFilePath = document.Versions.Last().FilePath;
            newFilePath = oldFilePath + ".v" + (document.Versions.Count + 1);
            GetVersionDifference(oldFilePath, newFilePath);
            version = new Version
            {
                DocumentId = document.Id,
                VersionDescription = versionDescription,
                FilePath = newFilePath,
                CreationDate = DateTime.Now
            };
        }
        document.Versions.Add(version);
        FileStorage.FileStorageManager.CopyFile(newFilePath, oldFilePath);
        _logger.LogInformation($"New version {version.Id} created for document {document.Id}");
        return;
    }

    public string GetVersionDifference(int documentId, int lastVersionId)
    {
        var oldVersion = _versionRepository.GetVersionById(documentId);
        var newVersion = _versionRepository.GetVersionById(lastVersionId);
        return _diffManager.GetDiff(oldVersion.FilePath, newVersion.FilePath);
    }

    public string GetVersionDifference(string oldFilePath, string newFilePath)
    {
        return _diffManager.GetDiff(oldFilePath, newFilePath);
    }

    public void SwitchToVersionAndDeleteNewer(Document document, Version version)
    {
        var documentFilePath = document.FilePath;
        var versionFilePath = version.FilePath;
        FileStorage.FileStorageManager.CopyFile(versionFilePath, documentFilePath);
        document.Versions.RemoveAll(v => v.CreationDate > version.CreationDate);
        document.LastModifiedDate = DateTime.Now;
        _logger.LogInformation($"Switched to version {version.Id} for document {document.Id}");
    }

    public void SwitchToVersionAndSaveAsLatest(Document document, Version version)
    {
        var documentFilePath = document.FilePath;
        var versionFilePath = version.FilePath;
        FileStorage.FileStorageManager.CopyFile(versionFilePath, documentFilePath);
        document.LastModifiedDate = DateTime.Now;
        var newVersion = new Version
        {
            DocumentId = document.Id,
            VersionDescription = version.VersionDescription,
            FilePath = documentFilePath,
            CreationDate = DateTime.Now
        };
        document.Versions.Add(newVersion);
        _logger.LogInformation($"Switched to version {version.Id} and saved as latest for document {document.Id}");
    }

    public void ChangeVersionDescription(Version version, string newDescription)
    {
        version.VersionDescription = newDescription;
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {version.Id} description changed to '{newDescription}'");
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

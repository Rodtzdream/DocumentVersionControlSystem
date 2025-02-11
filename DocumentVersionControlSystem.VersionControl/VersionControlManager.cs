namespace DocumentVersionControlSystem.VersionControl;

using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.Database.Repositories;
using Microsoft.EntityFrameworkCore;

public class VersionControlManager
{
    private readonly VersionRepository _versionRepository;
    private readonly DocumentRepository _documentRepository;
    private readonly DiffManager.DiffManager _diffManager = new();
    private readonly Logging.Logger _logger;

    public VersionControlManager(Logging.Logger logger)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DocVerControlDB;Integrated Security=True"); // Replace with your actual connection string
        _versionRepository = new VersionRepository(new DatabaseContext(optionsBuilder.Options));
        _documentRepository = new DocumentRepository(new DatabaseContext(optionsBuilder.Options));
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

    public bool CreateNewVersion(Document document, string versionDescription)
    {
        if (document.VersionCount > 0 && !_diffManager.IsFileChanged(document.FilePath, _versionRepository.GetLatestVersionByDocumentId(document.Id).FilePath))
        {
            return false;
        }

        var documentDirectory = Path.Combine("Documents", document.Name);
        string oldFilePath = document.FilePath;
        string newFilePath = Path.Combine(documentDirectory, $"{document.Name}_{DateTime.Now:yyyyMMddHHmmss}.txt");

        Version version = new()
        {
            DocumentId = document.Id,
            VersionDescription = versionDescription,
            FilePath = newFilePath,
            CreationDate = DateTime.Now
        };

        _versionRepository.AddVersion(document, version);
        FileStorage.FileStorageManager.CopyFile(oldFilePath, newFilePath);
        _logger.LogInformation($"New version {version.Id} created for document {document.Id}");

        _documentRepository.UpdateDocument(document);
        return true;
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

    public void SwitchToVersionAndDeleteNewer(int documentId, int versionId, string description)
    {
        Version version = GetVersionById(versionId);
        Document? document = _documentRepository.GetDocumentById(documentId);

        if (document == null)
        {
            _logger.LogError($"Document with ID {documentId} not found.");
            throw new InvalidOperationException($"Document with ID {documentId} not found.");
        }

        var documentFilePath = document.FilePath;
        var versionFilePath = version.FilePath;

        FileStorage.FileStorageManager.CopyFile(versionFilePath, documentFilePath);

        if (document.VersionCount != 0)
        {
            var versionsToDelete = _versionRepository.GetVersionsByDocumentId(document.Id).Where(v => v.CreationDate > version.CreationDate).ToList();
            foreach (var v in versionsToDelete)
            {
                FileStorage.FileStorageManager.DeleteFile(v.FilePath);
                _versionRepository.DeleteVersion(v);
                document.VersionCount--;
            }
        }

        document.LastModifiedDate = DateTime.Now;
        _documentRepository.UpdateDocument(document);
        version.VersionDescription = description;
        _versionRepository.UpdateVersion(version);
        _logger.LogInformation($"Switched to version {version.Id} for document {document.Id}");
    }

    public Version SwitchToVersionAndSaveAsLatest(int documentId, int versionId, string description)
    {
        Version version = GetVersionById(versionId);
        Document? document = _documentRepository.GetDocumentById(documentId);

        if (!_diffManager.IsFileChanged(_versionRepository.GetLatestVersionByDocumentId(document.Id).FilePath, version.FilePath))
        {
            _logger.LogError($"Version {versionId} is the same as the latest version.");
            return version;
        }

        var documentDirectory = Path.Combine("Documents", document.Name);
        string newFilePath = Path.Combine(documentDirectory, $"{document.Name}_{DateTime.Now:yyyyMMddHHmmss}.txt");

        var documentFilePath = document.FilePath;
        var versionFilePath = version.FilePath;

        FileStorage.FileStorageManager.CopyFile(versionFilePath, documentFilePath);

        var newVersion = new Version
        {
            DocumentId = document.Id,
            VersionDescription = description,
            FilePath = newFilePath,
            CreationDate = DateTime.Now
        };

        _versionRepository.AddVersion(document, newVersion);
        _versionRepository.UpdateVersion(version);

        document.LastModifiedDate = DateTime.Now;
        FileStorage.FileStorageManager.CopyFile(documentFilePath, newFilePath);
        _documentRepository.UpdateDocument(document);

        _logger.LogInformation($"Switched to version {version.Id} and saved as latest for document {document.Id}");

        return newVersion;
    }

    public void ChangeVersionDescription(int versionId, string newDescription)
    {
        GetVersionById(versionId).VersionDescription = newDescription;
        _versionRepository.SaveChanges();
        _logger.LogInformation($"Version {versionId} description changed to '{newDescription}'");
    }

    public void DeleteVersion(Version version)
    {
        _versionRepository.DeleteVersion(version);
        _versionRepository.SaveChanges();
        FileStorage.FileStorageManager.DeleteFile(version.FilePath);
        _logger.LogInformation($"Version {version.Id} deleted");
    }

    public void DeleteVersion(int versionId)
    {
        Version version = GetVersionById(versionId);
        var document = _documentRepository.GetDocumentById(version.DocumentId);

        DeleteVersion(version);

        if (document != null)
        {
            document.VersionCount--;
            _documentRepository.UpdateDocument(document);
        }
    }
}

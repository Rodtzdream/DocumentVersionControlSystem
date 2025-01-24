﻿namespace DocumentVersionControlSystem.VersionControl;

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
        if (document.VersionCount > 0 && !_diffManager.IsFileChanged(document.FilePath, _versionRepository.GetVersionsByDocumentId(document.Id).Last().FilePath))
        {
            return false;
        }

        Version version;
        string oldFilePath, newFilePath;

        var documentDirectory = Path.Combine("Documents", document.Name);
        oldFilePath = document.FilePath;
        newFilePath = Path.Combine(documentDirectory, $"{document.Name}_v{document.VersionCount + 1}.txt");

        version = new Version
        {
            DocumentId = document.Id,
            VersionDescription = versionDescription,
            FilePath = newFilePath,
            CreationDate = DateTime.Now
        };

        _versionRepository.AddVersion(document, version);
        FileStorage.FileStorageManager.CopyFile(oldFilePath, newFilePath);
        _logger.LogInformation($"New version {version.Id} created for document {document.Id}");

        _documentRepository.SaveChanges();
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

    public void SwitchToVersionAndDeleteNewer(Document document, int versionId)
    {
        Version version = GetVersionById(versionId);

        var documentFilePath = document.FilePath;
        var versionFilePath = version.FilePath;

        FileStorage.FileStorageManager.CopyFile(versionFilePath, documentFilePath);

        if (document.VersionCount != 0)
        {
            var versionsToDelete = _versionRepository.GetVersionsByDocumentId(document.Id).Where(v => v.CreationDate > version.CreationDate).ToList();
            foreach (var v in versionsToDelete)
            {
                FileStorage.FileStorageManager.DeleteFile(v.FilePath);
            }
            _versionRepository.GetVersionsByDocumentId(document.Id).RemoveAll(v => v.CreationDate > version.CreationDate);
        }

        document.LastModifiedDate = DateTime.Now;
        _logger.LogInformation($"Switched to version {version.Id} for document {document.Id}");
    }

    public void SwitchToVersionAndSaveAsLatest(Document document, int versionId)
    {
        Version version = GetVersionById(versionId);

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

        _versionRepository.AddVersion(document, newVersion);
        _logger.LogInformation($"Switched to version {version.Id} and saved as latest for document {document.Id}");
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
        DeleteVersion(version);
    }
}

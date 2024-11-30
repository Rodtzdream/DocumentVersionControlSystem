using Microsoft.EntityFrameworkCore;
using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.Database.Repositories;

namespace DocumentVersionControlSystem.Tests;

public class VersionRepositoryTests
{
    [Fact]
    public void AddVersion_ShouldAddVersionToDocument()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "AddDocument_ShouldAddDocumentToDatabase1")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var versionRepository = new VersionRepository(context);

            // Act
            versionRepository.AddVersion(document, version);
            versionRepository.SaveChanges();

            // Assert
            Assert.Single(context.Versions);
            Assert.Single(context.Documents);
            Assert.Equal(document.Id, version.DocumentId);
        }
    }

    [Fact]
    public void GetVersionById_ShouldReturnVersion()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetVersionById_ShouldReturnVersion")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            context.Versions.Add(version);
            context.SaveChanges();

            var versionRepository = new VersionRepository(context);

            // Act
            var result = versionRepository.GetVersionById(version.Id);

            // Assert
            Assert.Equal(version, result);
        }
    }

    [Fact]
    public void GetAllVersions_ShouldReturnAllVersions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetAllVersions_ShouldReturnAllVersions")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version1 = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version 1",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version2 = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version 2",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            context.Versions.Add(version1);
            context.Versions.Add(version2);
            context.SaveChanges();

            var versionRepository = new VersionRepository(context);

            // Act
            var result = versionRepository.GetAllVersions();

            // Assert
            Assert.Equal(2, result.Count);
        }
    }

    [Fact]
    public void DeleteVersion_ShouldDeleteVersion()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "DeleteVersion_ShouldDeleteVersion")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            context.Versions.Add(version);
            context.SaveChanges();

            var versionRepository = new VersionRepository(context);

            // Act
            versionRepository.DeleteVersion(version);
            versionRepository.SaveChanges();

            // Assert
            Assert.Empty(context.Versions);
        }
    }

    [Fact]
    public void GetVersionsByDocumentId_ShouldReturnVersions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetVersionsByDocumentId_ShouldReturnVersions")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version1 = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version 1",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version2 = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version 2",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            context.Versions.Add(version1);
            context.Versions.Add(version2);
            context.SaveChanges();

            var versionRepository = new VersionRepository(context);

            // Act
            var result = versionRepository.GetVersionsByDocumentId(document.Id);

            // Assert
            Assert.Equal(2, result.Count);
        }
    }

    [Fact]
    public void SaveChanges_ShouldSaveChanges()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "SaveChanges_ShouldSaveChanges")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var versionRepository = new VersionRepository(context);

            // Act
            versionRepository.AddVersion(document, version);
            versionRepository.SaveChanges();

            // Assert
            Assert.Single(context.Versions);
            Assert.Single(context.Documents);
            Assert.Equal(document.Id, version.DocumentId);
        }
    }

    [Fact]
    public void AddVersion_ShouldAddVersionToDocument_WhenDocumentHasNoVersions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "AddDocument_ShouldAddDocumentToDatabase2")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var versionRepository = new VersionRepository(context);

            // Act
            versionRepository.AddVersion(document, version);
            versionRepository.SaveChanges();

            // Assert
            Assert.Single(context.Versions);
            Assert.Single(context.Documents);
            Assert.Equal(document.Id, version.DocumentId);
        }
    }

    [Fact]
    public void AddVersion_ShouldAddVersionToDocument_WhenDocumentHasVersions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "AddDocument_ShouldAddDocumentToDatabase3")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version1 = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version 1",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version2 = new Database.Models.Version
            {
                Document = document,
                DocumentId = document.Id,
                VersionDescription = "Test Version 2",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var versionRepository = new VersionRepository(context);

            // Act
            versionRepository.AddVersion(document, version1);
            versionRepository.SaveChanges();
            versionRepository.AddVersion(document, version2);
            versionRepository.SaveChanges();

            // Assert
            Assert.Equal(2, context.Versions.Count());
            Assert.Single(context.Documents);
            Assert.Equal(document.Id, version1.DocumentId);
            Assert.Equal(document.Id, version2.DocumentId);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.Database.Repositories;
using System;

namespace DocumentVersionControlSystem.Tests;

public class VersionRepositoryTests
{
    [Fact]
    public void AddVersion_ShouldAddVersionToDocument()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "AddVersion_ShouldAddVersionToDocument")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var versionRepository = new VersionRepository(context);

            var document = new Document
            {
                Name = "Test_Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            documentRepository.AddDocument(document);

            var version = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version",
                FilePath = "C:\\Documents\\Test_Document.v1.txt",
                CreationDate = DateTime.Now
            };

            versionRepository.AddVersion(document, version);

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
            VersionRepository versionRepository = new VersionRepository(context);

            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            var version = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version",
                FilePath = "C:\\Documents\\Test_Document.v1.txt",
                CreationDate = DateTime.Now
            };

            versionRepository.AddVersion(document, version);

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
            DocumentRepository documentRepository = new DocumentRepository(context);

            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            documentRepository.AddDocument(document);

            var version1 = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version 1",
                FilePath = "C:\\Documents\\Test_Document.v1.txt",
                CreationDate = DateTime.Now
            };

            var version2 = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version 2",
                FilePath = "C:\\Documents\\Test_Document.v2.txt",
                CreationDate = DateTime.Now
            };

            var versionRepository = new VersionRepository(context);

            versionRepository.AddVersion(document, version1);
            versionRepository.AddVersion(document, version2);

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
            DocumentRepository documentRepository = new DocumentRepository(context);
            VersionRepository versionRepository = new VersionRepository(context);

            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            documentRepository.AddDocument(document);

            var version = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version",
                FilePath = "C:\\Documents\\Test_Document.v1.txt",
                CreationDate = DateTime.Now
            };

            versionRepository.AddVersion(document, version);

            // Act
            versionRepository.DeleteVersion(version);

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
            DocumentRepository documentRepository = new DocumentRepository(context);
            VersionRepository versionRepository = new VersionRepository(context);

            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            documentRepository.AddDocument(document);

            var version1 = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version 1",
                FilePath = "C:\\Documents\\Test_Document.v1.txt",
                CreationDate = DateTime.Now
            };

            var version2 = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version 2",
                FilePath = "C:\\Documents\\Test_Document.v2.txt",
                CreationDate = DateTime.Now
            };

            versionRepository.AddVersion(document, version1);
            versionRepository.AddVersion(document, version2);

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
            var documentRepository = new DocumentRepository(context);
            var versionRepository = new VersionRepository(context);

            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            documentRepository.AddDocument(document);

            var version = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version",
                FilePath = "C:\\Documents\\Test_Document.v1.txt",
                CreationDate = DateTime.Now
            };

            // Act
            versionRepository.AddVersion(document, version);

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
            .UseInMemoryDatabase(databaseName: "AddDocument_ShouldAddDocumentToDatabase2")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var versionRepository = new VersionRepository(context);

            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            documentRepository.AddDocument(document);

            var version1 = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version 1",
                FilePath = "C:\\Documents\\Test_Document.v1.txt",
                CreationDate = DateTime.Now
            };

            var version2 = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version 2",
                FilePath = "C:\\Documents\\Test_Document.v2.txt",
                CreationDate = DateTime.Now
            };


            // Act
            versionRepository.AddVersion(document, version1);
            versionRepository.AddVersion(document, version2);

            // Assert
            Assert.Equal(2, context.Versions.Count());
            Assert.Single(context.Documents);
            Assert.Equal(document.Id, version1.DocumentId);
            Assert.Equal(document.Id, version2.DocumentId);
        }
    }
}
using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.Database.Repositories;
using DocumentVersionControlSystem.DiffManager;
using DocumentVersionControlSystem.FileStorage;
using DocumentVersionControlSystem.Logging;
using DocumentVersionControlSystem.VersionControl;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DocumentVersionControlSystem.Tests;

public class VersionRepositoryTests
{
    private readonly Mock<Logger> _mockLogger = new Mock<Logger>();

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

    [Fact]
    public void GetVersionById_ShouldReturnVersion()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetVersionById_ShouldReturnVersion")
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
            var result = versionRepository.GetVersionById(version.Id);

            // Assert
            Assert.Equal(version.Id, result.Id);
            Assert.Equal(version.DocumentId, result.DocumentId);
            Assert.Equal(version.FilePath, result.FilePath);
            Assert.Equal(version.VersionDescription, result.VersionDescription);
            Assert.Equal(version.CreationDate.ToUniversalTime(), result.CreationDate.ToUniversalTime());
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
    public void ChangeVersionDescription_ShouldChangeDescription()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "ChangeVersionDescription_ShouldChangeDescription")
            .Options;

        var mockFileStorage = new Mock<IFileStorageManager>(); // МОК файл-сховища
        var mockDiffManager = new Mock<IDiffManager>(); // МОК порівняння файлів

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var versionRepository = new VersionRepository(context);
            var versionControlManager = new VersionControlManager("AppFolderPath", _mockLogger.Object, mockFileStorage.Object, mockDiffManager.Object, context);

            var document = new Document
            {
                Name = "Test Document",
                FilePath = "C:\\Documents\\TestDocument.txt"
            };

            documentRepository.AddDocument(document);

            var version = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Old Description",
                FilePath = "C:\\Documents\\Test_Document.v1.txt",
                CreationDate = DateTime.Now
            };

            versionRepository.AddVersion(document, version);

            // Act
            versionControlManager.ChangeVersionDescription(version.Id, "New Description");

            // Assert
            Assert.Equal("New Description", context.Versions.First().VersionDescription);
        }
    }

    [Fact]
    public void SwitchToVersionAndDeleteNewer_ShouldSwitchAndDeleteNewerVersions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "SwitchToVersionAndDeleteNewer_ShouldSwitchAndDeleteNewerVersions")
        .Options;

        var mockFileStorage = new Mock<IFileStorageManager>(); // МОК файл-сховища
        var mockDiffManager = new Mock<IDiffManager>(); // МОК порівняння файлів

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var versionRepository = new VersionRepository(context);
            var versionControlManager = new VersionControlManager("AppFolderPath", _mockLogger.Object, mockFileStorage.Object, mockDiffManager.Object, context);

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
                CreationDate = DateTime.Now.AddMinutes(-10)
            };

            var version2 = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version 2",
                FilePath = "C:\\Documents\\Test_Document.v2.txt",
                CreationDate = DateTime.Now.AddMinutes(-5)
            };

            versionRepository.AddVersion(document, version1);
            versionRepository.AddVersion(document, version2);

            // **Налаштуй мок, щоб нічого не робив**
            mockFileStorage.Setup(f => f.DeleteFile(It.IsAny<string>()));
            mockFileStorage.Setup(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            versionControlManager.SwitchToVersionAndDeleteNewer(document.Id, version1.Id, "Switched to version 1");

            // Assert
            Assert.Single(context.Versions);
            Assert.Equal(version1.Id, context.Versions.First().Id);

            // **Переконайся, що виклики до файлової системи відбулися**
            mockFileStorage.Verify(f => f.DeleteFile("C:\\Documents\\Test_Document.v2.txt"), Times.Once);
            mockFileStorage.Verify(f => f.CopyFile("C:\\Documents\\Test_Document.v1.txt", "C:\\Documents\\TestDocument.txt"), Times.Once);
        }
    }

    [Fact]
    public void SwitchToVersionAndSaveAsLatest_ShouldSwitchAndSaveAsLatest()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "SwitchToVersionAndSaveAsLatest_ShouldSwitchAndSaveAsLatest")
            .Options;

        var mockFileStorage = new Mock<IFileStorageManager>(); // МОК файл-сховища
        var mockDiffManager = new Mock<IDiffManager>(); // МОК порівняння файлів

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var versionRepository = new VersionRepository(context);
            var versionControlManager = new VersionControlManager("AppFolderPath", _mockLogger.Object, mockFileStorage.Object, mockDiffManager.Object, context);

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
                CreationDate = DateTime.Now.AddMinutes(-10)
            };

            var version2 = new Database.Models.Version
            {
                DocumentId = document.Id,
                VersionDescription = "Test Version 2",
                FilePath = "C:\\Documents\\Test_Document.v2.txt",
                CreationDate = DateTime.Now.AddMinutes(-5)
            };

            versionRepository.AddVersion(document, version1);
            versionRepository.AddVersion(document, version2);

            // **Налаштовуємо поведінку моків**
            mockDiffManager.Setup(dm => dm.IsFileChanged(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            mockFileStorage.Setup(fs => fs.CopyFile(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var newVersion = versionControlManager.SwitchToVersionAndSaveAsLatest(document.Id, version1.Id, "Switched to version 1 and saved as latest");

            // Assert
            Assert.Equal(3, context.Versions.Count());
            Assert.Equal(newVersion.Id, context.Versions.Last().Id);
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
            versionRepository.DeleteVersion(document, version);

            // Assert
            Assert.Empty(context.Versions);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.Database.Repositories;

namespace DocumentVersionControlSystem.Tests;

public class DocumentRepositoryTests
{
    [Fact]
    public void AddDocument_ShouldAddDocumentToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "AddDocument_ShouldAddDocumentToDatabase")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now
            };

            // Act
            documentRepository.AddDocument(document);
            documentRepository.SaveChanges();

            // Assert
            Assert.Single(context.Documents);
        }
    }

    [Fact]
    public void GetDocumentById_ShouldReturnDocument()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetDocumentById_ShouldReturnDocument")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now
            };

            documentRepository.AddDocument(document);
            documentRepository.SaveChanges();

            // Act
            var result = documentRepository.GetDocumentById(1);

            // Assert
            Assert.NotNull(result);
        }
    }

    [Fact]
    public void GetAllDocuments_ShouldReturnAllDocuments()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetAllDocuments_ShouldReturnAllDocuments")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document1 = new Document
            {
                Name = "Test Document 1",
                FilePath = "test1.txt",
                CreationDate = DateTime.Now
            };

            var document2 = new Document
            {
                Name = "Test Document 2",
                FilePath = "test2.txt",
                CreationDate = DateTime.Now
            };

            documentRepository.AddDocument(document1);
            documentRepository.AddDocument(document2);
            documentRepository.SaveChanges();

            // Act
            var result = documentRepository.GetAllDocuments();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Assert
            Assert.Collection(result,
                doc => Assert.Equal("Test Document 1", doc.Name),
                doc => Assert.Equal("Test Document 2", doc.Name));
        }
    }

    [Fact]
    public void UpdateDocument_ShouldUpdateDocumentInDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "UpdateDocument_ShouldUpdateDocumentInDatabase")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now
            };

            documentRepository.AddDocument(document);
            documentRepository.SaveChanges();

            // Act
            document.Name = "Updated Test Document";
            documentRepository.UpdateDocument(document);
            documentRepository.SaveChanges();

            // Assert
            var result = documentRepository.GetDocumentById(1);
            Assert.Equal("Updated Test Document", result.Name);
        }
    }

    [Fact]
    public void DeleteDocument_ShouldDeleteDocumentFromDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "DeleteDocument_ShouldDeleteDocumentFromDatabase")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now
            };

            documentRepository.AddDocument(document);
            documentRepository.SaveChanges();

            // Act
            documentRepository.DeleteDocument(document);
            documentRepository.SaveChanges();

            // Assert
            Assert.Empty(context.Documents);
        }
    }

    [Fact]
    public void GetDocumentsByName_ShouldReturnDocumentsWithGivenName()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetDocumentsByName_ShouldReturnDocumentsWithGivenName")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document1 = new Document
            {
                Name = "Test Document",
                FilePath = "test1.txt",
                CreationDate = DateTime.Now
            };

            var document2 = new Document
            {
                Name = "Test Document",
                FilePath = "test2.txt",
                CreationDate = DateTime.Now
            };

            documentRepository.AddDocument(document1);
            documentRepository.AddDocument(document2);
            documentRepository.SaveChanges();

            // Act
            var result = documentRepository.GetDocumentsByName("Test Document");

            // Assert
            Assert.Collection(result,
                doc => Assert.Equal("Test Document", doc.Name),
                doc => Assert.Equal("Test Document", doc.Name));
        }
    }

    [Fact]
    public void GetDocumentByPath_ShouldReturnDocumentWithGivenPath()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetDocumentByPath_ShouldReturnDocumentWithGivenPath")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now
            };

            documentRepository.AddDocument(document);
            documentRepository.SaveChanges();

            // Act
            var result = documentRepository.GetDocumentByPath("test.txt");

            // Assert
            Assert.NotNull(result);
        }
    }

    [Fact]
    public void SaveChanges_ShouldSaveChangesToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "SaveChanges_ShouldSaveChangesToDatabase")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now
            };

            documentRepository.AddDocument(document);

            // Act
            documentRepository.SaveChanges();

            // Assert
            Assert.Single(context.Documents);
        }
    }

    [Fact]
    public void SaveChanges_ShouldNotSaveChangesIfNotCalled()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "SaveChanges_ShouldNotSaveChangesIfNotCalled")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now
            };

            documentRepository.AddDocument(document);

            // Act
            // No call to SaveChanges

            // Assert
            Assert.Empty(context.Documents);
        }
    }

    [Fact]
    public void GetDocumentById_ShouldReturnDocumentWithVersions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetDocumentById_ShouldReturnDocumentWithVersions")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now,
                Versions = new List<Database.Models.Version>()
            };

            var version1 = new Database.Models.Version
            {
                Document = document,
                VersionDescription = "Initial version",
                FilePath = "test1.txt",
                CreationDate = DateTime.Now
            };

            var version2 = new Database.Models.Version
            {
                Document = document,
                VersionDescription = "Updated version",
                FilePath = "test2.txt",
                CreationDate = DateTime.Now
            };

            document.Versions.Add(version1);
            document.Versions.Add(version2);

            documentRepository.AddDocument(document);
            documentRepository.SaveChanges();

            // Act
            var result = documentRepository.GetDocumentById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Versions.Count);
        }
    }

    [Fact]
    public void DeleteDocument_ShouldDeleteDocumentWithVersionsFromDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "DeleteDocument_ShouldDeleteDocumentWithVersionsFromDatabase")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now,
                Versions = new List<Database.Models.Version>()
            };

            var version1 = new Database.Models.Version
            {
                Document = document,
                VersionDescription = "Initial version",
                FilePath = "test1.txt",
                CreationDate = DateTime.Now
            };

            var version2 = new Database.Models.Version
            {
                Document = document,
                VersionDescription = "Updated version",
                FilePath = "test2.txt",
                CreationDate = DateTime.Now
            };

            document.Versions.Add(version1);
            document.Versions.Add(version2);

            documentRepository.AddDocument(document);
            documentRepository.SaveChanges();

            // Act
            documentRepository.DeleteDocument(document);
            documentRepository.SaveChanges();

            // Assert
            Assert.Empty(context.Documents);
            Assert.Empty(context.Versions);
        }
    }

    [Fact]
    public void GetDocumentsByName_ShouldReturnDocumentsWithVersions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetDocumentsByName_ShouldReturnDocumentsWithVersions")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document1 = new Document
            {
                Name = "Test Document",
                FilePath = "test1.txt",
                CreationDate = DateTime.Now,
                Versions = new List<Database.Models.Version>()
            };

            var document2 = new Document
            {
                Name = "Test Document",
                FilePath = "test2.txt",
                CreationDate = DateTime.Now,
                Versions = new List<Database.Models.Version>()
            };

            var version1 = new Database.Models.Version
            {
                Document = document1,
                VersionDescription = "Initial version",
                FilePath = "test1.txt",
                CreationDate = DateTime.Now
            };

            var version2 = new Database.Models.Version
            {
                Document = document2,
                VersionDescription = "Updated version",
                FilePath = "test2.txt",
                CreationDate = DateTime.Now
            };

            document1.Versions.Add(version1);
            document2.Versions.Add(version2);

            documentRepository.AddDocument(document1);
            documentRepository.AddDocument(document2);
            documentRepository.SaveChanges();

            // Act
            var result = documentRepository.GetDocumentsByName("Test Document");

            // Assert
            Assert.Collection(result,
                doc => Assert.Equal("Test Document", doc.Name),
                doc => Assert.Equal("Test Document", doc.Name));

            Assert.Collection(result[0].Versions,
                ver => Assert.Equal("Initial version", ver.VersionDescription));

            Assert.Collection(result[1].Versions,
                ver => Assert.Equal("Updated version", ver.VersionDescription));
        }
    }

    [Fact]
    public void GetDocumentByPath_ShouldReturnDocumentWithVersions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetDocumentByPath_ShouldReturnDocumentWithVersions")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);
            var document = new Document
            {
                Name = "Test Document",
                FilePath = "test.txt",
                CreationDate = DateTime.Now,
                Versions = new List<Database.Models.Version>()
            };

            var version1 = new Database.Models.Version
            {
                Document = document,
                VersionDescription = "Initial version",
                FilePath = "test1.txt",
                CreationDate = DateTime.Now
            };

            var version2 = new Database.Models.Version
            {
                Document = document,
                VersionDescription = "Updated version",
                FilePath = "test2.txt",
                CreationDate = DateTime.Now
            };

            document.Versions.Add(version1);
            document.Versions.Add(version2);

            documentRepository.AddDocument(document);
            documentRepository.SaveChanges();

            // Act
            var result = documentRepository.GetDocumentByPath("test.txt");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Versions.Count);
        }
    }

    [Fact]
    public void GetDocumentByPath_ShouldReturnNullIfDocumentDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetDocumentByPath_ShouldReturnNullIfDocumentDoesNotExist")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);

            // Act
            var result = documentRepository.GetDocumentByPath("test.txt");

            // Assert
            Assert.Null(result);
        }
    }

    [Fact]
    public void GetDocumentById_ShouldReturnNullIfDocumentDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "GetDocumentById_ShouldReturnNullIfDocumentDoesNotExist")
            .Options;

        using (var context = new DatabaseContext(options))
        {
            var documentRepository = new DocumentRepository(context);

            // Act
            var result = documentRepository.GetDocumentById(1);

            // Assert
            Assert.Null(result);
        }
    }
}

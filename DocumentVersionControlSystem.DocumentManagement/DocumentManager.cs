using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using Microsoft.EntityFrameworkCore;
namespace DocumentVersionControlSystem.DocumentManagement;

public class DocumentManager
{
    private readonly Database.Repositories.DocumentRepository _documentRepository;
    private readonly Logging.Logger _logger;
    private readonly List<FileChangeWatcher> _fileWatchers = new();

    private bool _isInternalRename = false;

    public DocumentManager(Logging.Logger logger)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DocVerControlDB;Integrated Security=True"); // Replace with your actual connection string
        _documentRepository = new Database.Repositories.DocumentRepository(new DatabaseContext(optionsBuilder.Options));
        _logger = logger;
    }

    public void InitializeFileWatchers()
    {
        var documents = _documentRepository.GetAllDocuments();
        foreach (var document in documents)
        {
            var watcher = new FileChangeWatcher(
                document.FilePath,
                (oldPath, newPath) => HandleFileRenamedOrMoved(oldPath, newPath),
                (path) => HandleFileDeleted(path)
            );

            _fileWatchers.Add(watcher);
        }
    }

    private void HandleFileRenamedOrMoved(string oldPath, string newPath)
    {
        if (_isInternalRename)
            return;

        _isInternalRename = true;

        var document = _documentRepository.GetDocumentByPath(oldPath);
        if (document != null)
        {
            string newName = Path.GetFileNameWithoutExtension(newPath);

            // Переміщення папки документа
            Directory.Move(Path.Combine("Documents", document.Name), Path.Combine("Documents", newName));

            // Оновлення шляху в документі
            document.FilePath = newPath;
            document.Name = newName;
            document.LastModifiedDate = DateTime.Now;

            // Оновлення документа в базі даних
            _documentRepository.UpdateDocument(document);
            _documentRepository.SaveChanges();
            _logger.LogInformation($"Document {document.Id} renamed or moved");
        }

        _isInternalRename = false;
    }

    private void HandleFileDeleted(string path)
    {
        var document = _documentRepository.GetDocumentByPath(path);
        if (document != null)
        {
            _documentRepository.DeleteDocument(document);
            _documentRepository.SaveChanges();
            _logger.LogInformation($"Document {document.Id} deleted");
        }
    }

    public void StopAllWatchers()
    {
        foreach (var watcher in _fileWatchers)
            watcher.Stop();
        _fileWatchers.Clear();
    }

    public void AddDocument(string filePath)
    {
        if (GetDocumentsByName(Path.GetFileName(filePath)).Count > 0)
        {
            _logger.LogError($"Document {filePath} already exists");
            return;
        }

        var fileInfo = new FileInfo(filePath);

        var document = new Database.Models.Document
        {
            Name = Path.GetFileNameWithoutExtension(fileInfo.Name),
            FilePath = filePath,
            CreationDate = fileInfo.CreationTime,
            LastModifiedDate = fileInfo.LastWriteTime
        };

        var documentDirectory = Path.Combine("Documents", document.Name);
        if (!Directory.Exists(documentDirectory))
        {
            Directory.CreateDirectory(documentDirectory);
        }

        _documentRepository.AddDocument(document);
        _logger.LogInformation($"Document {document.Id} added");
    }

    public void RenameDocument(Document document, string newName)
    {
        _isInternalRename = true;

        if (document == null || string.IsNullOrWhiteSpace(newName))
        {
            _logger.LogWarning("RenameDocument: Invalid document or new name.");
            return;
        }

        // Перевірка на недопустимі символи у новому імені файлу
        if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            _logger.LogError($"RenameDocument: Invalid characters in new name {newName}.");
            return;
        }

        try
        {
            var oldFilePath = document.FilePath;
            var fileDirectory = Path.GetDirectoryName(oldFilePath) ?? string.Empty; // Отримуємо директорію файлу
            var newFilePath = Path.Combine(fileDirectory, newName) + ".txt";

            var currentDocumentPath = Path.Combine("Documents", document.Name);
            var newDocumentPath = Path.Combine("Documents", newName);

            // Перевірка, чи існує старий файл
            if (!File.Exists(oldFilePath))
            {
                _logger.LogError($"RenameDocument: File {oldFilePath} does not exist.");
                return;
            }

            // Перевірка, чи не існує файл із новим ім'ям
            if (File.Exists(newFilePath))
            {
                _logger.LogError($"RenameDocument: File {newFilePath} already exists.");
                return;
            }

            // Перейменування файлу
            File.Move(oldFilePath, newFilePath);

            // Переміщення папки документа
            Directory.Move(currentDocumentPath, newDocumentPath);

            // Оновлення шляху в документі
            document.FilePath = newFilePath;
            document.Name = newName;
            document.LastModifiedDate = DateTime.Now;

            // Оновлення документа в базі даних
            _documentRepository.UpdateDocument(document);
            _documentRepository.SaveChanges();

            _logger.LogInformation($"Document {document.Id} renamed successfully to {newName}.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"RenameDocument: Failed to rename document {document.Id}. Error: {ex.Message}");
        }

        _isInternalRename = false;
    }


    public Database.Models.Document? GetDocumentById(int id)
    {
        return _documentRepository.GetDocumentById(id);
    }

    public List<Database.Models.Document> GetAllDocuments()
    {
        return _documentRepository.GetAllDocuments();
    }

    public List<Database.Models.Document> GetDocumentsByName(string name)
    {
        return _documentRepository.GetDocumentsByName(name);
    }

    public void UpdateDocument(Database.Models.Document document)
    {
        _documentRepository.UpdateDocument(document);
        _documentRepository.SaveChanges();
        _logger.LogInformation($"Document {document.Id} updated");
    }

    public void UpdateDocument(int documentId, string name)
    {
        var document = _documentRepository.GetDocumentById(documentId);
        if (document != null)
        {
            document.Name = name;
            document.LastModifiedDate = DateTime.Now;
            _documentRepository.UpdateDocument(document);
            _documentRepository.SaveChanges();
            _logger.LogInformation($"Document {document.Id} updated");
        }
    }

    public void UpdateDocument(int documentId)
    {
        var document = _documentRepository.GetDocumentById(documentId);
        if (document != null)
        {
            document.LastModifiedDate = DateTime.Now;
            _documentRepository.UpdateDocument(document);
            _documentRepository.SaveChanges();
            _logger.LogInformation($"Document {document.Id} updated");
        }
    }

    public void DeleteDocument(Database.Models.Document document)
    {
        _documentRepository.DeleteDocument(document);
        _documentRepository.SaveChanges();

        var documentDirectory = Path.Combine("Documents", document.Name);
        if (Directory.Exists(documentDirectory))
        {
            Directory.Delete(documentDirectory, true);
        }

        _logger.LogInformation($"Document {document.Id} deleted");
    }

    public void DeleteDocument(int documentId)
    {
        var document = _documentRepository.GetDocumentById(documentId);
        if (document != null)
        {
            var documentDirectory = Path.Combine("Documents", document.Name);

            if (Directory.Exists(documentDirectory))
            {
                Directory.Delete(documentDirectory, true);
            }

            _documentRepository.DeleteDocument(document);
            _documentRepository.SaveChanges();
            _logger.LogInformation($"Document {document.Id} deleted");
        }
    }
}

﻿using DocumentVersionControlSystem.Database.Contexts;
using Microsoft.EntityFrameworkCore;
namespace DocumentVersionControlSystem.Database.Repositories;

public class DocumentRepository
{
    private readonly DatabaseContext _context;

    public DocumentRepository(DatabaseContext context)
    {
        _context = context;
    }

    public Models.Document? GetDocumentById(int id)
    {
        return _context.Documents.AsNoTracking().FirstOrDefault(d => d.Id == id);
    }

    public List<Models.Document> GetAllDocuments()
    {
        return _context.Documents.AsNoTracking().ToList();
    }

    public List<Models.Document> GetDocumentsByName(string name)
    {
        return _context.Documents.AsNoTracking().Where(d => d.Name == name).ToList();
    }

    public Models.Document? GetDocumentByPath(string path)
    {
        return _context.Documents.AsNoTracking().FirstOrDefault(d => d.FilePath == path);
    }

    public void AddDocument(Models.Document document)
    {
        _context.Documents.Add(document);
        _context.SaveChanges();
    }

    public void UpdateDocument(Models.Document document)
    {
        var existingDocument = _context.Documents.Local.FirstOrDefault(d => d.Id == document.Id);
        if (existingDocument != null)
        {
            _context.Entry(existingDocument).State = EntityState.Detached;
        }
        _context.Documents.Update(document);
        _context.SaveChanges();
    }

    public void UpdateDocumentLastModifiedDate(Models.Document document, DateTime dateTime)
    {
        document.LastModifiedDate = dateTime;
        _context.Entry(document).Property(d => d.LastModifiedDate).IsModified = true;
        _context.SaveChanges();
    }

    public void DeleteDocument(Models.Document document)
    {
        if (document.VersionCount > 0)
        {
            var versionsToRemove = _context.Versions.Where(v => v.DocumentId == document.Id).ToList();
            _context.Versions.RemoveRange(versionsToRemove);
        }
        _context.Documents.Remove(document);
        _context.SaveChanges();
    }
}

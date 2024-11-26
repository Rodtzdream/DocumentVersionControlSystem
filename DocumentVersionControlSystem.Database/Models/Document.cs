﻿namespace DocumentVersionControlSystem.Database.Models
{
    public class Document
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public required string FileReference { get; set; }
        public List<Version>? Versions { get; set; }
    }
}
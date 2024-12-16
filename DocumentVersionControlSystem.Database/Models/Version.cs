namespace DocumentVersionControlSystem.Database.Models
{
    public class Version
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string? VersionDescription { get; set; }
        public required string FilePath { get; set; }
        public DateTime CreationDate { get; set; }
    }
}

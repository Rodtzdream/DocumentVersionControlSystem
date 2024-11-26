namespace DocumentVersionControlSystem.Database.Models
{
    public class Version
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public required Document Document { get; set; }
        public string? VersionDescription { get; set; }
        public required string FileReference { get; set; }
        public DateTime CreationDate { get; set; }

        //ToDo: Diff-file Reference
    }
}

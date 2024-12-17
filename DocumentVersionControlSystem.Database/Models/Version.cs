namespace DocumentVersionControlSystem.Database.Models;

public class Version
{
    public int Id { get; set; }
    public required int DocumentId { get; set; }
    public string? VersionDescription { get; set; }
    public string? FilePath { get; set; }
    public required DateTime CreationDate { get; set; }
}

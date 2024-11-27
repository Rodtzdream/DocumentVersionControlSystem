namespace DocumentVersionControlSystem.DiffManager;

public class DiffManager
{
    public string GetDiff(string oldFileReference, string newFileReference)
    {
        var oldText = System.IO.File.ReadAllText(oldFileReference);
        var newText = System.IO.File.ReadAllText(newFileReference);
        return GetDiffText(oldText, newText);
    }

    public string GetDiffText(string oldText, string newText)
    {
        var differ = new DiffPlex.Differ();
        var diffBuilder = new DiffPlex.DiffBuilder.SideBySideDiffBuilder(differ);
        var diffModel = diffBuilder.BuildDiffModel(oldText, newText);
        diffBuilder.Equals(diffModel);

        var result = new System.Text.StringBuilder();
        foreach (var line in diffModel.NewText.Lines)
        {
            result.AppendLine($"{line.Type}: {line.Text}");
        }
        return result.ToString();
    }

    public bool IsFileChanged(string oldFileReference, string newFileReference)
    {
        var oldText = System.IO.File.ReadAllText(oldFileReference);
        var newText = System.IO.File.ReadAllText(newFileReference);
        return IsTextChanged(oldText, newText);
    }

    public bool IsTextChanged(string oldText, string newText)
    {
        return !string.Equals(oldText, newText, StringComparison.Ordinal);
    }
}

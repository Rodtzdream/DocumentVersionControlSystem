namespace DocumentVersionControlSystem.DiffManager;

public class DiffManager
{
    public string GetDiff(string firstFilePath, string secondFilePath)
    {
        var oldText = System.IO.File.ReadAllText(firstFilePath);
        var newText = System.IO.File.ReadAllText(secondFilePath);
        return GetDiffText(oldText, newText);
    }

    public string GetDiffText(string textA, string textB)
    {
        var differ = new DiffPlex.Differ();
        var diffBuilder = new DiffPlex.DiffBuilder.SideBySideDiffBuilder(differ);
        var diffModel = diffBuilder.BuildDiffModel(textA, textB);
        diffBuilder.Equals(diffModel);

        var result = new System.Text.StringBuilder();
        foreach (var line in diffModel.NewText.Lines)
        {
            result.AppendLine($"{line.Type}: {line.Text}");
        }
        return result.ToString();
    }

    public bool IsFileChanged(string firstFilePath, string secondFilePath)
    {
        var oldText = System.IO.File.ReadAllText(firstFilePath);
        var newText = System.IO.File.ReadAllText(secondFilePath);
        return IsTextChanged(oldText, newText);
    }

    public bool IsTextChanged(string textA, string textB)
    {
        return !string.Equals(textA, textB, StringComparison.Ordinal);
    }
}

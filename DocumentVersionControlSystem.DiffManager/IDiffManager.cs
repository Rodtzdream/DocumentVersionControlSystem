namespace DocumentVersionControlSystem.DiffManager
{
    public interface IDiffManager
    {
        string GetDiff(string firstFilePath, string secondFilePath);
        string GetDiffText(string textA, string textB);
        bool IsFileChanged(string firstFilePath, string secondFilePath);
        bool IsTextChanged(string textA, string textB);
    }
}

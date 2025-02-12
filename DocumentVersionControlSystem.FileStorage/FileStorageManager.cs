namespace DocumentVersionControlSystem.FileStorage;

public class FileStorageManager : IFileStorageManager
{
    public void SaveFile(string FilePath, string content)
    {
        System.IO.File.WriteAllText(FilePath, content);
    }

    public string ReadFile(string FilePath)
    {
        return System.IO.File.ReadAllText(FilePath);
    }

    public void DeleteFile(string FilePath)
    {
        System.IO.File.Delete(FilePath);
    }

    public bool FileExists(string FilePath)
    {
        return System.IO.File.Exists(FilePath);
    }

    public void CopyFile(string sourceFilePath, string destinationFilePath)
    {
        System.IO.File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
    }
}

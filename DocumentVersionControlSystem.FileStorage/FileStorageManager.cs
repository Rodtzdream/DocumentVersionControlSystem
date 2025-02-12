namespace DocumentVersionControlSystem.FileStorage;

public class FileStorageManager : IFileStorageManager
{
    public void SaveFile(string filePath, string content)
    {
        System.IO.File.WriteAllText(filePath, content);
    }

    public string ReadFile(string filePath)
    {
        return System.IO.File.ReadAllText(filePath);
    }

    public void DeleteFile(string filePath)
    {
        System.IO.File.Delete(filePath);
    }

    public bool FileExists(string filePath)
    {
        return System.IO.File.Exists(filePath);
    }

    public void CopyFile(string sourceFilePath, string destinationFilePath)
    {
        System.IO.File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
    }
}

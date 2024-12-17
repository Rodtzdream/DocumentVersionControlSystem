namespace DocumentVersionControlSystem.FileStorage;

public static class FileStorageManager
{
    public static void SaveFile(string FilePath, string content)
    {
        System.IO.File.WriteAllText(FilePath, content);
    }

    public static string ReadFile(string FilePath)
    {
        return System.IO.File.ReadAllText(FilePath);
    }

    public static void DeleteFile(string FilePath)
    {
        System.IO.File.Delete(FilePath);
    }

    public static bool FileExists(string FilePath)
    {
        return System.IO.File.Exists(FilePath);
    }

    public static void CopyFile(string sourceFilePath, string destinationFilePath)
    {
        System.IO.File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
    }
}

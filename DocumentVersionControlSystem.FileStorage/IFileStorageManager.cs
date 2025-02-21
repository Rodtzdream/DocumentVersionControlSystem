namespace DocumentVersionControlSystem.FileStorage;

public interface IFileStorageManager
{
    string ReadFile(string filePath);
    void DeleteFile(string filePath);
    bool FileExists(string filePath);
    void CopyFile(string sourceFilePath, string destinationFilePath);
}

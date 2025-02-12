namespace DocumentVersionControlSystem.FileStorage;

public interface IFileStorageManager
{
    void SaveFile(string filePath, string content);
    string ReadFile(string filePath);
    void DeleteFile(string filePath);
    bool FileExists(string filePath);
    void CopyFile(string sourceFilePath, string destinationFilePath);
}

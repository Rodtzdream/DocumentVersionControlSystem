namespace DocumentVersionControlSystem.FileStorage;

public interface IFileStorageManager
{
    void SaveFile(string FilePath, string content);
    string ReadFile(string FilePath);
    void DeleteFile(string FilePath);
    bool FileExists(string FilePath);
    void CopyFile(string sourceFilePath, string destinationFilePath);
}

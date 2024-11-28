namespace DocumentVersionControlSystem.DocumentManagement;

public class FileChangeWatcher
{
    private readonly FileSystemWatcher _watcher;
    private readonly Action<string, string> _onRenamedOrMoved;
    private readonly Action<string> _onDeleted;

    public FileChangeWatcher(string pathToWatch, Action<string, string> onRenamedOrMoved, Action<string> onDeleted)
    {
        _onRenamedOrMoved = onRenamedOrMoved;
        _onDeleted = onDeleted;

        _watcher = new FileSystemWatcher(Path.GetDirectoryName(pathToWatch)!)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            IncludeSubdirectories = true
        };

        _watcher.Renamed += (sender, e) => _onRenamedOrMoved?.Invoke(e.OldFullPath, e.FullPath);
        _watcher.Deleted += (sender, e) => _onDeleted?.Invoke(e.FullPath);
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
    }
}

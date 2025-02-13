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

        _watcher.Renamed += OnRenamed;
        _watcher.Deleted += OnDeleted;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _onRenamedOrMoved(e.OldFullPath, e.FullPath);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        _onDeleted(e.FullPath);
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Renamed -= OnRenamed;
        _watcher.Deleted -= OnDeleted;
        _watcher.Dispose();
    }
}

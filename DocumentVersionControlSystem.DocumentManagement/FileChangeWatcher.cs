namespace DocumentVersionControlSystem.DocumentManagement;

public class FileChangeWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly Action<string, string> _onRenamed;
    private readonly Action<string> _onDeleted;
    private bool _disposed = false;

    public string FileName => _watcher.Filter;

    public FileChangeWatcher(string pathToWatch, Action<string, string> onRenamed, Action<string> onDeleted)
    {
        _onRenamed = onRenamed ?? throw new ArgumentNullException(nameof(onRenamed));
        _onDeleted = onDeleted ?? throw new ArgumentNullException(nameof(onDeleted));

        _watcher = new FileSystemWatcher(Path.GetDirectoryName(pathToWatch)!)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = Path.GetFileName(pathToWatch)
        };

        _watcher.Renamed += OnRenamed;
        _watcher.Deleted += OnDeleted;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _onRenamed?.Invoke(e.OldFullPath, e.FullPath);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        _onDeleted?.Invoke(e.FullPath);
    }

    public void Stop()
    {
        if (_disposed) return;

        _watcher.EnableRaisingEvents = false;
        _watcher.Renamed -= OnRenamed;
        _watcher.Deleted -= OnDeleted;
    }

    public void Dispose()
    {
        if (_disposed) return;

        Stop();
        _watcher.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}

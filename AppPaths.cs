namespace DocumentVersionControlSystem.Infrastructure;

public static class AppPaths
{
    public static readonly string AppFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DocumentVersionControlSystem"
    );

    public static readonly string DatabaseFilePath = Path.Combine(AppFolderPath, "database.db");

    static AppPaths()
    {
        try
        {
            if (!Directory.Exists(AppFolderPath))
            {
                Directory.CreateDirectory(AppFolderPath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка при створенні директорії: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
}
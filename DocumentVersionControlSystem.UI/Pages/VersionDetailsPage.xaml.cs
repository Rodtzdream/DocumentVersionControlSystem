using DocumentVersionControlSystem.FileStorage;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DocumentVersionControlSystem.UI.Windows;

public partial class VersionDetailsPage : Page
{
    private readonly MainWindow _mainWindow;
    private readonly IFileStorageManager _fileStorageManager;
    private readonly VersionControlManager _versionControlManager;
    private Database.Models.Version _version;

    public VersionDetailsPage(MainWindow mainWindow, IFileStorageManager fileStorageManager, Database.Models.Version version, VersionControlManager versionControlManager)
    {
        _mainWindow = mainWindow;
        _fileStorageManager = fileStorageManager;
        _versionControlManager = versionControlManager;
        _version = version;

        InitializeComponent();
        LoadVersionDetails();
        _mainWindow.AddVersionButtons(_version.Id);

        Loaded += VersionDetailsPage_Loaded;
        _mainWindow.SizeChanged += OnWindowSizeChanged;
    }

    private void VersionDetailsPage_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonSizes(_mainWindow.ActualWidth, _mainWindow.ActualHeight);
    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateButtonSizes(e.NewSize.Width, e.NewSize.Height);
    }

    private void UpdateButtonSizes(double width, double height)
    {
        ApplyVersionButton.Width = width * 0.06;
        ApplyVersionButton.Height = height * 0.033;
        ApplyVersionButton.FontSize = Math.Max(12, ActualWidth * 0.007);
    }

    public int GetVersionId()
    {
        return _version.Id;
    }

    public bool ReadDocument()
    {
        if (!_fileStorageManager.FileExists(_version.FilePath))
            return false;

        DocumentText.Text = _fileStorageManager.ReadFile(_version.FilePath);
        return true;
    }

    private void ReadDescription()
    {
        VersionDescription.Text = string.IsNullOrEmpty(_version.VersionDescription)
            ? "Version description: None"
            : "Version description: " + _version.VersionDescription;
    }

    private void ReadDateTime()
    {
        VersionDateTime.Text = "Creation date & time: " + _version.CreationDate;
    }

    private void LoadVersionDetails()
    {
        ReadDocument();
        ReadDescription();
        ReadDateTime();
    }

    public bool RefreshWindow()
    {
        if (!ReadDocument())
            return false;

        ReadDescription();
        ReadDateTime();
        _mainWindow.AddVersionButtons(_version.Id);
        return true;
    }

    private void ApplyVersionButton_Click(object sender, RoutedEventArgs e)
    {
        var popup = new SelectSwitchOptionPopup(_version.VersionDescription);
        popup.ShowDialog();

        if (popup.DialogResult == true)
        {
            if (popup.switchOption == SelectSwitchOptionPopup.SwitchOption.DeleteNewer)
            {
                _versionControlManager.SwitchToVersionAndDeleteNewer(_version.DocumentId, _version.Id, popup.Description);
            }
            else if (popup.switchOption == SelectSwitchOptionPopup.SwitchOption.SaveAsTheLatest)
            {
                var newVersion = _versionControlManager.SwitchToVersionAndSaveAsLatest(_version.DocumentId, _version.Id, popup.Description);
                if (newVersion.Id == _version.Id)
                {
                    new InfoPopup(InfoPopupType.NoChangesDetected).ShowDialog();
                }
                else
                {
                    _version = newVersion;
                }
            }
            RefreshWindow();
        }
    }
}

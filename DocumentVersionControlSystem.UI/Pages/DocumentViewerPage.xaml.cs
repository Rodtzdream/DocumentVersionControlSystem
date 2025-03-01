using DocumentVersionControlSystem.FileStorage;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DocumentVersionControlSystem.UI.Windows;

public partial class DocumentViewerPage : Page
{
    private readonly MainWindow _mainWindow;
    private readonly IFileStorageManager _fileStorageManager;
    private readonly VersionControlManager _versionControlManager;
    private readonly Database.Models.Document _document;

    public DocumentViewerPage(MainWindow mainWindow, IFileStorageManager fileStorageManager, VersionControlManager versionControl, Database.Models.Document document)
    {
        _mainWindow = mainWindow;
        _fileStorageManager = fileStorageManager;
        _versionControlManager = versionControl;
        _document = document;

        InitializeComponent();
        ReadDocument();

        Loaded += DocumentViewerPage_Loaded;
        _mainWindow.SizeChanged += OnWindowSizeChanged;
    }

    private void DocumentViewerPage_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonSizes(_mainWindow.ActualWidth, _mainWindow.ActualHeight);
    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateButtonSizes(e.NewSize.Width, e.NewSize.Height);
    }

    private void UpdateButtonSizes(double width, double height)
    {
        double newSize = Math.Max(12, ActualWidth * 0.007);

        OpenDocumentButton.Width = width * 0.06;
        OpenDocumentButton.Height = height * 0.033;
        OpenDocumentButton.Margin = new Thickness(width * 0.025 + 80, 0, 0, 0);
        OpenDocumentButton.FontSize = newSize;

        CreateNewVersionButton.Width = width * 0.07;
        CreateNewVersionButton.Height = height * 0.033;
        CreateNewVersionButton.Margin = new Thickness(0, 0, width * 0.006 + 5, 0);
        CreateNewVersionButton.FontSize = newSize;
    }

    public bool ReadDocument()
    {
        if (!_fileStorageManager.FileExists(_document.FilePath))
            return false;

        _document.VersionCount = _versionControlManager.GetVersionsByDocumentId(_document.Id).Count;
        DocumentText.Text = File.ReadAllText(_document.FilePath);
        return true;
    }

    private void OpenInExternalEditor_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _document.FilePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            new InfoPopup("Error", $"Failed to open document in external editor. Error: {ex.Message}").ShowDialog();
        }
    }

    private void CreateNewVersionButton_Click(object sender, RoutedEventArgs e)
    {
        ReadDocument();

        var inputPopup = new InputPopup
        {
            Title = "Description",
            TitleText = { Text = "Enter version description:" }
        };
        inputPopup.ShowDialog();

        if (inputPopup.DialogResult == true)
        {
            string versionDescription = inputPopup.MessageText.Text;
            bool versionCreated = _versionControlManager.CreateNewVersion(_document, versionDescription);

            if (versionCreated)
                _mainWindow.AddVersionButtons();

            _mainWindow.ShowInfoPopup(versionCreated ? InfoPopupType.VersionCreatedSuccessfully : InfoPopupType.NoChangesDetected);
        }
        else if (inputPopup.DialogResult == false)
        {
            _mainWindow.ShowInfoPopup(InfoPopupType.VersionCreationCanceled);
        }
    }
}

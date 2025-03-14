﻿using DocumentVersionControlSystem.FileStorage;
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

    // Constructor
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

    // Page loaded event
    private void VersionDetailsPage_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonSizes(_mainWindow.ActualWidth, _mainWindow.ActualHeight);
    }

    // Event handlers
    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateButtonSizes(e.NewSize.Width, e.NewSize.Height);
    }

    private void ApplyVersionButton_Click(object sender, RoutedEventArgs e)
    {
        var popup = new SelectSwitchOptionPopup(_version.VersionDescription);
        popup.ShowDialog();

        if (popup.DialogResult == true)
        {
            if (popup.switchOption == SelectSwitchOptionPopup.SwitchOption.DeleteNewer)
            {
                if (_versionControlManager.SwitchToVersionAndDeleteNewer(_version.DocumentId, _version.Id, popup.Description))
                {
                    MainWindow.ShowInfoPopup(InfoPopupType.VersionSwitched);
                    RefreshWindow();
                }
                else
                {
                    MainWindow.ShowInfoPopup(InfoPopupType.OnlyOneVersion);
                }
            }
            else if (popup.switchOption == SelectSwitchOptionPopup.SwitchOption.SaveAsTheLatest)
            {
                var newVersion = _versionControlManager.SwitchToVersionAndSaveAsLatest(_version.DocumentId, _version.Id, popup.Description);
                if (newVersion.Id != _version.Id)
                {
                    _version = newVersion;
                    MainWindow.ShowInfoPopup(InfoPopupType.VersionSwitched);
                    RefreshWindow();
                }
                else
                {
                    MainWindow.ShowInfoPopup(InfoPopupType.NoChangesDetected);
                }
            }
        }
    }

    // Public methods
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

    public bool RefreshWindow()
    {
        if (!ReadDocument())
            return false;

        ReadDescription();
        ReadDateTime();
        _mainWindow.AddVersionButtons(_version.Id);
        return true;
    }

    // Private supportive methods
    private void UpdateButtonSizes(double width, double height)
    {
        ApplyVersionButton.Width = width * 0.06;
        ApplyVersionButton.Height = height * 0.033;
        ApplyVersionButton.FontSize = Math.Max(12, ActualWidth * 0.007);
    }

    private void ReadDateTime()
    {
        VersionDateTime.Text = "Creation date & time: " + _version.CreationDate;
    }

    private void ReadDescription()
    {
        VersionDescription.Text = string.IsNullOrEmpty(_version.VersionDescription)
            ? "Version description: None"
            : "Version description: " + _version.VersionDescription;
    }

    private void LoadVersionDetails()
    {
        ReadDocument();
        ReadDescription();
        ReadDateTime();
    }
}

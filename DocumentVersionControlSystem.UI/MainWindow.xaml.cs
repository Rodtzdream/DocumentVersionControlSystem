﻿using DocumentVersionControlSystem.Database.Contexts;
using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.DiffManager;
using DocumentVersionControlSystem.DocumentManagement;
using DocumentVersionControlSystem.FileStorage;
using DocumentVersionControlSystem.Infrastructure;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.UI.Windows;
using DocumentVersionControlSystem.VersionControl;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using static DocumentVersionControlSystem.UI.DocumentRecoveryWindow;

namespace DocumentVersionControlSystem.UI;

public partial class MainWindow : Window
{
    private Button _selectedDocumentButton;
    private Button _selectedVersionButton;
    private Button _currentVersionButton;
    private DocumentManager _documentManager;
    private VersionControlManager _versionControlManager;
    private IDiffManager _diffManager;
    private IFileStorageManager _fileStorageManager;
    private Logging.Logger _logger;

    private HomePage _homePage;

    private TextBlock _selectDocumentTextBlock, _noVersionsAvailableTextBlock;
    private StackPanel _buttonStackPanel, _navigationButtonsStackPanel;

    // Constructor and initialization
    public MainWindow()
    {
        InitializeComponent();

        var databaseContext = new DatabaseContext();
        ApplyMigration(databaseContext);

        _fileStorageManager = new FileStorageManager();
        _logger = new Logging.Logger();
        _diffManager = new DiffManager.DiffManager();
        _documentManager = new DocumentManager(AppPaths.AppFolderPath, databaseContext, _fileStorageManager, _logger);
        _versionControlManager = new VersionControlManager(AppPaths.AppFolderPath, _logger, _fileStorageManager, _diffManager, databaseContext);
        _homePage = new HomePage(this, _documentManager, _versionControlManager, _fileStorageManager);

        _navigationButtonsStackPanel = (StackPanel)FindName("NavigationButtons");

        InitializeDynamicGrid();

        var missingDocs = _documentManager.VerifyDocumentsIntegrity();
        RecoverDocuments(missingDocs);
    }

    private static void ApplyMigration(DatabaseContext databaseContext)
    {
        try
        {
            databaseContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            new InfoPopup("Error", $"Error updating the database: {ex.Message}").ShowDialog();
            throw;
        }
    }

    private void InitializeDynamicGrid()
    {
        SizeChanged += OnWindowSizeChanged;
        Activated += MainWindow_Activated;
        _documentManager.InitializeFileWatchers();
    }

    // Static methods
    public static void ShowInfoPopup(InfoPopupType popupType)
    {
        new InfoPopup(popupType).Show();
    }

    // Event window handlers
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(_homePage);

        _buttonStackPanel = (StackPanel)FindName("ButtonStackPanel");

        _selectDocumentTextBlock = CreateTextBlock("Select the document");
        _noVersionsAvailableTextBlock = CreateTextBlock("No versions available");

        ClearVersionButtons();
    }

    private void MainWindow_Activated(object sender, EventArgs e)
    {
        if (MainFrame.Content is not Page currentPage) return;

        if (_documentManager.IsFileExternallyDeleted())
        {
            var missingDocs = _documentManager.VerifyDocumentsIntegrity();
            RecoverDocuments(missingDocs);
            _documentManager.SetFileExternallyDeleted(false);

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        var allDocumentsCount = _documentManager.GetAllDocuments().Count;
        switch (currentPage)
        {
            case HomePage:
                _homePage.AdjustGridLayout(allDocumentsCount + 1);
                ClearVersionButtons();
                break;
            case DocumentViewerPage documentViewerPage:
                if (!documentViewerPage.ReadDocument())
                {
                    NavigateToHomePage(allDocumentsCount, InfoPopupType.LoadDocumentFailed);
                }
                break;
            case VersionDetailsPage versionDetailsPage:
                if (!versionDetailsPage.RefreshWindow())
                {
                    NavigateToHomePage(allDocumentsCount, InfoPopupType.LoadVersionFailed);
                }
                break;
        }
    }

    private void MainWindow_Closed(object sender, EventArgs e)
    {
        _documentManager.StopAllWatchers();
        Application.Current.Shutdown();
    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var allDocumentsCount = _documentManager.GetAllDocuments().Count;
        _homePage.AdjustGridLayout(allDocumentsCount + 1);

        _selectedDocumentButton = _homePage.GetSelectedButton();
        if (_selectedDocumentButton != null)
        {
            _selectedDocumentButton.BorderBrush = Brushes.Gray;
            _selectedDocumentButton.BorderThickness = new Thickness(3);
            AddVersionButtons();
        }

        double newSize = Math.Max(16, ActualWidth * 0.009);
        MainTitle.FontSize = newSize;
        SidePanelTitle.FontSize = newSize;

        if (_navigationButtonsStackPanel != null)
        {
            double buttonWidth = _navigationButtonsStackPanel.ActualWidth * 0.021;
            double buttonHeight = _navigationButtonsStackPanel.ActualHeight * 0.7;
            foreach (Button button in _navigationButtonsStackPanel.Children)
            {
                button.Width = buttonWidth;
                button.Height = buttonHeight;
            }
        }

        if (MainFrame.Content is DocumentViewerPage || MainFrame.Content is VersionDetailsPage)
        {
            var documentName = _documentManager.GetDocumentsByName(_selectedDocumentButton.Content.ToString()).FirstOrDefault()?.Name;
            int maxLength = MainFrame.Content is DocumentViewerPage ? (int)(ActualWidth / 40) : (int)(ActualWidth / 20);
            MainTitle.Text = documentName.Length > maxLength
                ? $"{documentName.Substring(0, maxLength)}..."
                : $"{documentName}.txt";
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            switch (e.SystemKey)
            {
                case Key.Left:
                    PreviousButton_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.Right:
                    NextButton_Click(sender, e);
                    e.Handled = true;
                    break;
            }
        }
        else if (e.Key == Key.Home)
        {
            HomeButton_Click(sender, e);
            e.Handled = true;
        }
    }

    // Navigation
    private void NavigateToHomePage(int allDocumentsCount, InfoPopupType popupType)
    {
        MainFrame.Navigate(_homePage);
        _homePage.AdjustGridLayout(allDocumentsCount + 1);
        ClearVersionButtons();
        ShowInfoPopup(popupType);
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        if (MainFrame.Content is HomePage)
            return;

        if (_homePage == null)
            _homePage = new HomePage(this, _documentManager, _versionControlManager, _fileStorageManager);

        MainFrame.Navigate(_homePage);
        ClearVersionButtons();
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (!MainFrame.CanGoBack) return;

        MainFrame.GoBack();

        Dispatcher.InvokeAsync(() =>
        {
            var currentPage = MainFrame.Content as Page;
            switch (currentPage)
            {
                case HomePage homePage:
                    homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);
                    ClearVersionButtons();
                    break;
                case VersionDetailsPage versionDetailsPage:
                    if (!versionDetailsPage.RefreshWindow())
                    {
                        MainFrame.Navigate(_homePage);
                        _homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);
                        ClearVersionButtons();

                        ShowInfoPopup(InfoPopupType.LoadVersionFailed);
                    }
                    else
                    {
                        AddVersionButtons(versionDetailsPage.GetVersionId());
                    }
                    break;
                case DocumentViewerPage documentViewerPage:
                    documentViewerPage.ReadDocument();
                    AddVersionButtons();
                    break;
            }
        }, DispatcherPriority.Background);
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (!MainFrame.CanGoForward) return;

        MainFrame.GoForward();

        Dispatcher.InvokeAsync(() =>
        {
            var currentPage = MainFrame.Content as Page;
            switch (currentPage)
            {
                case HomePage homePage:
                    homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);
                    ClearVersionButtons();
                    break;
                case VersionDetailsPage versionDetailsPage:
                    if (!versionDetailsPage.RefreshWindow())
                    {
                        MainFrame.Navigate(_homePage);
                        _homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);
                        ClearVersionButtons();

                        ShowInfoPopup(InfoPopupType.LoadVersionFailed);
                    }
                    else
                    {
                        AddVersionButtons(versionDetailsPage.GetVersionId());
                    }
                    break;
                case DocumentViewerPage documentViewerPage:
                    documentViewerPage.ReadDocument();
                    AddVersionButtons();
                    break;
            }
        }, DispatcherPriority.Background);
    }

    private void Frame_Navigated(object sender, NavigationEventArgs e)
    {
        if (e.Content is Page page)
        {
            Title = page.Title;

            var currentPage = MainFrame.Content as Page;
            switch (currentPage)
            {
                case HomePage:
                    MainTitle.Text = "Document Version Control System";
                    break;
                case DocumentViewerPage:
                    var documentName = _documentManager.GetDocumentsByName(_selectedDocumentButton.Content.ToString()).FirstOrDefault()?.Name;
                    int maxLength = (int)(ActualWidth / 40);
                    MainTitle.Text = documentName.Length > maxLength
                        ? $"{documentName.Substring(0, maxLength)}..."
                        : $"{documentName}.txt";
                    break;
                case VersionDetailsPage:
                    documentName = _documentManager.GetDocumentsByName(_selectedDocumentButton.Content.ToString()).FirstOrDefault()?.Name;
                    maxLength = (int)(ActualWidth / 20);
                    MainTitle.Text = documentName.Length > maxLength
                        ? $"{documentName.Substring(0, maxLength)}..."
                        : $"{documentName}.txt";
                    break;
            }
        }
    }

    // Document handling
    public void RecoverDocuments(List<Document> missingDocs)
    {
        if (missingDocs.Count == 0)
            return;

        var missingDocuments = new ObservableCollection<MissingDocumentViewModel>();

        foreach (var doc in missingDocs)
        {
            missingDocuments.Add(new MissingDocumentViewModel
            {
                DocumentName = doc.Name,
                OldPath = doc.FilePath,
                NewPath = string.Empty
            });
        }

        var recoverDocumentsWindow = new DocumentRecoveryWindow(missingDocuments);
        var result = recoverDocumentsWindow.ShowDialog();

        if (result == true)
        {
            foreach (var document in missingDocuments)
            {
                if (document.NewPath.StartsWith('<'))
                {
                    _documentManager.DeleteDocument(document.OldPath);
                }
                else if (!string.IsNullOrEmpty(document.NewPath))
                {
                    _documentManager.RecoverDocument(document.OldPath, document.NewPath, false);
                }
            }
        }
    }

    public void AddVersionButtons(int currentVersionId = -1)
    {
        if (_buttonStackPanel != null)
        {
            _buttonStackPanel.Children.Clear();

            _selectedDocumentButton = _homePage.GetSelectedButton();

            var documentId = _documentManager.GetDocumentsByName(_selectedDocumentButton.Content.ToString()).First().Id;
            var versions = _versionControlManager.GetVersionsByDocumentId(documentId);

            if (versions.Count == 0)
            {
                _buttonStackPanel.Children.Add(_noVersionsAvailableTextBlock);
                return;
            }

            foreach (var version in versions)
            {
                var button = CreateVersionButton(version, currentVersionId);
                _buttonStackPanel.Children.Add(button);
            }
        }
    }

    public void ClearVersionButtons()
    {
        if (_buttonStackPanel != null)
        {
            _buttonStackPanel.Children.Clear();
            _buttonStackPanel.Children.Add(_selectDocumentTextBlock);
        }
    }

    private void OpenVersionViewer(Database.Models.Version version)
    {
        var versionDetailsWindow = new VersionDetailsPage(this, _fileStorageManager, version, _versionControlManager);
        MainFrame.Navigate(versionDetailsWindow);
    }

    // Button event handlers
    private void OnButtonVersionClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button clickedButton || clickedButton == _selectedVersionButton || clickedButton == _currentVersionButton)
            return;

        _selectedVersionButton?.ClearValue(Button.BorderBrushProperty);
        _selectedVersionButton?.ClearValue(Button.BorderThicknessProperty);

        _selectedVersionButton = clickedButton;
        clickedButton.BorderBrush = Brushes.Gray;
        clickedButton.BorderThickness = new Thickness(3);
    }

    private void OnButtonVersionDoubleClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button clickedButton && clickedButton.CommandParameter is int versionId)
        {
            if (_currentVersionButton == null || clickedButton.CommandParameter != _currentVersionButton.CommandParameter)
            {
                var version = _versionControlManager.GetVersionById(versionId);
                OpenVersionViewer(version);
                AddVersionButtons(versionId);
            }
        }
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_selectedVersionButton != null)
        {
            _selectedVersionButton.ClearValue(Button.BorderBrushProperty);
            _selectedVersionButton.ClearValue(Button.BorderThicknessProperty);
            _selectedVersionButton = null;
        }
    }

    private Button CreateVersionButton(Database.Models.Version version, int currentVersionId)
    {
        var button = new Button
        {
            Content = version.CreationDate.ToString(),
            Tag = version.VersionDescription,
            Style = (Style)FindResource("RectangleButtonStyle"),
            Margin = new Thickness(0, 8, 0, 0),
            CommandParameter = version.Id,
            ToolTip = $"{version.VersionDescription}.txt{Environment.NewLine}Created: {version.CreationDate}",
            Width = _buttonStackPanel.ActualWidth * 0.95,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 140,
            MinHeight = 40
        };

        if (currentVersionId == version.Id)
        {
            _currentVersionButton = button;
            _currentVersionButton.BorderBrush = Brushes.Black;
            _currentVersionButton.BorderThickness = new Thickness(2.5);
        }

        button.Click += OnButtonVersionClicked;
        button.MouseRightButtonDown += OnButtonVersionClicked;
        button.MouseDoubleClick += OnButtonVersionDoubleClicked;

        CreateContextMenuForVersionButton(button);
        return button;
    }

    private void CreateContextMenuForVersionButton(Button button)
    {
        var contextMenu = new ContextMenu();

        var openVersion = new MenuItem { Header = "Open" };
        openVersion.Click += OpenVersion_Click;

        var changeVersionDescription = new MenuItem { Header = "Change description" };
        changeVersionDescription.Click += ChangeDescription_Click;

        var deleteVersion = new MenuItem { Header = "Delete" };
        deleteVersion.Click += DeleteVersion_Click;

        contextMenu.Items.Add(openVersion);
        contextMenu.Items.Add(changeVersionDescription);
        contextMenu.Items.Add(deleteVersion);

        button.ContextMenu = contextMenu;
    }

    private void OpenVersion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
        {
            var parentButton = contextMenu.PlacementTarget as Button;

            if (parentButton != null)
            {
                if (_currentVersionButton == null || parentButton.CommandParameter != _currentVersionButton.CommandParameter)
                {
                    var versionId = (int)parentButton.CommandParameter;
                    var version = _versionControlManager.GetVersionById(versionId);
                    var versionDetailsWindow = new VersionDetailsPage(this, _fileStorageManager, version, _versionControlManager);
                    MainFrame.Navigate(versionDetailsWindow);
                }
            }
        }
    }

    private void ChangeDescription_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
        {
            var parentButton = contextMenu.PlacementTarget as Button;

            if (parentButton != null)
            {
                var popup = new InputPopup
                {
                    TitleText = { Text = "Change version description" },
                    MessageText = { Text = parentButton.Tag.ToString() }
                };
                popup.ShowDialog();

                var newName = popup.MessageText.Text;
                _versionControlManager.ChangeVersionDescription((int)parentButton.CommandParameter, newName);
                AddVersionButtons();
            }
        }
    }

    private void DeleteVersion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
        {
            var parentButton = contextMenu.PlacementTarget as Button;

            if (parentButton != null)
            {
                if (_currentVersionButton != null && parentButton.CommandParameter == _currentVersionButton.CommandParameter)
                {
                    _versionControlManager.DeleteVersion((int)parentButton.CommandParameter);
                    MainFrame.Navigate(_homePage);
                    ClearVersionButtons();
                    ShowInfoPopup(InfoPopupType.OpenVersionDeleted);
                }
                else
                {
                    _versionControlManager.DeleteVersion((int)parentButton.CommandParameter);

                    if (MainFrame.Content is VersionDetailsPage && _currentVersionButton != null)
                        AddVersionButtons((int)_currentVersionButton.CommandParameter);
                    else
                        AddVersionButtons();
                }
            }
        }
    }

    // Supportive methods
    private TextBlock CreateTextBlock(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.Black,
            Margin = new Thickness(0, _buttonStackPanel.ActualHeight - ActualHeight / 2, 0, 0),
            IsHitTestVisible = false
        };
    }
}
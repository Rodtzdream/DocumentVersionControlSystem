using DocumentVersionControlSystem.DocumentManagement;
using DocumentVersionControlSystem.FileStorage;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DocumentVersionControlSystem.UI.Windows;

public partial class HomePage : Page
{
    private Button _selectedDocumentButton;
    private readonly MainWindow _mainWindow;
    private readonly DocumentManager _documentManager;
    private readonly VersionControlManager _versionControlManager;
    private readonly IFileStorageManager _fileStorageManager;
    private DocumentViewerPage _documentViewerWindow;

    private int _totalButtons;

    public HomePage(MainWindow mainWindow, DocumentManager documentManagement, VersionControlManager versionControl, IFileStorageManager fileStorageManager)
    {
        InitializeComponent();

        _mainWindow = mainWindow;
        _documentManager = documentManagement;
        _versionControlManager = versionControl;
        _fileStorageManager = fileStorageManager;

        _totalButtons = _documentManager.GetAllDocuments().Count + 1;
        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        AdjustGridLayout(_totalButtons);
    }

    private void Document_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (Path.GetExtension(file).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    DropTargetBorder.BorderBrush = Brushes.Transparent;
                    DragDropText.Visibility = Visibility.Hidden;

                    AddDocument(file);
                }
                else
                {
                    DropTargetBorder.BorderBrush = Brushes.Transparent;
                    DragDropText.Visibility = Visibility.Hidden;

                    MessageBox.Show("Only .txt files are allowed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void DocumentStackPanel_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.All(file => Path.GetExtension(file).Equals(".txt", StringComparison.OrdinalIgnoreCase)))
            {
                e.Effects = DragDropEffects.Copy;

                ClearGridButtons();
                DropTargetBorder.BorderBrush = Brushes.Gray;
                DragDropText.Visibility = Visibility.Visible;

            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void DocumentStackPanel_DragLeave(object sender, DragEventArgs e)
    {
        DropTargetBorder.BorderBrush = Brushes.Transparent;
        DragDropText.Visibility = Visibility.Hidden;

        AdjustGridLayout(_totalButtons);
    }

    public void AdjustGridLayout(int totalButtons)
    {
        double windowWidth = ActualWidth - 200;
        double buttonWidth = 120;
        int columns = Math.Max((int)(windowWidth / buttonWidth), 1);
        int rows = Math.Max((int)Math.Ceiling((double)totalButtons / columns), 1);

        ButtonGrid.ColumnDefinitions.Clear();
        ButtonGrid.RowDefinitions.Clear();

        for (int i = 0; i < columns; i++)
            ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition());

        for (int i = 0; i < rows; i++)
            ButtonGrid.RowDefinitions.Add(new RowDefinition());

        AddButtonsToGrid(columns, totalButtons);
    }

    private void AddButtonsToGrid(int columns, int totalButtons)
    {
        var documents = _documentManager.GetAllDocuments();
        ButtonGrid.Children.Clear();

        var addButton = new Button
        {
            Content = "Add document",
            Style = (Style)FindResource("AddButtonStyle"),
            Margin = new Thickness(5),
        };
        addButton.Click += AddDocumentClicked;

        Grid.SetColumn(addButton, 0);
        Grid.SetRow(addButton, 0);
        ButtonGrid.Children.Add(addButton);

        int missedDocuments = 0;
        for (int i = 0; i < documents.Count; i++)
        {
            var document = documents[i];
            var button = CreateButton(document.Name, "SquareButtonStyle", document);
            if (button == null)
            {
                missedDocuments++;
                continue;
            }
            button.Tag = document.Name + ".txt";
            button.Click += OnButtonClicked;
            button.MouseDoubleClick += OnButtonDoubleClicked;
            button.PreviewMouseRightButtonDown += OnButtonClicked;
            CreateContextMenuForButton(button);

            int column = (i + 1 - missedDocuments) % columns;
            int row = (i + 1 - missedDocuments) / columns;

            Grid.SetColumn(button, column);
            Grid.SetRow(button, row);
            ButtonGrid.Children.Add(button);
        }
    }

    private Button CreateButton(string content, string styleKey, Database.Models.Document document)
    {
        var fileInfo = new FileInfo(document.FilePath);
        if (fileInfo.Exists == false)
            return null;
        long fileSize = fileInfo.Length / 1024;
        string lastModified = fileInfo.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss");

        return new Button
        {
            Content = content,
            Style = (Style)FindResource(styleKey),
            Margin = new Thickness(5),
            ToolTip = $"{content}.txt{Environment.NewLine}Size: {fileSize} KB{Environment.NewLine}Modified: {lastModified}{Environment.NewLine}Version count: {document.VersionCount}"
        };
    }

    private void ClearGridButtons()
    {
        ButtonGrid.Children.Clear();
        ButtonGrid.ColumnDefinitions.Clear();
        ButtonGrid.RowDefinitions.Clear();
    }

    public Button GetSelectedButton()
    {
        return _selectedDocumentButton;
    }

    private void OnButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_selectedDocumentButton != null)
        {
            _selectedDocumentButton.ClearValue(Button.BorderBrushProperty);
            _selectedDocumentButton.ClearValue(Button.BorderThicknessProperty);
        }

        _selectedDocumentButton = sender as Button;
        _selectedDocumentButton.BorderBrush = Brushes.Gray;
        _selectedDocumentButton.BorderThickness = new Thickness(3);

        _mainWindow.AddVersionButtons();
    }

    private void OnButtonDoubleClicked(object sender, RoutedEventArgs e)
    {
        if (_selectedDocumentButton != null)
        {
            var clickedButton = sender as Button;
            var document = _documentManager.GetDocumentsByName(clickedButton.Content.ToString()).First();

            _documentViewerWindow = new DocumentViewerPage(_mainWindow, _fileStorageManager, _versionControlManager, document);
            _mainWindow.MainFrame.Navigate(_documentViewerWindow);
            _mainWindow.AddVersionButtons();
        }
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_selectedDocumentButton != null)
        {
            _selectedDocumentButton.ClearValue(Button.BorderBrushProperty);
            _selectedDocumentButton.ClearValue(Button.BorderThicknessProperty);
            _selectedDocumentButton = null;
            _mainWindow.ClearVersionButtons();
        }
    }

    private void AddDocumentClicked(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Select a text file to add",
            Multiselect = false
        };

        bool? result = openFileDialog.ShowDialog();
        if (result == true)
        {
            AddDocument(openFileDialog.FileName);
        }
    }

    private void AddDocument(string filePath)
    {
        try
        {
            if (!_documentManager.AddDocument(filePath))
            {
                MessageBox.Show($"Document {Path.GetFileName(filePath)} with this name already exists...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _totalButtons++;
            AdjustGridLayout(_totalButtons);

            MessageBox.Show($"File {Path.GetFileName(filePath)} added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error reading file {Path.GetFileName(filePath)}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CreateContextMenuForButton(Button button)
    {
        var contextMenu = new ContextMenu();

        var openItem = new MenuItem { Header = "Open" };
        openItem.Click += Open_Click;

        var renameItem = new MenuItem { Header = "Rename" };
        renameItem.Click += Rename_Click;

        var removeItem = new MenuItem { Header = "Remove" };
        removeItem.Click += Remove_Click;

        contextMenu.Items.Add(openItem);
        contextMenu.Items.Add(renameItem);
        contextMenu.Items.Add(removeItem);

        button.ContextMenu = contextMenu;
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDocumentButton != null)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                var parentButton = contextMenu.PlacementTarget as Button;

                if (parentButton != null)
                {
                    var document = _documentManager.GetDocumentsByName(parentButton.Content.ToString()).First();

                    _documentViewerWindow = new DocumentViewerPage(_mainWindow, _fileStorageManager, _versionControlManager, document);
                    _mainWindow.MainFrame.Navigate(_documentViewerWindow);
                    _mainWindow.AddVersionButtons();
                }
            }
        }
    }

    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        var document = _documentManager.GetDocumentsByName(_selectedDocumentButton.Content.ToString()).FirstOrDefault();
        if (document == null)
        {
            MessageBox.Show("Document not found...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var popup = new InputPopup
        {
            TitleText = { Text = "Rename document" }
        };
        popup.ShowDialog();

        string newName = popup.MessageText.Text?.Trim();

        if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
        {
            MessageBox.Show("Name cannot be empty...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (newName.Equals(document.Name, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Name cannot be the same as the current one...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (newName.Length < 3 || newName.Length > 50)
        {
            MessageBox.Show("Name must be between 3 and 50 characters...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (newName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Name cannot end with '.txt'...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_documentManager.GetDocumentsByName(newName).Any())
        {
            MessageBox.Show("A document with this name already exists...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            MessageBox.Show("Name contains invalid characters...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _documentManager.RenameDocument(document, newName);
        AdjustGridLayout(_totalButtons);
        MessageBox.Show("Rename successful", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        var document = _documentManager.GetDocumentsByName(_selectedDocumentButton.Content.ToString()).First();
        _documentManager.DeleteDocument(document);
        AdjustGridLayout(_totalButtons);
        _mainWindow.ClearVersionButtons();
        MessageBox.Show("Document have been removed successfully", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

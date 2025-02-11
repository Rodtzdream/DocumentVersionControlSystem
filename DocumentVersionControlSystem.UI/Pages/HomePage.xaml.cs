using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.DocumentManagement;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.VersionControl;
using Serilog.Sinks.File;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DocumentVersionControlSystem.UI.Windows
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private Button _selectedDocumentButton;
        private DocumentManager _documentManager;
        private VersionControlManager _versionControlManager;

        private MainWindow _mainWindow;
        private DocumentViewerPage _documentViewerWindow;

        private int totalButtons = 0;

        public HomePage(MainWindow mainWindow, DocumentManager documentManagement, VersionControlManager versionControl)
        {
            InitializeComponent();

            _documentManager = documentManagement;
            _versionControlManager = versionControl;
            _mainWindow = mainWindow;

            totalButtons = _documentManager.GetAllDocuments().Count + 1;

            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustGridLayout(totalButtons); // Перераховує сітку після того, як сторінка завантажена
        }

        public void AdjustGridLayout(int totalButtons)
        {
            // Отримуємо ширину і висоту контейнера
            double windowWidth = this.ActualWidth - 200;
            double windowHeight = this.ActualHeight - 32;

            // Визначаємо приблизну ширину кнопки (з урахуванням відступів)
            double buttonWidth = 120; // Мінімальна ширина кнопки
            int columns = (int)(windowWidth / buttonWidth);
            columns = Math.Max(columns, 1); // Мінімум 1 колонка

            // Обчислюємо кількість рядків
            int rows = (int)Math.Ceiling((double)totalButtons / columns);
            rows = Math.Max(rows, 1); // Мінімум 1 рядок

            // Очищаємо існуючі стовпці і рядки
            ButtonGrid.ColumnDefinitions.Clear();
            ButtonGrid.RowDefinitions.Clear();

            // Додаємо нові стовпці і рядки
            for (int i = 0; i < columns; i++)
            {
                ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < rows; i++)
            {
                ButtonGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Додаємо кнопки в сітку
            AddButtonsToGrid(columns, rows, totalButtons);
        }

        private void AddButtonsToGrid(int columns, int rows, int totalButtons)
        {
            List<Database.Models.Document> documents = _documentManager.GetAllDocuments();

            // Очистити старі кнопки
            ButtonGrid.Children.Clear();

            Button button = new Button
            {
                Content = "Add document",
                Style = (Style)FindResource("AddButtonStyle"),
                Margin = new Thickness(5),
            };
            button.Click += AddDocumentClicked;

            // Додавання кнопки до сітки
            Grid.SetColumn(button, 0);
            Grid.SetRow(button, 0);
            ButtonGrid.Children.Add(button);

            int i = 1;
            foreach (var document in documents)
            {
                // Створення кнопки
                button = CreateButton(document.Name, "SquareButtonStyle", document);
                button.Tag = document.Name + ".txt";
                button.Click += OnButtonClicked;
                button.MouseDoubleClick += OnButtonDoubleClicked;
                button.PreviewMouseRightButtonDown += OnButtonClicked;
                CreateContextMenuForButton(button);

                // Обчислення стовпця та рядка для кнопки
                int column = i % columns;
                int row = i / columns;

                // Додавання кнопки до сітки
                Grid.SetColumn(button, column);
                Grid.SetRow(button, row);
                ButtonGrid.Children.Add(button);
                ++i;
            }
        }

        private Button CreateButton(string content, string styleKey, Database.Models.Document document)
        {
            long fileSize = new FileInfo(document.FilePath).Length / 1024;
            string lastModified = File.GetLastWriteTime(document.FilePath).ToString("dd/MM/yyyy HH:mm:ss");

            return new Button
            {
                Content = content,
                Style = (Style)FindResource(styleKey),
                Margin = new Thickness(5),
                ToolTip = $"{content}.txt" + Environment.NewLine +
                          $"Size: {fileSize} KB" + Environment.NewLine +
                          $"Modified: {lastModified}" + Environment.NewLine +
                          $"Version count: {document.VersionCount}"
            };
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

            Button clickedButton = sender as Button;
            _selectedDocumentButton = clickedButton;
            clickedButton.BorderBrush = Brushes.Gray;
            clickedButton.BorderThickness = new Thickness(3);

            _mainWindow.AddVersionButtons();
        }

        private void OnButtonDoubleClicked(object sender, RoutedEventArgs e)
        {
            if (_selectedDocumentButton != null)
            {
                Button clickedButton = sender as Button;
                Database.Models.Document document = _documentManager.GetDocumentsByName(clickedButton.Content.ToString()).First();

                _documentViewerWindow = new DocumentViewerPage(_mainWindow, _versionControlManager, document);
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
            // Створення діалогового вікна для вибору файлу
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Select a text file to add",
                Multiselect = false // Забороняє вибір кількох файлів
            };

            // Відкриття діалогового вікна та перевірка, чи файл було обрано
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filePath = openFileDialog.FileName; // Отримуємо шлях до файлу

                // Обробка файлу (наприклад, зчитування вмісту)
                try
                {
                    // Наприклад, відобразимо вміст у TextBox (замініть TextBoxName на ваш контроль)
                    _documentManager.AddDocument(filePath);

                    // Перерахунок сітки
                    ++totalButtons;
                    AdjustGridLayout(totalButtons);
                    // update window

                    MessageBox.Show("File added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CreateContextMenuForButton(Button button)
        {
            ContextMenu contextMenu = new ContextMenu();

            MenuItem item1 = new MenuItem { Header = "Open" };
            item1.Click += Open_Click;

            MenuItem item2 = new MenuItem { Header = "Rename" };
            item2.Click += Rename_Click;

            MenuItem item3 = new MenuItem { Header = "Remove" };
            item3.Click += Remove_Click;

            contextMenu.Items.Add(item1);
            contextMenu.Items.Add(item2);
            contextMenu.Items.Add(item3);

            button.ContextMenu = contextMenu;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDocumentButton != null)
            {
                // Отримати кнопку, до якої прив'язане контекстне меню
                if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
                {
                    Button parentButton = contextMenu.PlacementTarget as Button;

                    if (parentButton != null)
                    {
                        Database.Models.Document document = _documentManager
                            .GetDocumentsByName(parentButton.Content.ToString())
                        .First();

                        _documentViewerWindow = new DocumentViewerPage(_mainWindow, _versionControlManager, document);
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

            InputPopup popup = new InputPopup
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

            if (document == null)
            {
                MessageBox.Show("Invalid document...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            AdjustGridLayout(totalButtons);
            MessageBox.Show("Rename successful", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var document = _documentManager.GetDocumentsByName(_selectedDocumentButton.Content.ToString()).First();
            _documentManager.DeleteDocument(document);
            AdjustGridLayout(totalButtons);
            _mainWindow.ClearVersionButtons();
            MessageBox.Show("Document have been removed successfully", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

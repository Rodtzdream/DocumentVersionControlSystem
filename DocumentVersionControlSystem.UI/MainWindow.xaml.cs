﻿using DocumentVersionControlSystem.DocumentManagement;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.UI.Windows;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DocumentVersionControlSystem.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button _selectedButton;
        private DocumentManager _documentManager;
        private VersionControlManager _versionControlManager;
        private Logging.Logger _logger;
        int totalButtons = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDynamicGrid();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustGridLayout(totalButtons); // Перераховує сітку після того, як вікно завантажене
        }

        private void InitializeDynamicGrid()
        {
            // Прив'язка події зміни розміру вікна
            this.SizeChanged += OnWindowSizeChanged;
            this.StateChanged += MainWindow_StateChanged;

            _logger = new Logging.Logger();

            _documentManager = new DocumentManager(_logger);
            _documentManager.InitializeFileWatchers();
            totalButtons = _documentManager.GetAllDocuments().Count + 1;

            _versionControlManager = new VersionControlManager(_logger);
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Перерахунок сітки при зміні розміру
            AdjustGridLayout(totalButtons);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized || WindowState == WindowState.Normal)
            {
                // Перерахунок сітки тільки при зміні стану на нормальний чи повноекранний
                AdjustGridLayout(totalButtons);
            }
        }

        private void AdjustGridLayout(int totalButtons)
        {
            // Отримуємо ширину і висоту контейнера
            double windowWidth = this.ActualWidth - 350;
            double windowHeight = this.ActualHeight - 32;

            // Визначаємо кількість стовпців і рядків на основі розміру вікна
            int columns = (int)(windowWidth / 100);
            int rows = totalButtons - columns + 1;

            // Якщо кількість стовпців або рядків менша ніж 1, робимо їх мінімум 1
            columns = Math.Max(columns, 1);
            rows = Math.Max(rows, 1);

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

            Button button = CreateButton($"Add document", "AddButtonStyle");
            button.Click += AddDocumentClicked;

            // Додавання кнопки до сітки
            Grid.SetColumn(button, 0);
            Grid.SetRow(button, 0);
            ButtonGrid.Children.Add(button);

            int i = 1;
            foreach (var document in documents)
            {
                // Створення кнопки
                button = CreateButton(document.Name, "SquareButtonStyle");
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

        public void AddVersionButtons()
        {
            // Отримати StackPanel за ім'ям
            StackPanel stackPanel = (StackPanel)FindName("ButtonStackPanel");

            if (stackPanel != null)
            {
                stackPanel.Children.Clear();

                var documentId = _documentManager.GetDocumentsByName(_selectedButton.Content.ToString()).First().Id;
                var versions = _versionControlManager.GetVersionsByDocumentId(documentId);

                // Додати кнопки
                foreach (var version in versions) // Змінити кількість кнопок за потребою
                {
                    Button button = new Button
                    {
                        Content = version.CreationDate.ToString(),
                        Tag = version.VersionDescription,
                        Style = (Style)FindResource("RectangleButtonStyle"), // Стиль із ресурсів
                        Margin = new Thickness(0, 8, 0, 0), // Відступи
                        CommandParameter = version.Id
                    };

                    CreateContextMenuForVersionButton(button);
                    stackPanel.Children.Add(button);
                }
            }
        }

        private Button CreateButton(string content, string styleKey)
        {
            return new Button
            {
                Content = content,
                Style = (Style)FindResource(styleKey),
                Margin = new Thickness(5)
            };
        }

        private void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_selectedButton != null)
            {
                _selectedButton.ClearValue(Button.BorderBrushProperty);
                _selectedButton.ClearValue(Button.BorderThicknessProperty);
            }

            Button clickedButton = sender as Button;
            _selectedButton = clickedButton;
            clickedButton.BorderBrush = Brushes.Gray;
            clickedButton.BorderThickness = new Thickness(3);

            AddVersionButtons();
        }

        private void OnButtonDoubleClicked(object sender, RoutedEventArgs e)
        {
            if (_selectedButton != null)
            {
                Button clickedButton = sender as Button;
                Database.Models.Document document = _documentManager.GetDocumentsByName(clickedButton.Content.ToString()).First();

                DocumentViewerWindow documentViewerWindow = new DocumentViewerWindow(_documentManager, _versionControlManager, document);
                documentViewerWindow.ShowDialog();
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
            if (_selectedButton != null)
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

                        DocumentViewerWindow documentViewerWindow = new DocumentViewerWindow(
                            _documentManager,
                            _versionControlManager,
                            document
                        );

                        documentViewerWindow.ShowDialog();
                    }
                }
            }
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            var document = _documentManager.GetDocumentsByName(_selectedButton.Content.ToString()).First();
            InputPopup popup = new InputPopup();
            popup.TitleText.Text = "Rename document";
            popup.ShowDialog();

            string newName = popup.MessageText.Text;
            _documentManager.RenameDocument(document, newName);
            AdjustGridLayout(totalButtons);
            MessageBox.Show("Rename...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var document = _documentManager.GetDocumentsByName(_selectedButton.Content.ToString()).First();
            _documentManager.DeleteDocument(document);
            AdjustGridLayout(totalButtons);
            MessageBox.Show("Document have been removed...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CreateContextMenuForVersionButton(Button button)
        {
            ContextMenu contextMenu = new ContextMenu();

            MenuItem item1 = new MenuItem { Header = "Open" };
            item1.Click += OpenVersion_Click;

            MenuItem item2 = new MenuItem { Header = "Change description" };
            item2.Click += ChangeDescription_Click;

            MenuItem item3 = new MenuItem { Header = "Delete" };
            item3.Click += DeleteVersion_Click;

            contextMenu.Items.Add(item1);
            contextMenu.Items.Add(item2);
            contextMenu.Items.Add(item3);

            button.ContextMenu = contextMenu;
        }

        private void OpenVersion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                Button parentButton = contextMenu.PlacementTarget as Button;

                if (parentButton != null)
                {
                    int versionId = (int)parentButton.CommandParameter;

                    Database.Models.Version version = _versionControlManager.GetVersionById(versionId);

                    VersionDetailsWindow versionDetailsWindow = new VersionDetailsWindow(version, _versionControlManager);
                    versionDetailsWindow.ShowDialog();
                }
            }
        }

        private void ChangeDescription_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                Button parentButton = contextMenu.PlacementTarget as Button;

                if (parentButton != null)
                {
                    InputPopup popup = new InputPopup();
                    popup.TitleText.Text = "Change version description";
                    popup.MessageText.Text = parentButton.Tag.ToString();
                    popup.ShowDialog();

                    string newName = popup.MessageText.Text;
                    _versionControlManager.ChangeVersionDescription((int)parentButton.CommandParameter, newName);
                    AddVersionButtons();
                }
            }
        }

        private void DeleteVersion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                Button parentButton = contextMenu.PlacementTarget as Button;

                if (parentButton != null)
                {
                    var version = _versionControlManager.GetVersionById((int)parentButton.CommandParameter);
                    _versionControlManager.DeleteVersion(version);
                    AddVersionButtons();
                }
            }
        }
    }
}

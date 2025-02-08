using DocumentVersionControlSystem.DocumentManagement;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.UI.Windows;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DocumentVersionControlSystem.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button _selectedButton;
        private Button _selectedVersionButton;
        private DocumentManager _documentManager;
        private VersionControlManager _versionControlManager;
        private Logging.Logger _logger;

        private HomePage _homePage;

        public MainWindow()
        {
            InitializeComponent();

            _logger = new Logging.Logger();
            _documentManager = new DocumentManager(_logger);
            _versionControlManager = new VersionControlManager(_logger);
            _homePage = new HomePage(this, _documentManager, _versionControlManager);

            InitializeDynamicGrid();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(_homePage);
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Перерахунок сітки при зміні розміру
            _homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);

            _selectedButton = _homePage.GetSelectedButton();
            if (_selectedButton != null)
            {
                _selectedButton.BorderBrush = Brushes.Gray;
                _selectedButton.BorderThickness = new Thickness(3);
                AddVersionButtons();
            }

            double newSize = Math.Max(16, this.ActualWidth * 0.01); // Мінімальний розмір 12
            DocumentVersionsTextBlock.FontSize = newSize;
            DocVersionControlTextBlock.FontSize = newSize;

            StackPanel stackPanel = (StackPanel)FindName("NavigationButtons");

            if (stackPanel != null)
            {
                foreach (Button button in stackPanel.Children)
                {
                    button.Width = stackPanel.ActualWidth * 0.021;
                    button.Height = stackPanel.ActualHeight * 0.7;
                }
            }
        }

        private void InitializeDynamicGrid()
        {
            // Прив'язка події зміни розміру вікна
            this.SizeChanged += OnWindowSizeChanged;

            _documentManager.InitializeFileWatchers();
        }

        private void OnButtonVersionClicked(object sender, RoutedEventArgs e)
        {
            if (_selectedVersionButton != null)
            {
                _selectedVersionButton.ClearValue(Button.BorderBrushProperty);
                _selectedVersionButton.ClearValue(Button.BorderThicknessProperty);
            }

            Button clickedButton = sender as Button;
            _selectedVersionButton = clickedButton;
            clickedButton.BorderBrush = Brushes.Gray;
            clickedButton.BorderThickness = new Thickness(3);

            var version = _versionControlManager.GetVersionById((int)clickedButton.CommandParameter);
            OpenVersionViewer(version);
        }

        private void OpenVersionViewer(Database.Models.Version version)
        {
            VersionDetailsPage versionDetailsWindow = new VersionDetailsPage(this, version, _versionControlManager);
            MainFrame.Navigate(versionDetailsWindow);
        }

        public void AddVersionButtons()
        {
            // Отримати StackPanel за ім'ям
            StackPanel stackPanel = (StackPanel)FindName("ButtonStackPanel");

            if (stackPanel != null)
            {
                stackPanel.Children.Clear();

                _selectedButton = _homePage.GetSelectedButton();
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
                        CommandParameter = version.Id,

                        // Адаптивність:
                        Width = stackPanel.ActualWidth * 0.95, // Автоширина (заповнює StackPanel)
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        MinWidth = 140, // Мінімальна ширина
                        MinHeight = 40   // Мінімальна висота
                    };

                    button.Click += OnButtonVersionClicked;

                    CreateContextMenuForVersionButton(button);
                    stackPanel.Children.Add(button);
                }
            }
        }

        public void ClearVersionButtons()
        {
            StackPanel stackPanel = (StackPanel)FindName("ButtonStackPanel");
            if (stackPanel != null)
            {
                stackPanel.Children.Clear();
            }
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

                    VersionDetailsPage versionDetailsWindow = new VersionDetailsPage(this, version, _versionControlManager);
                    MainFrame.Navigate(versionDetailsWindow);
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
                    _versionControlManager.DeleteVersion((int)parentButton.CommandParameter);
                    AddVersionButtons();
                }
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is HomePage)
                return;

            if (_homePage == null)
                _homePage = new HomePage(this, _documentManager, _versionControlManager);

            MainFrame.Navigate(_homePage);
            ClearVersionButtons();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();

                Dispatcher.Invoke(() =>
                {
                    var currentPage = MainFrame.Content as Page;
                    if (currentPage is HomePage homePage)
                    {
                        homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);
                        ClearVersionButtons();
                    }
                    else if (currentPage is VersionDetailsPage versionDetailsPage)
                    {
                        if (!versionDetailsPage.RefreshWindow())
                        {
                            MainFrame.Navigate(_homePage);
                            _homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);
                            ClearVersionButtons();
                            MessageBox.Show("Failed to load version. Returning to the home page.");
                        }
                    }
                    else if (currentPage is DocumentViewerPage documentViewerPage)
                    {
                        documentViewerPage.ReadDocument();
                    }
                }, DispatcherPriority.Background);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoForward)
            {
                MainFrame.GoForward();

                Dispatcher.Invoke(() =>
                {
                    var currentPage = MainFrame.Content as Page;
                    if (currentPage is HomePage homePage)
                    {
                        homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);
                        ClearVersionButtons();
                    }
                    else if (currentPage is VersionDetailsPage versionDetailsPage)
                    {
                        if (!versionDetailsPage.RefreshWindow())
                        {
                            MainFrame.Navigate(_homePage);
                            _homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);
                            ClearVersionButtons();
                            MessageBox.Show("Failed to load version. Returning to the home page.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else if (currentPage is DocumentViewerPage documentViewerPage)
                    {
                        documentViewerPage.ReadDocument();
                    }
                }, DispatcherPriority.Background);
            }
        }
    }
}

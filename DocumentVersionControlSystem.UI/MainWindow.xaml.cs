using DocumentVersionControlSystem.DocumentManagement;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.UI.Windows;
using DocumentVersionControlSystem.VersionControl;
using Serilog.Sinks.File;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DocumentVersionControlSystem.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button _selectedDocumentButton;
        private Button _selectedVersionButton;
        private Button _currentVersionButton;
        private DocumentManager _documentManager;
        private VersionControlManager _versionControlManager;
        private Logging.Logger _logger;

        private HomePage _homePage;

        private TextBlock _selectDocumentTextBlock, _noVersionsAvailableTextBlock;
        private StackPanel _buttonStackPanel, _navigationButtonsStackPanel;

        public MainWindow()
        {
            InitializeComponent();

            _logger = new Logging.Logger();
            _documentManager = new DocumentManager(_logger);
            _versionControlManager = new VersionControlManager(_logger);
            _homePage = new HomePage(this, _documentManager, _versionControlManager);

            _navigationButtonsStackPanel = (StackPanel)FindName("NavigationButtons");

            InitializeDynamicGrid();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(_homePage);

            _buttonStackPanel = (StackPanel)FindName("ButtonStackPanel");

            _selectDocumentTextBlock = new TextBlock
            {
                Text = "Select the document",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black,
                Margin = new Thickness(0, _buttonStackPanel.ActualHeight - ActualHeight / 2, 0, 0),
                IsHitTestVisible = false
            };

            _noVersionsAvailableTextBlock = new TextBlock
            {
                Text = "No versions available",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black,
                Margin = new Thickness(0, _buttonStackPanel.ActualHeight - ActualHeight / 2, 0, 0),
                IsHitTestVisible = false
            };

            ClearVersionButtons();
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Перерахунок сітки при зміні розміру
            _homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);

            _selectedDocumentButton = _homePage.GetSelectedButton();
            if (_selectedDocumentButton != null)
            {
                _selectedDocumentButton.BorderBrush = Brushes.Gray;
                _selectedDocumentButton.BorderThickness = new Thickness(3);
                AddVersionButtons();
            }

            double newSize = Math.Max(16, this.ActualWidth * 0.01);
            DocumentVersionsTextBlock.FontSize = newSize;
            DocVersionControlTextBlock.FontSize = newSize;

            if (_navigationButtonsStackPanel != null)
            {
                foreach (Button button in _navigationButtonsStackPanel.Children)
                {
                    button.Width = _navigationButtonsStackPanel.ActualWidth * 0.021;
                    button.Height = _navigationButtonsStackPanel.ActualHeight * 0.7;
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
                var version = _versionControlManager.GetVersionById(versionId);
                OpenVersionViewer(version);
                AddVersionButtons(versionId);
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

        private void OpenVersionViewer(Database.Models.Version version)
        {
            VersionDetailsPage versionDetailsWindow = new VersionDetailsPage(this, version, _versionControlManager);
            MainFrame.Navigate(versionDetailsWindow);
        }

        public void AddVersionButtons(int currentVersionId = -1)
        {
            // Отримати StackPanel за ім'ям

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
                    Button button = new Button
                    {
                        Content = version.CreationDate.ToString(),
                        Tag = version.VersionDescription,
                        Style = (Style)FindResource("RectangleButtonStyle"), // Стиль із ресурсів
                        Margin = new Thickness(0, 8, 0, 0), // Відступи
                        CommandParameter = version.Id,
                        ToolTip = $"{version.VersionDescription}.txt" + Environment.NewLine +
                                  $"Created: {version.CreationDate}",
                        // Адаптивність:
                        Width = _buttonStackPanel.ActualWidth * 0.95, // Автоширина (заповнює StackPanel)
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        MinWidth = 140, // Мінімальна ширина
                        MinHeight = 40   // Мінімальна висота
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
                    _homePage.AdjustGridLayout(_documentManager.GetAllDocuments().Count + 1);
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
                        else
                            AddVersionButtons(versionDetailsPage.GetVersionId());
                    }
                    else if (currentPage is DocumentViewerPage documentViewerPage)
                    {
                        documentViewerPage.ReadDocument();
                        AddVersionButtons();
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
                        else
                            AddVersionButtons(versionDetailsPage.GetVersionId());
                    }
                    else if (currentPage is DocumentViewerPage documentViewerPage)
                    {
                        documentViewerPage.ReadDocument();
                        AddVersionButtons();
                    }
                }, DispatcherPriority.Background);
            }
        }
    }
}

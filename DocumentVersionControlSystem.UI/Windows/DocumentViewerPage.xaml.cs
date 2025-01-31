using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.DocumentManagement;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DocumentVersionControlSystem.UI.Windows
{
    /// <summary>
    /// Interaction logic for DocumentViewerPage.xaml
    /// </summary>
    public partial class DocumentViewerPage : Page
    {
        private MainWindow _mainWindow;

        private Button _selectedVersionButton;
        private readonly DocumentManager _documentManager;
        private readonly VersionControlManager _versionControlManager;
        private Database.Models.Document _document;

        private TextBox textForm;

        public DocumentViewerPage(MainWindow mainWindow, DocumentManager documentManagement, VersionControlManager versionControl, Database.Models.Document document)
        {
            _documentManager = documentManagement;
            _versionControlManager = versionControl;
            _document = document;

            InitializeComponent();
            _mainWindow = mainWindow;
            ReadDocument();
            AddVersionButtons();
        }
        public void ReadDocument()
        {
            string documentText = File.ReadAllText(_document.FilePath);
            TextBox.Text = documentText;
        }

        public void AddVersionButtons()
        {
            // Отримати StackPanel за ім'ям
            StackPanel stackPanel = ButtonStackPanel;
            if (stackPanel != null)
            {
                stackPanel.Children.Clear();

                var documentId = _documentManager.GetDocumentsByName(_document.Name);
                var versions = _versionControlManager.GetVersionsByDocumentId(_document.Id);

                // Додати кнопки
                foreach (var version in versions) // Змінити кількість кнопок за потребою
                {
                    Button button = new Button
                    {
                        Content = version.CreationDate.ToString(),
                        Tag = version.VersionDescription,
                        Style = (Style)Application.Current.FindResource("RectangleButtonStyle"), // Стиль із ресурсів
                        Margin = new Thickness(0, 8, 0, 0), // Відступи
                        CommandParameter = version.Id
                    };

                    button.Click += OnButtonVersionClicked;

                    CreateContextMenuForButton(button);
                    stackPanel.Children.Add(button);
                }
            }
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
                MessageBox.Show($"Failed to open document in external editor. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            string documentText = File.ReadAllText(_document.FilePath);
            textForm.Text = documentText;
            AddVersionButtons();
        }

        private void CreateNewVersionButton_Click(object sender, RoutedEventArgs e)
        {
            string documentText = File.ReadAllText(_document.FilePath);
            textForm.Text = documentText;

            InputPopup inputPopup = new InputPopup();
            inputPopup.Title = "Description";
            inputPopup.TitleText.Text = "Enter version description:";
            inputPopup.ShowDialog();

            if (inputPopup.DialogResult == true)
            {
                string _versionDescription = inputPopup.MessageText.Text;
                bool versionCreated = _versionControlManager.CreateNewVersion(_document, _versionDescription);
                if (versionCreated)
                {
                    AddVersionButtons();
                    InfoPopup infoPopup = new InfoPopup(InfoPopupType.VersionCreatedSuccessfully);
                    infoPopup.ShowDialog();
                }
                else
                {
                    InfoPopup infoPopup = new InfoPopup(InfoPopupType.NoChangesDetected);
                    infoPopup.ShowDialog();
                }
            }
        }

        private void CreateContextMenuForButton(Button button)
        {
            ContextMenu contextMenu = new ContextMenu();

            MenuItem item1 = new MenuItem { Header = "Open" };
            item1.Click += Open_Click;

            MenuItem item2 = new MenuItem { Header = "Rename" };
            item2.Click += ChangeDescription_Click;

            MenuItem item3 = new MenuItem { Header = "Remove" };
            item3.Click += Delete_Click;

            contextMenu.Items.Add(item1);
            contextMenu.Items.Add(item2);
            contextMenu.Items.Add(item3);

            button.ContextMenu = contextMenu;
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

            var versionId = _versionControlManager.GetVersionById((int)clickedButton.CommandParameter);
            VersionDetailsPage versionDetailsWindow = new VersionDetailsPage(_mainWindow, versionId, _versionControlManager);
            NavigationService.Navigate(versionDetailsWindow);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                Button parentButton = contextMenu.PlacementTarget as Button;

                if (parentButton != null)
                {
                    int versionId = (int)parentButton.CommandParameter;

                    Database.Models.Version version = _versionControlManager.GetVersionById(versionId);

                    VersionDetailsPage versionDetailsWindow = new VersionDetailsPage(_mainWindow, version, _versionControlManager);
                    NavigationService.Navigate(versionDetailsWindow);
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

        private void Delete_Click(object sender, RoutedEventArgs e)
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

using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DocumentVersionControlSystem.UI.Windows
{
    /// <summary>
    /// Interaction logic for DocumentViewerPage.xaml
    /// </summary>
    public partial class DocumentViewerPage : Page
    {
        private MainWindow _mainWindow;

        private readonly VersionControlManager _versionControlManager;
        private Database.Models.Document _document;

        public DocumentViewerPage(MainWindow mainWindow, VersionControlManager versionControl, Database.Models.Document document)
        {
            _versionControlManager = versionControl;
            _document = document;
            _mainWindow = mainWindow;

            InitializeComponent();
            ReadDocument();

            _mainWindow.SizeChanged += OnWindowSizeChanged;

            UpdateButtonSizes(_mainWindow.ActualWidth, _mainWindow.ActualHeight);
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateButtonSizes(e.NewSize.Width, e.NewSize.Height);
        }

        private void UpdateButtonSizes(double width, double height)
        {
            OpenDocumentButton.Width = width * 0.06;
            OpenDocumentButton.Height = height * 0.033;
            OpenDocumentButton.Margin = new Thickness(width * 0.024 + 25 * 3 + 5, 0, 0, 0);

            CreateNewVersionButton.Width = width * 0.05;
            CreateNewVersionButton.Height = height * 0.033;
            CreateNewVersionButton.Margin = new Thickness(0, 0, width * 0.006 + 25 + 10, 0);

            RefreshButton.Width = width * 0.016;
            RefreshButton.Height = height * 0.03;
        }

        public void ReadDocument()
        {
            string documentText = File.ReadAllText(_document.FilePath);
            TextBox.Text = documentText;
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
            ReadDocument();
            _mainWindow.AddVersionButtons();
        }

        private void CreateNewVersionButton_Click(object sender, RoutedEventArgs e)
        {
            string documentText = File.ReadAllText(_document.FilePath);
            TextBox.Text = documentText;

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
                    _mainWindow.AddVersionButtons();
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
    }
}

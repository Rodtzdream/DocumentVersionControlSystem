using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DocumentVersionControlSystem.UI.Windows
{
    /// <summary>
    /// Interaction logic for VersionDetailsPage.xaml
    /// </summary>
    public partial class VersionDetailsPage : Page
    {
        MainWindow _mainWindow;

        Database.Models.Version _version;
        private readonly VersionControlManager _versionControlManager;

        public VersionDetailsPage(MainWindow mainWindow, Database.Models.Version version, VersionControlManager versionControlManager)
        {
            _version = version;
            _versionControlManager = versionControlManager;
            _mainWindow = mainWindow;

            InitializeComponent();
            ReadDocument();
            ReadDescription();
            ReadDateTime();
            _mainWindow.AddVersionButtons(_version.Id);

            Loaded += VersionDetailsPage_Loaded;
            _mainWindow.SizeChanged += OnWindowSizeChanged;
        }

        private void VersionDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButtonSizes(_mainWindow.ActualWidth, _mainWindow.ActualHeight);
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateButtonSizes(e.NewSize.Width, e.NewSize.Height);
        }

        private void UpdateButtonSizes(double width, double height)
        {
            ApplyVersionButton.Width = width * 0.06;
            ApplyVersionButton.Height = height * 0.033;
            ApplyVersionButton.FontSize = Math.Max(12, this.ActualWidth * 0.007);
        }

        public int GetVersionId()
        {
            return _version.Id;
        }

        private bool ReadDocument()
        {
            try
            {
                string documentVersionText = File.ReadAllText(_version.FilePath);
                TextBox textBox = (TextBox)FindName("TextBox");
                textBox.Text = documentVersionText;
            }
            catch (System.Exception)
            {
                return false;
                throw new System.Exception("Failed to read document version file.");
            }
            return true;
        }

        private void ReadDescription()
        {
            string descriptionText = _version.VersionDescription;
            TextBlock textBox = (TextBlock)FindName("VersionDescription");
            textBox.Text = "Version description: " + descriptionText;
            if(descriptionText == "")
                textBox.Text = "Version description: None";
        }

        private void ReadDateTime()
        {
            string dateTimeText = _version.CreationDate.ToString();
            TextBlock textBox = (TextBlock)FindName("VersionDateTime");
            textBox.Text = "Creation date & time: " + dateTimeText;
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

        private void ApplyVersionButton_Click(object sender, RoutedEventArgs e)
        {
            SelectSwitchOptionPopup popup = new SelectSwitchOptionPopup(_version.VersionDescription);
            popup.ShowDialog();

            if (popup.DialogResult == true)
            {
                if (popup.switchOption == SelectSwitchOptionPopup.SwitchOption.DeleteNewer)
                {
                    _versionControlManager.SwitchToVersionAndDeleteNewer(_version.DocumentId, _version.Id, popup.Description);
                }
                else if (popup.switchOption == SelectSwitchOptionPopup.SwitchOption.SaveAsTheLatest)
                {
                    _version = _versionControlManager.SwitchToVersionAndSaveAsLatest(_version.DocumentId, _version.Id, popup.Description);
                }
                RefreshWindow();
            }
        }
    }
}

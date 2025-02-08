using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.VersionControl;
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
            
            _mainWindow.SizeChanged += OnWindowSizeChanged;

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
        }

        public bool ReadDocument()
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

        public void ReadDescription()
        {
            string descriptionText = _version.VersionDescription;
            TextBlock textBox = (TextBlock)FindName("TextBlock");
            textBox.Text = "Description: " + descriptionText;
            if(descriptionText == "")
                textBox.Text = "Description: None";
        }

        public bool RefreshWindow()
        {
            if (!ReadDocument())
                return false;

            ReadDescription();
            _mainWindow.AddVersionButtons();
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

using System.Windows;

namespace DocumentVersionControlSystem.UI.Popups
{
    public enum InfoPopupType
    {
        VersionCreationCanceled,
        VersionCreatedSuccessfully,
        NoChangesDetected,
    }

    public partial class InfoPopup : Window
    {
        public InfoPopup(InfoPopupType popupType)
        {
            InitializeComponent();
            switch (popupType)
            {
                case InfoPopupType.VersionCreationCanceled:
                    TitleText.Text = "Version creation canceled";
                    MessageText.Text = "You have canceled the process of creating a new version. No changes have been saved.";
                    break;
                case InfoPopupType.VersionCreatedSuccessfully:
                    TitleText.Text = "Version created successfully";
                    MessageText.Text = "The new version of the document has been created and saved successfully.";
                    break;
                case InfoPopupType.NoChangesDetected:
                    TitleText.Text = "No changes detected";
                    MessageText.Text = "The selected file does not differ from the latest version. \nNo new version can be created.";
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

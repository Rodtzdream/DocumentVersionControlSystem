using System.Windows;

namespace DocumentVersionControlSystem.UI.Popups
{
    public enum InfoPopupType
    {
        VersionCreationCanceled,
        VersionCreatedSuccessfully,
        NoChangesDetected,
        LoadVersionFailed,
        LoadDocumentFailed,
        DocumentNotFound,
        DocumentAlreadyExistsInTheSystem,
        DocumentAlreadyExistsInTheRootPath,
        SameDocumentName,
        DocumentNameEmpty,
        DocumentNameTooLong,
        DocumentNameCannotEndWithTxt,
        DocumentNameContainsInvalidCharacters,
        SuccessfulDocumentRename,
        SuccessfulDocumentRemove,
        InvalidFileFormat
    }

    public partial class InfoPopup : Window
    {
        public InfoPopup()
        {
            InitializeComponent();
        }

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
                case InfoPopupType.LoadVersionFailed:
                    TitleText.Text = "Load version failed";
                    MessageText.Text = "Failed to load the version. Returning to the home page.";
                    break;
                case InfoPopupType.LoadDocumentFailed:
                    TitleText.Text = "Load version failed";
                    MessageText.Text = "Failed to load the document. Returning to the home page.";
                    break;
                case InfoPopupType.DocumentNotFound:
                    TitleText.Text = "Document not found";
                    MessageText.Text = "The document you are trying to access does not exist.";
                    break;
                case InfoPopupType.DocumentAlreadyExistsInTheSystem:
                    TitleText.Text = "Document already exists";
                    MessageText.Text = "A document with the same name already exists in the system.";
                    break;
                case InfoPopupType.DocumentAlreadyExistsInTheRootPath:
                    TitleText.Text = "Document already exists in the root path";
                    MessageText.Text = "A document with the same name already exists in the root path. Please choose a different name or move the original document.";
                    break;
                case InfoPopupType.SameDocumentName:
                    TitleText.Text = "Same document name";
                    MessageText.Text = "Name cannot be the same as the current one.";
                    break;
                case InfoPopupType.DocumentNameEmpty:
                    TitleText.Text = "Document name is empty";
                    MessageText.Text = "Please enter a name for the document.";
                    break;
                case InfoPopupType.DocumentNameTooLong:
                    TitleText.Text = "Document name is too long";
                    MessageText.Text = "The document name exceeds the maximum length of 50 characters.";
                    break;
                case InfoPopupType.DocumentNameCannotEndWithTxt:
                    TitleText.Text = "Document name cannot end with .txt";
                    MessageText.Text = "The document name cannot end with .txt. Please choose a different name.";
                    break;
                case InfoPopupType.DocumentNameContainsInvalidCharacters:
                    TitleText.Text = "Invalid characters";
                    MessageText.Text = "The document name contains invalid characters. Please use only letters, numbers, spaces, and the following special characters: - _ .";
                    break;
                case InfoPopupType.SuccessfulDocumentRename:
                    TitleText.Text = "Document renamed successfully";
                    MessageText.Text = "The document has been renamed successfully.";
                    break;
                case InfoPopupType.SuccessfulDocumentRemove:
                    TitleText.Text = "Document removed successfully";
                    MessageText.Text = "The document has been removed successfully.";
                    break;
                case InfoPopupType.InvalidFileFormat:
                    TitleText.Text = "Invalid file format";
                    MessageText.Text = "Invalid file format. Only .txt files are allowed.";
                    break;
            }
        }

        public InfoPopup(string title, string message)
        {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    OkButton_Click(sender, e);
                    break;
                case System.Windows.Input.Key.Escape:
                    this.Close();
                    break;
            }
        }
    }
}

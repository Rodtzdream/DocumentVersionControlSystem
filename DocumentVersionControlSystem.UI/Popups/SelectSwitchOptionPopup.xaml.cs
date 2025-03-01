using System.Windows;
namespace DocumentVersionControlSystem.UI.Popups;

public partial class SelectSwitchOptionPopup : Window
{
    public SwitchOption switchOption { get; set; }
    public string Description { get; private set; }

    public enum SwitchOption
    {
        DeleteNewer,
        SaveAsTheLatest
    }

    public SelectSwitchOptionPopup(string versionDescription)
    {
        Description = versionDescription;
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var popup = new InputPopup(Description)
        {
            TitleText = { Text = "Enter new version description:" }
        };
        popup.ShowDialog();

        if (popup.DialogResult == true)
        {
            Description = popup.MessageText.Text;
            DialogResult = true;
        }
        else
        {
            DialogResult = false;
            new InfoPopup(InfoPopupType.VersionCreationCanceled).ShowDialog();
        }
        Close();
    }

    private void DeleteNewer_Checked(object sender, RoutedEventArgs e)
    {
        switchOption = SwitchOption.DeleteNewer;
    }

    private void SaveAsTheLatest_Checked(object sender, RoutedEventArgs e)
    {
        switchOption = SwitchOption.SaveAsTheLatest;
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case System.Windows.Input.Key.Enter:
                OkButton_Click(sender, e);
                break;
            case System.Windows.Input.Key.Escape:
                DialogResult = false;
                Close();
                break;
        }
    }
}

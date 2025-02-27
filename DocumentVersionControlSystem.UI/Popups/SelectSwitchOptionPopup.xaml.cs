using System.Windows;
namespace DocumentVersionControlSystem.UI.Popups;


public partial class SelectSwitchOptionPopup : Window
{
    InputPopup InputPopup;
    public SwitchOption switchOption { get; set; }
    public string Description;

    public enum SwitchOption
    {
        DeleteNewer,
        SaveAsTheLatest
    }

    public SelectSwitchOptionPopup(string versionDescription)
    {
        this.Description = versionDescription;
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
        this.Close();
        InputPopup = new InputPopup(this.Description);
        InputPopup.TitleText.Text = "Enter version description:";
        InputPopup.ShowDialog();

        if (InputPopup.DialogResult == true)
            Description = InputPopup.MessageText.Text;
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
                this.DialogResult = false;
                this.Close();
                break;
        }
    }
}

using System.Windows;
namespace DocumentVersionControlSystem.UI.Popups;


public partial class InputPopup : Window
{
    public InputPopup(string _messageText = "")
    {
        InitializeComponent();
        this.MessageText.Text = _messageText;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
        this.Close();
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case System.Windows.Input.Key.Enter:
                ConfirmButton_Click(sender, e);
                break;
            case System.Windows.Input.Key.Escape:
                CancelButton_Click(sender, e);
                break;
        }
    }
}

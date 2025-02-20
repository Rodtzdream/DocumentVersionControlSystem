using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace DocumentVersionControlSystem.UI;

public partial class DocumentRecoveryWindow : Window
{
    public ObservableCollection<MissingDocumentViewModel> _missingDocuments;

    public DocumentRecoveryWindow(ObservableCollection<MissingDocumentViewModel> missedDocuments)
    {
        InitializeComponent();
        _missingDocuments = missedDocuments;
        MissingDocumentsList.ItemsSource = _missingDocuments;
    }

    private void MissingDocumentsList_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (MissingDocumentsList.View is GridView gridView)
        {
            double totalWidth = MissingDocumentsList.ActualWidth - SystemParameters.VerticalScrollBarWidth;

            gridView.Columns[0].Width = totalWidth * 0.3;
            gridView.Columns[1].Width = totalWidth * 0.35;
            gridView.Columns[2].Width = totalWidth * 0.35;
        }
    }

    private void MissingDocumentsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OpenFileDialogAndSetNewPath();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialogAndSetNewPath();
    }

    private void OpenFileDialogAndSetNewPath()
    {
        if (MissingDocumentsList.SelectedItem is MissingDocumentViewModel selectedDocument)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select New Document Path",
                Filter = "Text files (*.txt)|*.txt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedDocument.NewPath = openFileDialog.FileName;
            }
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (MissingDocumentsList.SelectedItem is MissingDocumentViewModel selectedDocument)
        {
            selectedDocument.NewPath = "<Removed>";
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    public class MissingDocumentViewModel : INotifyPropertyChanged
    {
        private string? _newPath;

        public string DocumentName { get; set; }
        public string OldPath { get; set; }

        public string? NewPath
        {
            get => _newPath;
            set
            {
                if (_newPath != value)
                {
                    _newPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

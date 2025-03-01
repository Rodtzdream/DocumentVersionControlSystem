using DocumentVersionControlSystem.UI.Popups;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DocumentVersionControlSystem.UI;

public partial class DocumentRecoveryWindow : Window
{
    public ObservableCollection<MissingDocumentViewModel> _missingDocuments;
    private readonly InfoPopup _infoPopup;

    // Constructor
    public DocumentRecoveryWindow(ObservableCollection<MissingDocumentViewModel> missedDocuments)
    {
        InitializeComponent();
        _missingDocuments = missedDocuments;
        _infoPopup = new InfoPopup(InfoPopupType.InvalidFileFormat);
        MissingDocumentsList.ItemsSource = _missingDocuments;
    }

    // Event handlers
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

    private void MissingDocumentsList_DragOver(object sender, DragEventArgs e)
    {
        Point position = e.GetPosition(MissingDocumentsList);

        var item = GetListViewItemAt(position);

        if (item != null)
        {
            MissingDocumentsList.SelectedItem = item.DataContext;
        }
    }

    private void MissingDocumentsList_DragLeave(object sender, DragEventArgs e)
    {
        MissingDocumentsList.SelectedItem = null;
    }

    private void MissingDocumentsList_Drop(object sender, DragEventArgs e)
    {
        if (MissingDocumentsList.SelectedItem is MissingDocumentViewModel selectedDocument)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1 && Path.GetExtension(files[0]).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    selectedDocument.NewPath = files[0];
                }
                else
                {
                    ShowInvalidFileFormatPopup();
                }
            }
            else
            {
                ShowInvalidFileFormatPopup();
            }
        }
    }

    // Support methods
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

    private ListViewItem? GetListViewItemAt(Point position)
    {
        HitTestResult hitTestResult = VisualTreeHelper.HitTest(MissingDocumentsList, position);
        DependencyObject obj = hitTestResult.VisualHit;
        while (obj != null && obj is not ListViewItem)
        {
            obj = VisualTreeHelper.GetParent(obj);
        }
        return obj as ListViewItem;
    }

    private void ShowInvalidFileFormatPopup()
    {
        _infoPopup.Show();
    }

    // Internal class
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

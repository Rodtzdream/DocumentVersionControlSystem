using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.DocumentManagement;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DocumentVersionControlSystem.UI.Windows
{
    /// <summary>
    /// Interaction logic for DocumentViewerWindow.xaml
    /// </summary>
    public partial class DocumentViewerWindow : Window
    {
        private readonly DocumentManager _documentManager;
        private readonly VersionControlManager _versionControlManager;
        private Database.Models.Document _document;

        private TextBox textForm;

        public DocumentViewerWindow(DocumentManager documentManagement, VersionControlManager versionControl, Database.Models.Document document)
        {
            _documentManager = documentManagement;
            _versionControlManager = versionControl;
            _document = document;

            InitializeComponent();
            ReadDocument();
            AddVersionButtons();
        }
        public void ReadDocument()
        {
            string documentText = File.ReadAllText(_document.FilePath);
            textForm = (TextBox)FindName("TextBox");
            textForm.Text = documentText;
        }

        public void AddVersionButtons()
        {
            // Отримати StackPanel за ім'ям
            StackPanel stackPanel = (StackPanel)FindName("ButtonStackPanel");
            if (stackPanel != null)
            {
                stackPanel.Children.Clear();

                var documentId = _documentManager.GetDocumentsByName(_document.Name);
                var versions = _versionControlManager.GetVersionsByDocumentId(_document.Id);

                // Додати кнопки
                foreach (var version in versions) // Змінити кількість кнопок за потребою
                {
                    Button button = new Button
                    {
                        Content = version.CreationDate,
                        Tag = version.VersionDescription,
                        Style = (Style)FindResource("RectangleButtonStyle"), // Стиль із ресурсів
                        Margin = new Thickness(0, 8, 0, 0) // Відступи
                    };

                    stackPanel.Children.Add(button);
                }
            }
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
            string documentText = File.ReadAllText(_document.FilePath);
            textForm.Text = documentText;
        }
    }
}

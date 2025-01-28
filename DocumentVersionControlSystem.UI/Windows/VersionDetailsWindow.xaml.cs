using DocumentVersionControlSystem.Database.Models;
using DocumentVersionControlSystem.DocumentManagement;
using DocumentVersionControlSystem.UI.Popups;
using DocumentVersionControlSystem.VersionControl;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DocumentVersionControlSystem.UI.Windows
{
    /// <summary>
    /// Interaction logic for VersionDetailsWindow.xaml
    /// </summary>
    public partial class VersionDetailsWindow : Window
    {
        Button _selectedVersionButton;
        Database.Models.Version _version;
        private readonly VersionControlManager _versionControlManager;

        public VersionDetailsWindow(Database.Models.Version version, VersionControlManager versionControlManager)
        {
            _version = version;
            _versionControlManager = versionControlManager;

            InitializeComponent();
            ReadDocument();
            ReadDescription();
            AddVersionButtons();
        }

        public void ReadDocument()
        {
            string documentVersionText = File.ReadAllText(_version.FilePath);
            TextBox textBox = (TextBox)FindName("TextBox");
            textBox.Text = documentVersionText;
        }

        public void ReadDescription()
        {
            string descriptionText = _version.VersionDescription;
            TextBlock textBox = (TextBlock)FindName("TextBlock");
            textBox.Text = descriptionText;
        }

        public void AddVersionButtons()
        {
            // Отримати StackPanel за ім'ям
            StackPanel stackPanel = (StackPanel)FindName("ButtonStackPanel");
            if (stackPanel != null)
            {
                stackPanel.Children.Clear();

                var documentId = _version.DocumentId;
                var versions = _versionControlManager.GetVersionsByDocumentId(documentId);

                // Додати кнопки
                foreach (var version in versions) // Змінити кількість кнопок за потребою
                {
                    Button button = new Button
                    {
                        Content = version.CreationDate.ToString(),
                        Tag = version.VersionDescription,
                        Style = (Style)FindResource("RectangleButtonStyle"), // Стиль із ресурсів
                        Margin = new Thickness(0, 8, 0, 0), // Відступи
                        CommandParameter = version.Id
                    };

                    if (version.Id == _version.Id)
                    {
                        button.BorderBrush = Brushes.Gray;
                        button.BorderThickness = new Thickness(3);
                        _selectedVersionButton = button;
                    }

                    button.Click += OnButtonVersionClicked;

                    CreateContextMenuForVersionButton(button);
                    stackPanel.Children.Add(button);
                }
            }
        }

        private void CreateContextMenuForVersionButton(Button button)
        {
            ContextMenu contextMenu = new ContextMenu();

            MenuItem item1 = new MenuItem { Header = "Open" };
            item1.Click += OpenVersion_Click;

            MenuItem item2 = new MenuItem { Header = "Change description" };
            item2.Click += ChangeDescription_Click;

            MenuItem item3 = new MenuItem { Header = "Delete" };
            item3.Click += DeleteVersion_Click;

            contextMenu.Items.Add(item1);
            contextMenu.Items.Add(item2);
            contextMenu.Items.Add(item3);

            button.ContextMenu = contextMenu;
        }

        private void RefreshWindow()
        {
            ReadDocument();
            ReadDescription();
            AddVersionButtons();
        }

        private void OnButtonVersionClicked(object sender, RoutedEventArgs e)
        {
            if (_selectedVersionButton != null)
            {
                _selectedVersionButton.ClearValue(Button.BorderBrushProperty);
                _selectedVersionButton.ClearValue(Button.BorderThicknessProperty);
            }

            Button clickedButton = sender as Button;
            _selectedVersionButton = clickedButton;
            clickedButton.BorderBrush = Brushes.Gray;
            clickedButton.BorderThickness = new Thickness(3);

            _version = _versionControlManager.GetVersionById((int)clickedButton.CommandParameter);

            ReadDocument();
            ReadDescription();
        }

        private void OpenVersion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                Button parentButton = contextMenu.PlacementTarget as Button;

                if (parentButton != null)
                {
                    int versionId = (int)parentButton.CommandParameter;

                    _version = _versionControlManager.GetVersionById(versionId);

                    ReadDocument();
                    ReadDescription();
                }
            }
        }

        private void ChangeDescription_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                Button parentButton = contextMenu.PlacementTarget as Button;

                if (parentButton != null)
                {
                    InputPopup popup = new InputPopup();
                    popup.TitleText.Text = "Change version description";
                    popup.MessageText.Text = parentButton.Tag.ToString();
                    popup.ShowDialog();

                    string newName = popup.MessageText.Text;
                    _versionControlManager.ChangeVersionDescription((int)parentButton.CommandParameter, newName);
                    AddVersionButtons();
                }
            }
        }

        private void DeleteVersion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                Button parentButton = contextMenu.PlacementTarget as Button;

                if (parentButton != null)
                {
                    var version = _versionControlManager.GetVersionById((int)parentButton.CommandParameter);
                    _versionControlManager.DeleteVersion(version);
                    AddVersionButtons();
                }
            }
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

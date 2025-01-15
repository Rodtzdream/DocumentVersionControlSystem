using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for VersionHistoryWindow.xaml
    /// </summary>
    public partial class VersionHistoryWindow : Window
    {
        public VersionHistoryWindow()
        {
            InitializeComponent();
            ReadDocument();
            ReadDescription();
            AddVersionButtons();
        }
        public void ReadDocument()
        {
            string documentText = "Document text goes here";
            TextBox textBox = (TextBox)FindName("TextBox");
            textBox.Text = documentText;
        }

        public void ReadDescription()
        {
            string descriptionText = "Description text goes here";
            TextBlock textBox = (TextBlock)FindName("TextBlock");
            textBox.Text = descriptionText;
        }

        public void AddVersionButtons()
        {
            // Отримати StackPanel за ім'ям
            StackPanel stackPanel = (StackPanel)FindName("ButtonStackPanel");

            if (stackPanel != null)
            {
                // Додати кнопки
                for (int i = 1; i <= 10; i++) // Змінити кількість кнопок за потребою
                {
                    Button button = new Button
                    {
                        Content = $"Version {i}",
                        Tag = "Short Description",
                        Style = (Style)FindResource("RectangleButtonStyle"), // Стиль із ресурсів
                        Margin = new Thickness(0, 8, 0, 0) // Відступи
                    };

                    stackPanel.Children.Add(button);
                }
            }
        }
    }
}

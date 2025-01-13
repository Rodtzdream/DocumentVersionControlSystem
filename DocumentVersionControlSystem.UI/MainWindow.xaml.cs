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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DocumentVersionControlSystem.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AddButtonsToGrid();
            AddVersionButtonsToGrid();
        }

        private void AddButtonsToGrid()
        {
            int totalButtons = 12; // Кількість кнопок
            int columns = ButtonGrid.ColumnDefinitions.Count; // Кількість стовпців
            int rows = ButtonGrid.RowDefinitions.Count; // Кількість рядків

            // Створення кнопки
            Button button = new Button
            {
                Content = $"Add document",
                Style = (Style)FindResource("AddButtonStyle"), // Застосування стилю
                Margin = new Thickness(0, 6, 0, 0)
            };

            // Додавання кнопки до сітки
            Grid.SetColumn(button, 0);
            Grid.SetRow(button, 0);
            ButtonGrid.Children.Add(button);

            for (int i = 1; i < totalButtons; i++)
            {
                // Створення кнопки
                button = new Button
                {
                    Content = $"Document {i}",
                    Tag = $"Document {i}",
                    Style = (Style)FindResource("SquareButtonStyle"), // Застосування стилю
                    Margin = new Thickness(0, i < columns ? 6 : 8, 0, 0)
                };

                // Обчислення стовпця та рядка для кнопки
                int column = i % columns;
                int row = i / columns;

                // Додавання кнопки до сітки
                Grid.SetColumn(button, column);
                Grid.SetRow(button, row);
                ButtonGrid.Children.Add(button);
            }
        }

        public void AddVersionButtonsToGrid()
        {
            // Отримати StackPanel за ім'ям
            StackPanel stackPanel = (StackPanel)FindName("ButtonStackPanel");

            if (stackPanel != null)
            {
                // Додати кнопки
                for (int i = 1; i <= 3; i++) // Змінити кількість кнопок за потребою
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

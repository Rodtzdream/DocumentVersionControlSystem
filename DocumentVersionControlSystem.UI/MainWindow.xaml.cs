using DocumentVersionControlSystem.UI.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DocumentVersionControlSystem.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button _selectedButton;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDynamicGrid();
            AdjustGridLayout(120);
            AddVersionButtons();
            DocumentDetailsWindow documentDetailsWindow = new DocumentDetailsWindow();
            documentDetailsWindow.Show();
            VersionHistoryWindow versionHistoryWindow = new VersionHistoryWindow();
            versionHistoryWindow.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustGridLayout(120); // Перераховує сітку після того, як вікно завантажене
        }

        private void InitializeDynamicGrid()
        {
            // Прив'язка події зміни розміру вікна
            this.SizeChanged += OnWindowSizeChanged;
            this.StateChanged += MainWindow_StateChanged;
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Перерахунок сітки при зміні розміру
            AdjustGridLayout(120);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized || WindowState == WindowState.Normal)
            {
                // Перерахунок сітки тільки при зміні стану на нормальний чи повноекранний
                AdjustGridLayout(120);
            }
        }

        private void AdjustGridLayout(int totalButtons)
        {
            // Отримуємо ширину і висоту контейнера
            double windowWidth = this.ActualWidth - 350;
            double windowHeight = this.ActualHeight - 32;

            // Визначаємо кількість стовпців і рядків на основі розміру вікна
            int columns = (int)(windowWidth / 100);
            int rows = totalButtons - columns;

            // Якщо кількість стовпців або рядків менша ніж 1, робимо їх мінімум 1
            columns = Math.Max(columns, 1);
            rows = Math.Max(rows, 1);

            // Очищаємо існуючі стовпці і рядки
            ButtonGrid.ColumnDefinitions.Clear();
            ButtonGrid.RowDefinitions.Clear();

            // Додаємо нові стовпці і рядки
            for (int i = 0; i < columns; i++)
            {
                ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < rows; i++)
            {
                ButtonGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Додаємо кнопки в сітку
            AddButtonsToGrid(columns, rows);
        }

        private void AddButtonsToGrid(int columns, int rows)
        {
            // Очистити старі кнопки
            ButtonGrid.Children.Clear();

            int totalButtons = 10; // Кількість кнопок

            Button button = CreateButton($"Add document", "AddButtonStyle");
            button.Click += OnButtonClicked;

            // Додавання кнопки до сітки
            Grid.SetColumn(button, 0);
            Grid.SetRow(button, 0);
            ButtonGrid.Children.Add(button);

            for (int i = 1; i <= totalButtons; i++)
            {
                // Створення кнопки
                button = CreateButton($"Document {i}", "SquareButtonStyle");
                button.Tag = $"Document {i}";
                button.Click += OnButtonClicked;

                // Обчислення стовпця та рядка для кнопки
                int column = i % columns;
                int row = i / columns;

                // Додавання кнопки до сітки
                Grid.SetColumn(button, column);
                Grid.SetRow(button, row);
                ButtonGrid.Children.Add(button);
            }
        }

        private Button CreateButton(string content, string styleKey)
        {
            return new Button
            {
                Content = content,
                Style = (Style)FindResource(styleKey),
                Margin = new Thickness(5)
            };
        }

        private void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_selectedButton != null)
            {
                _selectedButton.ClearValue(Button.BorderBrushProperty);
                _selectedButton.ClearValue(Button.BorderThicknessProperty);
            }

            Button clickedButton = sender as Button;
            _selectedButton = clickedButton;
            clickedButton.BorderBrush = Brushes.Gray;
            clickedButton.BorderThickness = new Thickness(3);
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

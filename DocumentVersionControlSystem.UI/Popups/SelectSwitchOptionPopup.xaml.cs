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

namespace DocumentVersionControlSystem.UI.Popups
{
    /// <summary>
    /// Interaction logic for SelectSwitchOptionPopup.xaml
    /// </summary>
    public partial class SelectSwitchOptionPopup : Window
    {
        EnterDescriptionPopup enterDescriptionPopup;
        public SelectSwitchOptionPopup()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            enterDescriptionPopup = new EnterDescriptionPopup();
            enterDescriptionPopup.ShowDialog();
        }
    }
}

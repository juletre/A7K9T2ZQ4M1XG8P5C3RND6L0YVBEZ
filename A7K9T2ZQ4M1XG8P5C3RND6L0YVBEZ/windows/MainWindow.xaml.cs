using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.company_selection;
using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.settlements;
using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.support;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int pos_type = 1;
        int aspect_ratio = 1;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenPos_Click(object sender, RoutedEventArgs e)
        {
            if (pos_type == 1 && aspect_ratio == 1)
                new pos_hospitality_16x9().Show();
            else if (pos_type == 1 && aspect_ratio == 2)
                new pos_hospitality_4x3().Show();
            else if (pos_type == 2 && aspect_ratio == 1)
                new pos_store_16x9().Show();
            else if (pos_type == 2 && aspect_ratio == 2)
                new pos_store_4x3().Show();
            else
                MessageBox.Show("Error");
        }
        private void OpenAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (aspect_ratio == 1)
                new admin_16x9().Show();
            else if (aspect_ratio == 2)
                new admin_4x3().Show();
            else
                MessageBox.Show("Error");
        }
        private void Open_Settlement(object sender, RoutedEventArgs e)
        {
            new settlement_menu().Show();
        }
        private void Open_Support(object sender, RoutedEventArgs e)
        {
            new support().Show();
        }
        private void Open_Salespoint_Selector(object sender, RoutedEventArgs e)
        {
            new salespoint_selector().Show();
        }
    }
}

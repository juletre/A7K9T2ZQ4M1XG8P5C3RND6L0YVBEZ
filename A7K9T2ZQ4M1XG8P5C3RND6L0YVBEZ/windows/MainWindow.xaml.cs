using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.company_selection;
using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.settlements;
using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.support;
using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.pos_hospitality;
using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.pos_store;
using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.administration;
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
            if (pos_type == 1)
                new pos_hospitality().Show();
            else if (pos_type == 2)
                new pos_store().Show();
            else
                MessageBox.Show("Error");
        }
        private void OpenAdmin_Click(object sender, RoutedEventArgs e)
        {
                new admin_window().Show();
        }
        private void Open_Settlement(object sender, RoutedEventArgs e)
        {
            new settlement_menu().Show();
        }
        private void Open_Support(object sender, RoutedEventArgs e)
        {
            new support_window().Show();
        }
        private void Open_Salespoint_Selector(object sender, RoutedEventArgs e)
        {
            new salespoint_selector().Show();
        }
    }
}

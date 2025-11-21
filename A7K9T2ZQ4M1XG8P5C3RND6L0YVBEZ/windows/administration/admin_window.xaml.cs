using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.Helpers;
using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.administration;
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

namespace A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.administration
{
    /// <summary>
    /// Interaction logic for admin_window.xaml
    /// </summary>
    public partial class admin_window : Window
    {
        
        public admin_window()
        {
            InitializeComponent();

            SelectAndFillMonitor.ShowFullScreenOnConfiguredScreen(this);

            functionButton10.Content = "EXIT";
        }

        private void functionButton10_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}

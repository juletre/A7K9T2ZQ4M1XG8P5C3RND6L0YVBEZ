using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ
{
    /// <summary>
    /// Interaction logic for pos_hospitality_16x9.xaml
    /// </summary>
    public partial class pos_hospitality_16x9 : Window
    {
        public ObservableCollection<OrderItem> OrderItems { get; set; }

        public pos_hospitality_16x9()
        {
            InitializeComponent();
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;

            OrderItems = new ObservableCollection<OrderItem>
            {
                new OrderItem { Product = "Coffee", Quantity = 1, Price = 29.00 },
                new OrderItem { Product = "Sandwich", Quantity = 2, Price = 89.00 },
                new OrderItem { Product = "Juice", Quantity = 1, Price = 45.00 }
            };

            OrderList.ItemsSource = OrderItems;
            UpdateTotal();

            // Når listen endres (legg til/fjern), oppdater totalsummen
            OrderItems.CollectionChanged += (s, e) => UpdateTotal();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is OrderItem item)
            {
                OrderItems.Remove(item);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            double total = OrderItems.Sum(i => i.Price * i.Quantity);
            TotalText.Text = total.ToString("0.00");
        }
    }

    public class OrderItem
    {
        public string Product { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}

using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.pos_store
{
    /// <summary>
    /// Interaction logic for pos_store.xaml
    /// </summary>
    public partial class pos_store : Window, INotifyPropertyChanged
    {
        public ObservableCollection<string> Categories { get; } = new();
        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<OrderLine> OrderLines { get; } = new();

        public ICollectionView ProductsView { get; private set; }

        private string? selectedCategory;
        public string? SelectedCategory
        {
            get => selectedCategory;
            set
            {
                if (selectedCategory != value)
                {
                    selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                    ProductsView.Refresh();
                }
            }
        }

        private decimal subtotal;
        public decimal Subtotal
        {
            get => subtotal;
            private set
            {
                subtotal = value;
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        private decimal tax;
        public decimal Tax
        {
            get => tax;
            private set
            {
                tax = value;
                OnPropertyChanged(nameof(Tax));
            }
        }

        private decimal total;
        public decimal Total
        {
            get => total;
            private set
            {
                total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public pos_store()
        {
            InitializeComponent();

            SelectAndFillMonitor.ShowFullScreenOnConfiguredScreen(this);

            DataContext = this;

            Categories.Add("Alle varer");
            Categories.Add("Drikke");
            Categories.Add("Snacks");
            Categories.Add("Dagligvarer");
            Categories.Add("Helse");
            Categories.Add("Husholdning");
            Categories.Add("Gavekort");

            Products.Add(new Product("200001", "Kaffe latte", "Drikke", 39.00m));
            Products.Add(new Product("200002", "Iskald te", "Drikke", 32.00m));
            Products.Add(new Product("200003", "Smoothie", "Drikke", 45.00m));
            Products.Add(new Product("200004", "Mineralvann", "Drikke", 28.00m));
            Products.Add(new Product("200005", "Potetgull", "Snacks", 35.00m));
            Products.Add(new Product("200006", "Nøtteblanding", "Snacks", 42.00m));
            Products.Add(new Product("200007", "Sjokolade", "Snacks", 29.00m));
            Products.Add(new Product("200008", "Kjeks", "Snacks", 25.00m));
            Products.Add(new Product("200009", "Brød", "Dagligvarer", 32.00m));
            Products.Add(new Product("200010", "Melk", "Dagligvarer", 24.00m));
            Products.Add(new Product("200011", "Egg", "Dagligvarer", 45.00m));
            Products.Add(new Product("200012", "Yoghurt", "Dagligvarer", 21.00m));
            Products.Add(new Product("200013", "Håndkrem", "Helse", 55.00m));
            Products.Add(new Product("200014", "Plaster", "Helse", 29.00m));
            Products.Add(new Product("200015", "Vitaminer", "Helse", 89.00m));
            Products.Add(new Product("200016", "Gavekort 200", "Gavekort", 200.00m));

            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProducts;

            SelectedCategory = Categories.First();
            CategoryList.SelectedIndex = 0;

            OrderLines.CollectionChanged += (_, __) => UpdateTotals();
            UpdateTotals();
        }

        private bool FilterProducts(object item)
        {
            if (item is not Product product)
            {
                return false;
            }

            if (SelectedCategory == "Alle varer" || string.IsNullOrWhiteSpace(SelectedCategory))
            {
                return true;
            }

            return product.Category == SelectedCategory;
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryList.SelectedItem is string category)
            {
                SelectedCategory = category;
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not Product product)
            {
                return;
            }

            SubmitCommand(product.Code);
        }

        private void RemoveLine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is OrderLine line)
            {
                OrderLines.Remove(line);
                UpdateTotals();
            }
        }

        private void ClearOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderLines.Clear();
            UpdateTotals();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || sender is not TextBox input)
            {
                return;
            }

            ExecuteCommand(input.Text);
            input.Clear();
            e.Handled = true;
        }

        private void SubmitCommand(string command)
        {
            if (CommandInput == null)
            {
                return;
            }

            CommandInput.Text = command;
            CommandInput.CaretIndex = CommandInput.Text.Length;
            ExecuteCommand(CommandInput.Text);
            CommandInput.Clear();
        }

        private void ExecuteCommand(string input)
        {
            var trimmed = input.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return;
            }

            if (trimmed.StartsWith("%"))
            {
                MessageBox.Show($"Kommando mottatt: {trimmed}");
                return;
            }

            if (trimmed.All(char.IsDigit))
            {
                HandleNumericCommand(trimmed);
                return;
            }

            MessageBox.Show("Ugyldig kommandoformat.");
        }

        private void HandleNumericCommand(string code)
        {
            var product = Products.FirstOrDefault(item => item.Code == code);
            if (product == null)
            {
                MessageBox.Show($"Fant ingen vare for nummer {code}.");
                return;
            }

            AddProductToOrder(product);
        }

        private void AddProductToOrder(Product product)
        {
            var existing = OrderLines.FirstOrDefault(line => line.Product == product.Name);
            if (existing != null)
            {
                existing.Quantity += 1;
                existing.LineTotal = existing.Quantity * existing.UnitPrice;
            }
            else
            {
                OrderLines.Add(new OrderLine
                {
                    Product = product.Name,
                    Quantity = 1,
                    UnitPrice = product.Price,
                    LineTotal = product.Price
                });
            }

            UpdateTotals();
        }

        private void UpdateTotals()
        {
            Subtotal = OrderLines.Sum(line => line.LineTotal);
            Tax = Subtotal * 0.25m;
            Total = Subtotal + Tax;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Product
    {
        public Product(string code, string name, string category, decimal price)
        {
            Code = code;
            Name = name;
            Category = category;
            Price = price;
        }

        public string Code { get; }
        public string Name { get; }
        public string Category { get; }
        public decimal Price { get; }
    }

    public class OrderLine : INotifyPropertyChanged
    {
        private int quantity;
        private decimal unitPrice;
        private decimal lineTotal;

        public string Product { get; set; } = string.Empty;

        public int Quantity
        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }
        }

        public decimal UnitPrice
        {
            get => unitPrice;
            set
            {
                if (unitPrice != value)
                {
                    unitPrice = value;
                    OnPropertyChanged(nameof(UnitPrice));
                }
            }
        }

        public decimal LineTotal
        {
            get => lineTotal;
            set
            {
                if (lineTotal != value)
                {
                    lineTotal = value;
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

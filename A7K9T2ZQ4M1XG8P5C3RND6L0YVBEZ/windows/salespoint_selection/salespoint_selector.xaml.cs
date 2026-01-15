using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.company_selection
{
    /// <summary>
    /// Interaction logic for salespoint_selector.xaml
    /// </summary>
    public partial class salespoint_selector : Window, INotifyPropertyChanged
    {
        public ObservableCollection<Salespoint> Salespoints { get; } = new();

        private Salespoint? selectedSalespoint;
        public Salespoint? SelectedSalespoint
        {
            get => selectedSalespoint;
            set
            {
                if (selectedSalespoint != value)
                {
                    selectedSalespoint = value;
                    OnPropertyChanged(nameof(SelectedSalespoint));
                }
            }
        }

        public salespoint_selector()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += async (_, __) => await LoadSalespointsAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenSalespoint_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSalespoint == null)
            {
                MessageBox.Show("Velg et salgssted før du fortsetter.");
                return;
            }

            MessageBox.Show($"Åpner {SelectedSalespoint.Name}...");
            Close();
        }

        private async Task LoadSalespointsAsync()
        {
            Salespoints.Clear();

            if (App.LicenseService == null)
            {
                AddFallbackSalespoints();
                return;
            }

            var companies = await App.LicenseService.GetLicensedCompaniesAsync();
            if (!companies.Any())
            {
                AddFallbackSalespoints();
                return;
            }

            foreach (var company in companies)
            {
                Salespoints.Add(new Salespoint(
                    company.CompanyName,
                    company.OrgNumber,
                    "Firma",
                    "Ikke satt",
                    "Lisensiert"));
            }

            SelectedSalespoint = Salespoints.FirstOrDefault();
        }

        private void AddFallbackSalespoints()
        {
            Salespoints.Add(new Salespoint("Eventlink Storgata", "Oslo sentrum", "Butikk", "Terminal 03", "Online"));
            Salespoints.Add(new Salespoint("Eventlink Scene", "Bergen sentrum", "Servering", "Terminal 02", "Online"));
            Salespoints.Add(new Salespoint("Eventlink Popup", "Trondheim", "Popup", "Terminal 01", "Klar"));
            Salespoints.Add(new Salespoint("Eventlink Outlet", "Stavanger", "Lager", "Terminal 05", "Vedlikehold"));

            SelectedSalespoint = Salespoints.FirstOrDefault();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Salespoint
    {
        public Salespoint(string name, string location, string type, string terminalId, string status)
        {
            Name = name;
            Location = location;
            Type = type;
            TerminalId = terminalId;
            Status = status;
        }

        public string Name { get; }
        public string Location { get; }
        public string Type { get; }
        public string TerminalId { get; }
        public string Status { get; }
    }
}

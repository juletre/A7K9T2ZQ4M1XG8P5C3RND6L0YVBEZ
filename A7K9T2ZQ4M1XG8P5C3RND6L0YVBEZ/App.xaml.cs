using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.services;
using System;
using System.Configuration;
using System.Windows;

namespace A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static LicenseService? LicenseService { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var posConnection = ConfigurationManager.ConnectionStrings["PosDatabase"]?.ConnectionString ?? string.Empty;
            var licenseConnection = ConfigurationManager.ConnectionStrings["LicenseDatabase"]?.ConnectionString ?? string.Empty;
            var licenseNumber = ConfigurationManager.AppSettings["LicenseNumber"] ?? string.Empty;

            try
            {
                if (!string.IsNullOrWhiteSpace(posConnection))
                {
                    var posInitializer = new DatabaseInitializer(posConnection);
                    posInitializer.EnsurePosTablesAsync().GetAwaiter().GetResult();
                }
                else
                {
                    MessageBox.Show("Manglende tilkobling til POS-database. Oppdater App.config.");
                }

                if (!string.IsNullOrWhiteSpace(licenseConnection))
                {
                    LicenseService = new LicenseService(licenseConnection, licenseNumber);
                    LicenseService.InitializeAsync().GetAwaiter().GetResult();
                }
                else
                {
                    MessageBox.Show("Manglende tilkobling til lisensdatabase. Oppdater App.config.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oppstart feilet: {ex.Message}");
            }
        }
    }
}

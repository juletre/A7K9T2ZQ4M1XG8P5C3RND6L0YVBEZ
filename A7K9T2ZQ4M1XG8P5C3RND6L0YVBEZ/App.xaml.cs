using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.services;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
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

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();

            _ = InitializeServicesAsync();
        }

        private static async Task InitializeServicesAsync()
        {
            var posConnection = ConfigurationManager.ConnectionStrings["PosDatabase"]?.ConnectionString ?? string.Empty;
            var licenseConnection = ConfigurationManager.ConnectionStrings["LicenseDatabase"]?.ConnectionString ?? string.Empty;
            var licenseNumber = ConfigurationManager.AppSettings["LicenseNumber"] ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(posConnection))
            {
                try
                {
                    var posInitializer = new DatabaseInitializer(posConnection);
                    await posInitializer.EnsurePosTablesAsync();
                    await posInitializer.EnsureSeedCompanyAsync(
                        licenseNumber,
                        "999999999",
                        "EVPOS Testfirma");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Pos-initialisering feilet: {ex.Message}");
                }
            }

            LicenseService = new LicenseService(licenseConnection, licenseNumber);
            await LicenseService.InitializeAsync();
        }
    }
}

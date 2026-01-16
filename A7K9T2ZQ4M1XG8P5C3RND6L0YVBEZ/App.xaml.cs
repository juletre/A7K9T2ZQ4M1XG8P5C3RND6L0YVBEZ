using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.services;
using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<Type, Window> OpenWindows = new();

        public static bool TryActivateWindow<T>() where T : Window
        {
            if (OpenWindows.TryGetValue(typeof(T), out var window) && window.IsVisible)
            {
                window.Activate();
                window.Focus();
                return true;
            }

            return false;
        }

        public static void ShowSingleWindow<T>(Func<T> factory) where T : Window
        {
            if (TryActivateWindow<T>())
            {
                return;
            }

            var window = factory();
            RegisterWindow(window);
            window.Show();
        }

        public static void RegisterWindow(Window window)
        {
            var type = window.GetType();
            OpenWindows[type] = window;
            window.Closed += (_, __) => OpenWindows.Remove(type);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            RegisterWindow(mainWindow);
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

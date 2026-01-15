using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.services
{
    public class LicenseService
    {
        private readonly string connectionString;
        private readonly string licenseNumber;
        private readonly DispatcherTimer refreshTimer;

        // TODO: Replace direct SQL validation with a secure API/gateway to avoid exposing license data.
        public LicenseStatus CurrentStatus { get; private set; } = LicenseStatus.Unvalidated();

        public int ActivePosWindows { get; private set; }

        public LicenseService(string connectionString, string licenseNumber)
        {
            this.connectionString = connectionString;
            this.licenseNumber = licenseNumber;

            refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            refreshTimer.Tick += async (_, __) => await RefreshAsync();
        }

        public async Task InitializeAsync()
        {
            var initializer = new DatabaseInitializer(connectionString);
            await initializer.EnsureLicenseTablesAsync();

            await RefreshAsync();
            refreshTimer.Start();
        }

        public bool TryRegisterPosWindow(Window window, out string message)
        {
            message = string.Empty;

            if (!CurrentStatus.IsValid)
            {
                message = CurrentStatus.Message;
                return false;
            }

            if (ActivePosWindows >= CurrentStatus.MaxConcurrentPos)
            {
                message = $"Lisensen tillater kun {CurrentStatus.MaxConcurrentPos} samtidige kassevinduer.";
                return false;
            }

            ActivePosWindows += 1;
            window.Closed += (_, __) => ActivePosWindows = Math.Max(0, ActivePosWindows - 1);
            _ = ReportUsageAsync();

            return true;
        }

        public async Task<IReadOnlyList<LicensedCompany>> GetLicensedCompaniesAsync()
        {
            var companies = new List<LicensedCompany>();

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = @"
SELECT c.OrgNumber, c.CompanyName, c.ContactEmail
FROM dbo.LicenseCompanies c
INNER JOIN dbo.Licenses l ON l.Id = c.LicenseId
WHERE l.LicenseNumber = @LicenseNumber
ORDER BY c.CompanyName";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@LicenseNumber", licenseNumber);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                companies.Add(new LicensedCompany(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2)));
            }

            return companies;
        }

        private async Task RefreshAsync()
        {
            try
            {
                var status = await FetchLicenseAsync();
                CurrentStatus = status;
                await ReportUsageAsync();
            }
            catch (Exception ex)
            {
                CurrentStatus = LicenseStatus.Invalid($"Kunne ikke validere lisens: {ex.Message}");
            }
        }

        private async Task<LicenseStatus> FetchLicenseAsync()
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return LicenseStatus.Invalid("Manglende tilkobling til lisensdatabase.");
            }

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = @"
SELECT LicenseNumber, MaxCompanies, MaxConcurrentPos
FROM dbo.Licenses
WHERE LicenseNumber = @LicenseNumber";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@LicenseNumber", licenseNumber);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return LicenseStatus.Invalid("Lisensnummeret finnes ikke i databasen.");
            }

            return LicenseStatus.Valid(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2));
        }

        private async Task ReportUsageAsync()
        {
            if (!CurrentStatus.IsValid)
            {
                return;
            }

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = @"
UPDATE dbo.Licenses
SET ActivePosSessions = @ActivePosSessions
WHERE LicenseNumber = @LicenseNumber";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ActivePosSessions", ActivePosWindows);
            command.Parameters.AddWithValue("@LicenseNumber", licenseNumber);

            await command.ExecuteNonQueryAsync();
        }
    }

    public record LicensedCompany(string OrgNumber, string CompanyName, string? ContactEmail);

    public record LicenseStatus(string LicenseNumber, int MaxCompanies, int MaxConcurrentPos, bool IsValid, string Message)
    {
        public static LicenseStatus Unvalidated() => new(string.Empty, 0, 0, false, "Lisens er ikke validert ennå.");

        public static LicenseStatus Valid(string licenseNumber, int maxCompanies, int maxConcurrentPos)
            => new(licenseNumber, maxCompanies, maxConcurrentPos, true, string.Empty);

        public static LicenseStatus Invalid(string message)
            => new(string.Empty, 0, 0, false, message);
    }
}

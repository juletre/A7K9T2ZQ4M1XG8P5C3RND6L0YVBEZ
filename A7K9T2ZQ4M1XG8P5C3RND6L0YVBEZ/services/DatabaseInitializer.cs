using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.services
{
    public class DatabaseInitializer
    {
        private readonly string connectionString;

        public DatabaseInitializer(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task EnsurePosTablesAsync()
        {
            await EnsureDatabaseExistsAsync();

            const string commandText = @"
IF OBJECT_ID('dbo.Companies', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Companies (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        LicenseNumber NVARCHAR(64) NOT NULL,
        OrgNumber NVARCHAR(32) NOT NULL,
        CompanyName NVARCHAR(256) NOT NULL,
        ContactEmail NVARCHAR(256) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END

IF OBJECT_ID('dbo.Employees', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId INT NOT NULL,
        EmployeeNumber NVARCHAR(64) NOT NULL,
        FullName NVARCHAR(256) NOT NULL,
        Role NVARCHAR(128) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Employees_Companies FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(Id)
    );
END

IF OBJECT_ID('dbo.Customers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId INT NOT NULL,
        CustomerNumber NVARCHAR(64) NULL,
        FullName NVARCHAR(256) NOT NULL,
        Email NVARCHAR(256) NULL,
        Phone NVARCHAR(64) NULL,
        GroupName NVARCHAR(128) NULL,
        DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Customers_Companies FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(Id)
    );
END

IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId INT NOT NULL,
        Sku NVARCHAR(64) NULL,
        Name NVARCHAR(256) NOT NULL,
        Category NVARCHAR(128) NULL,
        Price DECIMAL(12,2) NOT NULL,
        VatRate DECIMAL(5,2) NOT NULL DEFAULT 25,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Products_Companies FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(Id)
    );
END

IF OBJECT_ID('dbo.PaymentMethods', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentMethods (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId INT NOT NULL,
        MethodName NVARCHAR(128) NOT NULL,
        MethodType NVARCHAR(64) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_PaymentMethods_Companies FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(Id)
    );
END

IF OBJECT_ID('dbo.Terminals', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Terminals (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId INT NOT NULL,
        TerminalName NVARCHAR(128) NOT NULL,
        TerminalType NVARCHAR(64) NOT NULL,
        IpAddress NVARCHAR(64) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Terminals_Companies FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(Id)
    );
END

IF OBJECT_ID('dbo.Sales', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sales (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId INT NOT NULL,
        TerminalId INT NULL,
        EmployeeId INT NULL,
        CustomerId INT NULL,
        ReceiptNumber NVARCHAR(64) NOT NULL,
        Status NVARCHAR(32) NOT NULL,
        Subtotal DECIMAL(12,2) NOT NULL,
        VatTotal DECIMAL(12,2) NOT NULL,
        Total DECIMAL(12,2) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Sales_Companies FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(Id),
        CONSTRAINT FK_Sales_Terminals FOREIGN KEY (TerminalId) REFERENCES dbo.Terminals(Id),
        CONSTRAINT FK_Sales_Employees FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id),
        CONSTRAINT FK_Sales_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id)
    );
END

IF OBJECT_ID('dbo.SaleLines', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaleLines (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SaleId INT NOT NULL,
        ProductId INT NULL,
        Description NVARCHAR(256) NOT NULL,
        Quantity DECIMAL(10,2) NOT NULL,
        UnitPrice DECIMAL(12,2) NOT NULL,
        LineTotal DECIMAL(12,2) NOT NULL,
        VatRate DECIMAL(5,2) NOT NULL,
        CONSTRAINT FK_SaleLines_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id)
    );
END

IF OBJECT_ID('dbo.SalePayments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalePayments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SaleId INT NOT NULL,
        PaymentMethodId INT NOT NULL,
        Amount DECIMAL(12,2) NOT NULL,
        Reference NVARCHAR(128) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_SalePayments_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id),
        CONSTRAINT FK_SalePayments_PaymentMethods FOREIGN KEY (PaymentMethodId) REFERENCES dbo.PaymentMethods(Id)
    );
END

IF OBJECT_ID('dbo.PosSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PosSettings (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CompanyId INT NOT NULL,
        TerminalId INT NULL,
        SettingKey NVARCHAR(128) NOT NULL,
        SettingValue NVARCHAR(512) NOT NULL,
        Scope NVARCHAR(32) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_PosSettings_Companies FOREIGN KEY (CompanyId) REFERENCES dbo.Companies(Id),
        CONSTRAINT FK_PosSettings_Terminals FOREIGN KEY (TerminalId) REFERENCES dbo.Terminals(Id)
    );
END
";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(commandText, connection);
            await command.ExecuteNonQueryAsync();
        }

        public async Task EnsureLicenseTablesAsync()
        {
            await EnsureDatabaseExistsAsync();

            const string commandText = @"
IF OBJECT_ID('dbo.Licenses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Licenses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        LicenseNumber NVARCHAR(64) NOT NULL UNIQUE,
        ContactName NVARCHAR(256) NULL,
        ContactEmail NVARCHAR(256) NULL,
        InvoiceAddress NVARCHAR(512) NULL,
        MaxCompanies INT NOT NULL,
        MaxConcurrentPos INT NOT NULL,
        ActivePosSessions INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END

IF OBJECT_ID('dbo.LicenseCompanies', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LicenseCompanies (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        LicenseId INT NOT NULL,
        OrgNumber NVARCHAR(32) NOT NULL,
        CompanyName NVARCHAR(256) NOT NULL,
        ContactEmail NVARCHAR(256) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_LicenseCompanies_Licenses FOREIGN KEY (LicenseId) REFERENCES dbo.Licenses(Id)
    );
END
";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(commandText, connection);
            await command.ExecuteNonQueryAsync();
        }

        public async Task EnsureSeedCompanyAsync(string licenseNumber, string orgNumber, string companyName)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE OrgNumber = @OrgNumber)
BEGIN
    INSERT INTO dbo.Companies (LicenseNumber, OrgNumber, CompanyName)
    VALUES (@LicenseNumber, @OrgNumber, @CompanyName);
END";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@LicenseNumber", licenseNumber);
            command.Parameters.AddWithValue("@OrgNumber", orgNumber);
            command.Parameters.AddWithValue("@CompanyName", companyName);

            await command.ExecuteNonQueryAsync();
        }

        private async Task EnsureDatabaseExistsAsync()
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return;
            }

            var masterBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            await using var connection = new SqlConnection(masterBuilder.ConnectionString);
            await connection.OpenAsync();

            const string sql = @"
IF DB_ID(@DatabaseName) IS NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX) = N'CREATE DATABASE [' + @DatabaseName + N']';
    EXEC sp_executesql @sql;
END";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@DatabaseName", databaseName);
            await command.ExecuteNonQueryAsync();
        }
    }
}

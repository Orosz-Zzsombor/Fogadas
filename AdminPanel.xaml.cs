using FogadasMokuskodas;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Data;
using Fogadas;
using LiveCharts;
using LiveCharts.Wpf;

namespace FogadasMokuskodas
{
    public partial class AdminPanel : Window
    {
        private readonly string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private readonly AdminService adminService;
        private Bettor currentAdmin;
        private ObservableCollection<Bettor> users;
        private ObservableCollection<Event> events;
        private ObservableCollection<Bet> transactions;
        public SeriesCollection RegistrationSeries { get; set; }
        public ObservableCollection<string> MonthLabels { get; set; }

        public AdminPanel(Bettor admin)
        {
            InitializeComponent();
            adminService = new AdminService(connectionString);
            currentAdmin = admin;
            InitializeCollections();
            RegistrationSeries = new SeriesCollection();
            MonthLabels = new ObservableCollection<string>();
            DataContext = this;
            LoadDashboardData();
        }

        private void InitializeCollections()
        {
            users = new ObservableCollection<Bettor>();
            events = new ObservableCollection<Event>();
            transactions = new ObservableCollection<Bet>();
        }


        private async void LoadDashboardData()
        {
            try
            {
             
                var stats = await adminService.GetDashboardStatistics();
                UpdateDashboardUI(stats);

           
                await LoadRegistrationDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task LoadRegistrationDataAsync()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        DATE_FORMAT(JoinDate, '%Y-%m') AS Month,
                        COUNT(*) AS RegistrationCount
                    FROM Bettors 
                    GROUP BY DATE_FORMAT(JoinDate, '%Y-%m')
                    ORDER BY DATE_FORMAT(JoinDate, '%Y-%m')";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var values = new ChartValues<int>();
                        while (await reader.ReadAsync())
                        {
                            string month = reader.GetString("Month");
                            int registrationCount = reader.GetInt32("RegistrationCount");

                            MonthLabels.Add(month); 
                            values.Add(registrationCount); 
                        }

     
                        RegistrationSeries.Add(new LineSeries
                        {
                            Title = "Registrations",
                            Values = values
                        });

                      
                        RegistrationsChart.Series = RegistrationSeries;
                    }
                }
            }
        }
            private void UpdateDashboardUI(DashboardStatistics stats)
        {
            txtTotalUsers.Text = stats.TotalUsers.ToString("N0");
            txtActiveEvents.Text = stats.ActiveEvents.ToString();
            txtTotalRevenue.Text = $"${stats.TotalRevenue:N2}";
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationWindow auth = new AuthenticationWindow();
            auth.Show();
            this.Close();
        }
    }

    public class AdminService
    {
        private readonly string connectionString;

        public AdminService(string connString)
        {
            connectionString = connString;
        }

        public async Task<DashboardStatistics> GetDashboardStatistics()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var stats = new DashboardStatistics();
                using (var cmd = new MySqlCommand(@"
                    SELECT 
                        COUNT(*) as TotalUsers,
                        SUM(CASE WHEN JoinDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) as NewUsers
                    FROM Bettors
                    WHERE Role = 'user' AND IsActive = 1", conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            stats.TotalUsers = reader.GetInt32("TotalUsers");
                        }
                    }
                }

                using (var cmd = new MySqlCommand(@"
                    SELECT 
                        COUNT(*) as ActiveEvents
                    FROM Events
                    WHERE EventDate >= CURDATE()", conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            stats.ActiveEvents = reader.GetInt32("ActiveEvents");
                        }
                    }
                }

                using (var cmd = new MySqlCommand(@"
                    SELECT SUM(Amount) as TotalRevenue
                    FROM Bets
                    WHERE BetDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)", conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stats.TotalRevenue = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }

                return stats;
            }
        }

        public async Task<List<Bettor>> GetUsers(string searchTerm = "", string sortBy = "Username")
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var users = new List<Bettor>();

                var query = @"
                    SELECT BettorsID, Username, Email, Balance, Role, IsActive
                    FROM Bettors
                    WHERE (@searchTerm = '' OR Username LIKE @searchTerm OR Email LIKE @searchTerm)
                    ORDER BY " + sortBy;

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new Bettor
                            {
                                BettorsID = reader.GetInt32("BettorsID"),
                                Username = reader.GetString("Username"),
                                Email = reader.GetString("Email"),
                                Balance = reader.GetDecimal("Balance"),
                                Role = reader.GetString("Role"),
                                IsActive = reader.GetBoolean("IsActive")
                            });
                        }
                    }
                }

                return users;
            }
        }

        public async Task<bool> UpdateUserStatus(int userId, bool isActive)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand("UPDATE Bettors SET IsActive = @isActive WHERE BettorsID = @userId", conn))
                {
                    cmd.Parameters.AddWithValue("@isActive", isActive);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<List<Bet>> GetTransactions(DateTime? startDate = null, DateTime? endDate = null)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var transactions = new List<Bet>();

                var query = @"
                    SELECT b.BetsID, b.BetDate, b.Amount, b.Odds, b.Status, 
                           b.EventID, e.EventName, b.BettorsID
                    FROM Bets b
                    JOIN Events e ON b.EventID = e.EventID
                    WHERE (@startDate IS NULL OR b.BetDate >= @startDate)
                    AND (@endDate IS NULL OR b.BetDate <= @endDate)
                    ORDER BY b.BetDate DESC";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@startDate", startDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@endDate", endDate ?? (object)DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transactions.Add(new Bet
                            {
                                BetsID = reader.GetInt32("BetsID"),
                                BetDate = reader.GetDateTime("BetDate"),
                                Amount = reader.GetDecimal("Amount"),
                                Odds = reader.GetDecimal("Odds"),
                                Status = reader.GetInt32("Status"),
                                EventID = reader.GetInt32("EventID"),
                                EventName = reader.GetString("EventName"),
                                BettorsID = reader.GetInt32("BettorsID")
                            });
                        }
                    }
                }

                return transactions;
            }
        }
    }

    public class DashboardStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveEvents { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using LiveCharts;
using LiveCharts.Wpf;
using Fogadas;
using System.Transactions;

namespace FogadasMokuskodas
{
    public partial class AdminPanel : Window
    {
        private readonly string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private Bettor currentAdmin;
        private ObservableCollection<Bettor> users;
        private ObservableCollection<Event> events;
        private ObservableCollection<Bet> transactions;
        public SeriesCollection RegistrationSeries { get; set; }
        public ObservableCollection<string> MonthLabels { get; set; }

        public AdminPanel(Bettor admin)
        {
            InitializeComponent();
            currentAdmin = admin;
            InitializeCollections();
            RegistrationSeries = new SeriesCollection();
            MonthLabels = new ObservableCollection<string>();
            DataContext = this;
            LoadDashboardData();


            txtTotalUsers.MouseLeftButtonDown += TxtTotalUsers_MouseLeftButtonDown;
        }

        private void InitializeCollections()
        {
            users = new ObservableCollection<Bettor>();
            events = new ObservableCollection<Event>();
            transactions = new ObservableCollection<Bet>();
        }
        private void OverviewButton_Click(object sender, RoutedEventArgs e)
        {
            OverviewSection.Visibility = Visibility.Visible;
            UsersSection.Visibility = Visibility.Collapsed;
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            OverviewSection.Visibility = Visibility.Collapsed;
            UsersSection.Visibility = Visibility.Visible;
            LoadAndDisplayUsers();
        }

        private void LoadDashboardData()
        {
            try
            {
                var stats = GetDashboardStatistics();
                UpdateDashboardUI(stats);
                LoadRegistrationData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRegistrationData()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        DATE_FORMAT(JoinDate, '%Y-%m') AS Month,
                        COUNT(*) AS RegistrationCount
                    FROM Bettors 
                    GROUP BY DATE_FORMAT(JoinDate, '%Y-%m')
                    ORDER BY DATE_FORMAT(JoinDate, '%Y-%m')";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var values = new ChartValues<int>();
                        while (reader.Read())
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

        private DashboardStatistics GetDashboardStatistics()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var stats = new DashboardStatistics();

                using (var cmd = new MySqlCommand(@"
                    SELECT 
                        COUNT(*) as TotalUsers,
                        SUM(CASE WHEN JoinDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) as NewUsers
                    FROM Bettors
                    WHERE Role = 'user' AND IsActive = 1", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
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
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
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
                    var result = cmd.ExecuteScalar();
                    stats.TotalRevenue = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }

                return stats;
            }
        }

        private void TxtTotalUsers_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoadAndDisplayUsers();
        }

        private void LoadAndDisplayUsers()
        {
            try
            {
                users = new ObservableCollection<Bettor>(GetUsers());
                UsersItemsControl.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Bettor> GetUsers(string searchTerm = "", string sortBy = "Username")
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var users = new List<Bettor>();

                var query = @"
                    SELECT BettorsID, Username, Email, Balance
                    FROM Bettors
                    WHERE (@searchTerm = '' OR Username LIKE @searchTerm OR Email LIKE @searchTerm)
                    ORDER BY " + sortBy;

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new Bettor
                            {
                                BettorsID = reader.GetInt32("BettorsID"),
                                Username = reader.GetString("Username"),
                                Email = reader.GetString("Email"),
                                Balance = reader.GetDecimal("Balance")
                            });
                        }
                    }
                }

                return users;
            }
        }

        private void DisplayUsers(List<Bettor> users)
        {
            UsersSection.Children.Clear();

            foreach (var user in users)
            {
                var userPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var userInfo = new TextBlock
                {
                    Text = $"{user.Username} - {user.Email} - Balance: ${user.Balance:N2}",
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                var modifyButton = new Button
                {
                    Content = "Modify",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38B2AC")),
                    Foreground = Brushes.White,
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(0, 0, 10, 0)
                };
         
              
            }
        }

        private void ModifyUser_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var user = (Bettor)button.DataContext;

            var modifyWindow = new ModifyUserWindow(user);
            if (modifyWindow.ShowDialog() == true)
            {
                UpdateUserInDatabase(modifyWindow.ModifiedUser);
                LoadAndDisplayUsers();
            }
        }
        private void UpdateUserInDatabase(Bettor user)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE Bettors SET Username = @username, Email = @email, Balance = @balance WHERE BettorsID = @userId", conn))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    cmd.Parameters.AddWithValue("@balance", user.Balance);
                    cmd.Parameters.AddWithValue("@userId", user.BettorsID);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var user = (Bettor)button.DataContext;
            DeleteUser(user);
        }

        private void DeleteUser(Bettor user)
        {
            var result = MessageBox.Show($"Are you sure you want to delete user: {user.Username}? This will also delete all related bets.", "Delete User", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var scope = new TransactionScope())
                    {
                        DeleteUserBets(user.BettorsID);
                        DeleteUserFromDatabase(user.BettorsID);
                        scope.Complete();
                    }
                    users.Remove(user);
                    MessageBox.Show($"User {user.Username} and all related bets have been successfully deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void DeleteUserBets(int userId)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("DELETE FROM Bets WHERE BettorsID = @userId", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteUserRelatedData(int userId)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                DeleteUserBets(conn, userId);
                DeleteUserEvents(conn, userId);
          
            }
        }
        private void DeleteUserBets(MySqlConnection conn, int userId)
        {
            using (var cmd = new MySqlCommand("DELETE FROM Bets WHERE BettorsID = @userId", conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        private void DeleteUserEvents(MySqlConnection conn, int userId)
        {
            using (var cmd = new MySqlCommand("DELETE FROM Events WHERE CreatorID = @userId", conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();
            }
        }
        private void DeleteUserFromDatabase(int userId)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("DELETE FROM Bettors WHERE BettorsID = @userId", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                }
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
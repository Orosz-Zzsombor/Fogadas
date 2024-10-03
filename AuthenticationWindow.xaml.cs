using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Fogadas
{
    public partial class AuthenticationWindow : Window
    {
        private string connectionString = "server=localhost;port=3306;database=Fogadas;user=root;password=;";

        public AuthenticationWindow()
        {
            InitializeComponent();
        }
        private void SwitchToRegister_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;

   
            LoginButton.Background = (Brush)FindResource("SidebarInactiveBrush");
            SignupButton.Background = (Brush)FindResource("MainBackgroundBrush");


            LoginButton.Margin = new Thickness(0, 0, -80, 20);
            SignupButton.Margin = new Thickness(0, 0, -60, 0); 
        }

        private void SwitchToLogin_Click(object sender, RoutedEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;

            
            LoginButton.Background = (Brush)FindResource("MainBackgroundBrush");
            SignupButton.Background = (Brush)FindResource("SidebarInactiveBrush");

            
            LoginButton.Margin = new Thickness(0, 0, -60, 20); 
            SignupButton.Margin = new Thickness(0, 0, -80, 0); 
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUsername.Text;
            string password = LoginPassword.Password;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Bettors WHERE Username = @username AND Password = @password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        MessageBox.Show("Login successful!");
                    
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close(); 
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegisterUsername.Text;
            string email = RegisterEmail.Text;
            string password = RegisterPassword.Password;

            if (!IsValidUsername(username) || !IsValidPassword(password) || !IsValidEmail(email))
            {
                MessageBox.Show("Please check your input. Username must be at least 5 characters, password must be at least 5 characters, and email must be valid.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Bettors (Username, Password, Balance, Email, JoinDate, IsActive, Role) " +
                                   "VALUES (@username, @password, 1000, @Email, CURDATE(), 1, 'user')";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@Email", email);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Registration successful!");
                    SwitchToLogin_Click(sender, e); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidUsername(string username)
        {
            return username.Length >= 5;
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 5;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

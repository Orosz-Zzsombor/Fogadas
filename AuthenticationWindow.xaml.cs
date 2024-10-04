using FogadasMokuskodas;
using MySql.Data.MySqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using static FogadasMokuskodas.Bettor;

namespace Fogadas
{
    public partial class AuthenticationWindow : Window
    {
        private string connectionString = "server=localhost;port=3306;database=FogadasDB;user=root;password=;";

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

           
            string hashedPassword = ComputeSha256Hash(password);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Bettors WHERE Username = @username AND Password = @password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);  

                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        
                        Bettor bettor = new Bettor
                        {
                            BettorsID = reader.GetInt32("BettorsID"),
                            Username = reader.GetString("Username"),
                            Email = reader.GetString("Email"),
                            Balance = reader.GetInt32("Balance"),
                            Role = reader.GetString("Role"),
                            IsActive = reader.GetBoolean("IsActive")
                        };

                     
                        SessionData.CurrentBettor = bettor;

                       
                        MessageBox.Show($"Login successful! Welcome {bettor.Username}");
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


       
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegisterUsername.Text;
            string password = RegisterPassword.Password;

            string hashedPassword = ComputeSha256Hash(password);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Bettors (Username, Password, Balance, Email, JoinDate, IsActive, Role) VALUES (@username, @password, 0, @Email, CURDATE(), 1, 'user')";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);  
                    cmd.Parameters.AddWithValue("@Email", RegisterEmail.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Registration successful!");
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

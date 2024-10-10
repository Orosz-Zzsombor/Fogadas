using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using FogadasMokuskodas;
using System.Text; 

namespace Fogadas
{
    public partial class ProfileWindow : Window
    {
        private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private Bettor currentBettor;
        public event Action<string> UsernameUpdated;

        public ProfileWindow(Bettor bettor)
        {
            InitializeComponent();
            currentBettor = bettor;
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (currentBettor != null)
            {
                txtUsername.Text = currentBettor.Username;
                txtEmail.Text = currentBettor.Email;

            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
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

        private bool ValidateCurrentPassword(string password)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Password FROM Bettors WHERE BettorsID = @id";
                command.Parameters.AddWithValue("@id", currentBettor.BettorsID);

                var storedHash = command.ExecuteScalar()?.ToString();
                var inputHash = ComputeSha256Hash(password); 

                return storedHash == inputHash;
            }
        }

        private void UpdateUserData()
        {
    
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Email address cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

   
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

  
            if (!string.IsNullOrEmpty(txtNewPassword.Password))
            {
                if (txtNewPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show("New passwords do not match.", "Password Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateCurrentPassword(txtCurrentPassword.Password))
                {
                    MessageBox.Show("Current password is incorrect.", "Password Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

             
                    command.CommandText = @"
                    UPDATE Bettors 
                    SET Email = @email, Username = @username" +
                        (!string.IsNullOrEmpty(txtNewPassword.Password) ? ", Password = @password" : "") +
                        " WHERE BettorsID = @id";

        
                    command.Parameters.AddWithValue("@email", txtEmail.Text);
                    command.Parameters.AddWithValue("@username", txtUsername.Text);
                    command.Parameters.AddWithValue("@id", currentBettor.BettorsID);

      
                    if (!string.IsNullOrEmpty(txtNewPassword.Password))
                    {
                        command.Parameters.AddWithValue("@password", ComputeSha256Hash(txtNewPassword.Password));
                    }

                    command.ExecuteNonQuery();

          
                    currentBettor.Email = txtEmail.Text;
                    currentBettor.Username = txtUsername.Text;

          
                    UsernameUpdated?.Invoke(currentBettor.Username);

                    MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    


    private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            UpdateUserData();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
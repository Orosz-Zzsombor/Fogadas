using System;
using System.Windows;
using System.Windows.Controls;
using Fogadas;
using FogadasMokuskodas;
using MySql.Data.MySqlClient;
using static FogadasMokuskodas.Bettor;

namespace FogadasMokuskodas
{
    public partial class WalletWindow : Window
    {
        private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private Bettor currentBettor;
        private readonly MainWindow mainWindow;

        public WalletWindow(Bettor bettor, MainWindow mainWindow)
        {
            InitializeComponent();
            currentBettor = SessionData.CurrentBettor;
            this.mainWindow = mainWindow;
            LoadCurrentBalance();
        }

        private void LoadCurrentBalance()
        {
            if (currentBettor != null)
            {

                BalanceTextBlock.Text = $"{currentBettor.Balance:N2} Ft";
            }
            else
            {
                BalanceTextBlock.Text = "0.00 Ft";

            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DepositButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                if (decimal.TryParse(DepositAmountTextBox.Text, out decimal depositAmount))
                {
                    if (depositAmount > 0)
                    {
                        UpdateBalance(depositAmount);
                 
                        ClearInputs();
                        mainWindow.UpdateBalanceDisplay();
                        MessageBox.Show("Deposit successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid deposit amount.", "Invalid Amount", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid deposit amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(CardNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(CardholderNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(ExpirationDateTextBox.Text) ||
                string.IsNullOrWhiteSpace(CVVTextBox.Text) ||
                string.IsNullOrWhiteSpace(DepositAmountTextBox.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Incomplete Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

 
            if (
                !CardNumberTextBox.Text.All(char.IsDigit))
            {
                MessageBox.Show("Invalid card number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!ExpirationDateTextBox.Text.Contains("/") ||
                ExpirationDateTextBox.Text.Length != 5)
            {
                MessageBox.Show("Invalid expiration date. Please use MM/YY format.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CVVTextBox.Text.Length != 3 ||
                !CVVTextBox.Text.All(char.IsDigit))
            {
                MessageBox.Show("Invalid CVV. Please enter a 3-digit number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void UpdateBalance(decimal depositAmount)
        {
            if (currentBettor != null)
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bettors SET Balance = Balance + @DepositAmount WHERE BettorsID = @BettorID";
                    command.Parameters.AddWithValue("@DepositAmount", depositAmount);
                    command.Parameters.AddWithValue("@BettorID", currentBettor.BettorsID);
                    command.ExecuteNonQuery();


                    currentBettor.Balance += (int)depositAmount;
                    SessionData.CurrentBettor = currentBettor;

                  
                    LoadCurrentBalance();
                }
            }
        }

        private void ClearInputs()
        {
            CardNumberTextBox.Clear();
            CardholderNameTextBox.Clear();
            ExpirationDateTextBox.Clear();
            CVVTextBox.Clear();
            DepositAmountTextBox.Clear();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
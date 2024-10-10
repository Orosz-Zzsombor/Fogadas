using System;
using System.Windows;
using System.Windows.Controls;
using FogadasMokuskodas;
using MySql.Data.MySqlClient;

namespace Fogadas
{
    public partial class EventDetailsWindow : Window
    {
        private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private readonly Event selectedEvent;
        private readonly Bettor currentBettor;
        private readonly MainWindow mainWindow;
        private decimal currentOdds = 2.5m; 

        public EventDetailsWindow(Event evt, Bettor bettor, MainWindow mainWindow)
        {
            InitializeComponent();
            selectedEvent = evt;
            currentBettor = bettor;
            this.mainWindow = mainWindow;
            DisplayEventDetails(evt);
            SetupEventHandlers();
        }

        private void DisplayEventDetails(Event evt)
        {
            EventNameTextBlock.Text = evt.EventName;
            EventDateTextBlock.Text = evt.EventDate.ToShortDateString();
            CategoryTextBlock.Text = evt.Category;
            LocationTextBlock.Text = evt.Location;
            


            UpdateOddsDisplay();

            UpdateEventStatistics();
        }

        private void SetupEventHandlers()
        {
            BetAmountTextBox.TextChanged += BetAmountTextBox_TextChanged;
        }

        private void UpdateOddsDisplay()
        {
 
            OddsTextBlock.Text = $"Current Odds: {currentOdds:F2}";
        }

        private void UpdateEventStatistics()
        {
     
            TotalBetsTextBlock.Text = $"Total Bets: {GetTotalBetsCount()}";
            AverageBetTextBlock.Text = $"Average Bet: ${GetAverageBetAmount():F2}";
            HighestBetTextBlock.Text = $"Highest Bet: ${GetHighestBetAmount():F2}";
        }

        private int GetTotalBetsCount()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var command = new MySqlCommand("SELECT COUNT(*) FROM Bets WHERE EventID = @eventId", conn);
                command.Parameters.AddWithValue("@eventId", selectedEvent.EventID);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private decimal GetAverageBetAmount()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var command = new MySqlCommand("SELECT AVG(Amount) FROM Bets WHERE EventID = @eventId", conn);
                command.Parameters.AddWithValue("@eventId", selectedEvent.EventID);
                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        private decimal GetHighestBetAmount()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var command = new MySqlCommand("SELECT MAX(Amount) FROM Bets WHERE EventID = @eventId", conn);
                command.Parameters.AddWithValue("@eventId", selectedEvent.EventID);
                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        private void BetAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(BetAmountTextBox.Text, out decimal betAmount))
            {
                decimal potentialWinnings = betAmount * currentOdds;
                PotentialWinningsTextBlock.Text = $"${potentialWinnings:F2}";
            }
            else
            {
                PotentialWinningsTextBlock.Text = "$0.00";
            }
        }

        private void PlaceBet_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(BetAmountTextBox.Text, out decimal betAmount) && betAmount > 0)
            {
                if (currentBettor.Balance < betAmount)
                {
                    MessageBox.Show("You do not have enough balance to place this bet.", "Insufficient Balance", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                            var command = new MySqlCommand("INSERT INTO Bets (BetDate, Odds, Amount, BettorsID, EventID, Status) VALUES (NOW(), @odds, @amount, @bettorsId, @eventId, @status)", conn);
                            command.Parameters.AddWithValue("@odds", currentOdds);
                            command.Parameters.AddWithValue("@amount", betAmount);
                            command.Parameters.AddWithValue("@bettorsId", currentBettor.BettorsID);
                            command.Parameters.AddWithValue("@eventId", selectedEvent.EventID);
                            command.Parameters.AddWithValue("@status", true);
                            command.Transaction = transaction;

                            command.ExecuteNonQuery();

                            var updateBalanceCommand = new MySqlCommand("UPDATE Bettors SET Balance = Balance - @amount WHERE BettorsID = @bettorsId", conn);
                            updateBalanceCommand.Parameters.AddWithValue("@amount", betAmount);
                            updateBalanceCommand.Parameters.AddWithValue("@bettorsId", currentBettor.BettorsID);
                            updateBalanceCommand.Transaction = transaction;

                            updateBalanceCommand.ExecuteNonQuery();

                            transaction.Commit();
                        }

                        currentBettor.Balance -= betAmount;
                        mainWindow.UpdateBalanceDisplay();
                        UpdateEventStatistics();

                        MessageBox.Show("Bet placed successfully!");
                        BetAmountTextBox.Text = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error placing bet: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid bet amount greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
using System;
using System.Windows;
using FogadasMokuskodas;
using MySql.Data.MySqlClient; // Ensure this is included to access the Bettor class

namespace Fogadas
{
    public partial class EventDetailsWindow : Window
    {
        private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private readonly Event selectedEvent; // Store the event details
        private readonly Bettor currentBettor; // Store the current bettor

        // Constructor that accepts both the event and the current bettor
        public EventDetailsWindow(Event evt, Bettor bettor)
        {
            InitializeComponent();
            selectedEvent = evt;
            currentBettor = bettor; // Assign the current bettor
            DisplayEventDetails(evt);
        }

        private void DisplayEventDetails(Event evt)
        {
            EventNameTextBlock.Text = evt.EventName;
            EventDateTextBlock.Text = $"Date: {evt.EventDate.ToShortDateString()}";
            CategoryTextBlock.Text = $"Category: {evt.Category}";
            LocationTextBlock.Text = $"Location: {evt.Location}";
            // You can add more details if needed, e.g., Description, Odds, etc.
        }

        private void PlaceBet_Click(object sender, RoutedEventArgs e)
        {
            // Validate and process the bet amount
            if (decimal.TryParse(BetAmountTextBox.Text, out decimal betAmount) && betAmount > 0)
            {
                // Check if the bettor has enough balance
                if (currentBettor.Balance < betAmount)
                {
                    MessageBox.Show("You do not have enough balance to place this bet.", "Insufficient Balance", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Exit if balance is insufficient
                }

                // Logic to handle the bet placement
                // Here you can define your odds based on the event details
                decimal odds = 2.0m; // Example odds, you could fetch or calculate this based on your business logic.

                // Insert the bet into the database
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                            var command = new MySqlCommand("INSERT INTO Bets (BetDate, Odds, Amount, BettorsID, EventID, Status) VALUES (NOW(), @odds, @amount, @bettorsId, @eventId, @status)", conn);
                            command.Parameters.AddWithValue("@odds", odds); // Capture the odds here
                            command.Parameters.AddWithValue("@amount", betAmount);
                            command.Parameters.AddWithValue("@bettorsId", currentBettor.BettorsID);
                            command.Parameters.AddWithValue("@eventId", selectedEvent.EventID);
                            command.Parameters.AddWithValue("@status", true); // Assuming true means active or successful bet
                            command.Transaction = transaction;

                            command.ExecuteNonQuery();

                            // Deduct the bet amount from the bettor's balance
                            var updateBalanceCommand = new MySqlCommand("UPDATE Bettors SET Balance = Balance - @amount WHERE BettorsID = @bettorsId", conn);
                            updateBalanceCommand.Parameters.AddWithValue("@amount", betAmount);
                            updateBalanceCommand.Parameters.AddWithValue("@bettorsId", currentBettor.BettorsID);
                            updateBalanceCommand.Transaction = transaction;

                            updateBalanceCommand.ExecuteNonQuery();

                            transaction.Commit(); // Commit the transaction
                        }

                        MessageBox.Show("Bet placed successfully!");
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


    }
}

using System;
using System.Windows;
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

       
        public EventDetailsWindow(Event evt, Bettor bettor, MainWindow mainWindow)
        {
            InitializeComponent();
            selectedEvent = evt;
            currentBettor = bettor;
            this.mainWindow = mainWindow;
            DisplayEventDetails(evt);
        }

        private void DisplayEventDetails(Event evt)
        {
            EventNameTextBlock.Text = evt.EventName;
            EventDateTextBlock.Text = $"Date: {evt.EventDate.ToShortDateString()}";
            CategoryTextBlock.Text = $"Category: {evt.Category}";
            LocationTextBlock.Text = $"Location: {evt.Location}";
           
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

       
                decimal odds = 2.0m; 

               
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction())
                        {
                         
                            var command = new MySqlCommand("INSERT INTO Bets (BetDate, Odds, Amount, BettorsID, EventID, Status) VALUES (NOW(), @odds, @amount, @bettorsId, @eventId, @status)", conn);
                            command.Parameters.AddWithValue("@odds", selectedEvent.Odds); 
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
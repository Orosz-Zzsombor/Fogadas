using System;
using System.Collections.Generic;
using System.Windows;
using MySql.Data.MySqlClient;

namespace FogadasMokuskodas
{
    public partial class UserHistoryWindow : Window
    {
        private string connectionString;
        private Bettor user;

        public UserHistoryWindow(string connectionString, Bettor user)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this.user = user;
            LoadUserHistory();
        }

        private void LoadUserHistory()
        {
            UserInfoTextBlock.Text = $"Betting History for {user.Username}";

            var betHistory = new List<BetHistoryItem>();
            int totalBets = 0; 

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var query = @"
            SELECT e.EventName, b.BetDate, b.Amount, b.Odds, b.Status
            FROM Bets b
            JOIN Events e ON b.EventID = e.EventID
            WHERE b.BettorsID = @userId
            ORDER BY b.BetDate DESC";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", user.BettorsID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            betHistory.Add(new BetHistoryItem
                            {
                                EventName = reader.GetString("EventName"),
                                BetDate = reader.GetDateTime("BetDate"),
                                Amount = reader.GetDecimal("Amount"),
                                Odds = reader.GetFloat("Odds"),
                            });
                            totalBets++; 
                        }
                    }
                }
            }

           
            BetHistoryDataGrid.ItemsSource = betHistory;
            TotalBetsTextBlock.Text = $"Total Bets: {totalBets}";
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class BetHistoryItem
    {
        public string EventName { get; set; }
        public DateTime BetDate { get; set; }
        public decimal Amount { get; set; }
        public float Odds { get; set; }
        public string Status { get; set; }
    }
}
using FogadasMokuskodas;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Fogadas
{
    public partial class MyBetsWindow : Window
    {
        private readonly string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private readonly Bettor currentBettor;

        public MyBetsWindow(Bettor bettor)
        {
            InitializeComponent();
            currentBettor = bettor;
            LoadBets();
        }

        private void LoadBets()
        {
            List<Bet> bets = new List<Bet>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var command = new MySqlCommand(
                        @"SELECT b.BetsID, b.BetDate, b.Amount, b.Odds, b.Status, e.EventID, e.EventName
                  FROM Bets b
                  JOIN Events e ON b.EventID = e.EventID
                  WHERE b.BettorsID = @bettorsId", conn);

                    command.Parameters.AddWithValue("@bettorsId", currentBettor.BettorsID);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bets.Add(new Bet
                            {
                                BetsID = reader.GetInt32(0),
                                BetDate = reader.GetDateTime(1),
                                Amount = reader.GetDecimal(2),
                                Odds = reader.GetDecimal(3),
                                Status = reader.GetInt32(4), // Read the integer status
                                EventID = reader.GetInt32(5), // Read the EventID
                                EventName = reader.GetString(6) // Get the EventName from the database
                            });
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Database error loading bets: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading bets: " + ex.Message);
                }
            }

            BetsListView.ItemsSource = bets;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
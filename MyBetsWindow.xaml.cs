using System;
using System.Collections.Generic;
using System.Windows;
using FogadasMokuskodas;
using MySql.Data.MySqlClient;

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
                    var command = new MySqlCommand("SELECT BetDate, Amount, Odds, Status FROM Bets WHERE BettorsID = @bettorsId", conn);
                    command.Parameters.AddWithValue("@bettorsId", currentBettor.BettorsID);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bets.Add(new Bet
                            {
                                BetDate = reader.GetDateTime(0),
                                Amount = reader.GetDecimal(1),
                                Odds = reader.GetDecimal(2),
                                Status = reader.GetBoolean(3) ? "Active" : "Inactive"
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading bets: " + ex.Message);
                }
            }

            BetsListView.ItemsSource = bets;
        }
    }
}

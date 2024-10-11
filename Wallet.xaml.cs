using System;
using System.Windows;
using FogadasMokuskodas;
using LiveCharts;
using LiveCharts.Wpf;
using MySql.Data.MySqlClient;

namespace Fogadas
{
    public partial class WalletWindow : Window
    {
        private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private readonly Bettor currentBettor;
        public ChartValues<double> BalanceValues { get; set; }

        public WalletWindow(Bettor bettor)
        {
            InitializeComponent();
            currentBettor = bettor;
            DataContext = this;
            InitializeChart();
            UpdateBalanceDisplay(); // Frissítsd az egyenleget az ablak megnyitásakor
        }

        private void InitializeChart()
        {
            BalanceValues = new ChartValues<double>();
            double startingBalance = GetCurrentBalance(); // Kezdő egyenleg lekérése
            BalanceValues.Add(startingBalance); // Hozzáadás a diagramhoz
            LoadBalanceHistory(startingBalance); // Betöltjük a fogadási történetet
        }

        private void LoadBalanceHistory(double startingBalance)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Amount FROM Bets 
                    WHERE BettorsID = @bettorId
                    ORDER BY BetDate ASC";
                command.Parameters.AddWithValue("@bettorId", currentBettor.BettorsID);

                double currentBalance = startingBalance; // Kezdő egyenleg
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Feltételezzük, hogy az Amount oszlop double típusú
                        double betAmount = reader.GetDouble(0);
                        currentBalance -= betAmount; // Az aktuális egyenleg csökkentése a tét összegével
                        BalanceValues.Add(currentBalance); // Hozzáadjuk az új egyenleget a diagramhoz
                    }
                }
            }
        }

        private void UpdateBalanceDisplay()
        {
            if (currentBettor != null)
            {
                double currentBalance = GetCurrentBalance();
                txtCurrentBalance.Text = $"${currentBalance:N2}"; // Kijelző az egyenleg

                // Frissítjük a diagramot az aktuális egyenleggel
                if (BalanceValues.Count > 0)
                {
                    BalanceValues[BalanceValues.Count - 1] = currentBalance; // Frissítjük az utolsó értéket
                }
                else
                {
                    BalanceValues.Add(currentBalance); // Ha nincs érték, akkor hozzáadjuk
                }
            }
        }

        private double GetCurrentBalance()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Balance FROM Bettors WHERE BettorsID = @bettorId";
                command.Parameters.AddWithValue("@bettorId", currentBettor.BettorsID);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToDouble(result) : 0;
            }
        }

        private int GetTotalBetsCount()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Bets WHERE BettorsID = @bettorId";
                command.Parameters.AddWithValue("@bettorId", currentBettor.BettorsID);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

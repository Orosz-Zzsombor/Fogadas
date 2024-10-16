﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fogadas
{
    public class Event
    {
      
            public int EventID { get; set; }
            public string EventName { get; set; }
            public DateTime EventDate { get; set; }
            public string Category { get; set; }
            public string Location { get; set; }
            public float Odds { get; set; }
            public int IsClosed { get; set; }



    }

    public class EventService
    {
        private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private static Dictionary<int, float> odds = new Dictionary<int, float>();
        private float GenerateRandomOdds()
        {
            Random rand = new Random();
            return (float)Math.Round(1.0 + rand.NextDouble() * 2.0, 2);  
        }




        public List<Event> GetCurrentEvents()
        {
           
            List<Event> events = new List<Event>();
            
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();


                    string query = "SELECT EventID, EventName, EventDate, Category, Location, IsClosed FROM Events WHERE EventDate >= CURDATE()";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int eventId = reader.GetInt32("EventID");


                                if (!odds.ContainsKey(eventId))
                                {
                                    float odd = GenerateRandomOdds();
                                    odds[eventId] = odd;
                                }

                                Event evt = new Event
                                {
                                    EventID = reader.GetInt32("EventID"),
                                    EventName = reader.GetString("EventName"),
                                    EventDate = reader.GetDateTime("EventDate"),
                                    Category = reader.GetString("Category"),
                                    Location = reader.GetString("Location"),
                                    Odds = odds[eventId],                     
                                    IsClosed = reader.GetInt32("IsClosed") 
                                };

                                events.Add(evt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return events;
        }

        public bool CreateEvent(string eventName, DateTime eventDate, string category, string location)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var command = new MySqlCommand("INSERT INTO Events (EventName, EventDate, Category, Location, IsClosed) VALUES (@name, @date, @category, @location,1)", conn);
                    command.Parameters.AddWithValue("@name", eventName);
                    command.Parameters.AddWithValue("@date", eventDate);
                    command.Parameters.AddWithValue("@category", category);
                    command.Parameters.AddWithValue("@location", location);
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating event: " + ex.Message);
                    return false;
                }
            }
        }
 
        public void UpdateEvent(Event evt)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                UPDATE Events
                SET EventName = @EventName,
                    EventDate = @EventDate,
                    Category = @Category,
                    Location = @Location
                WHERE EventID = @EventID";

                command.Parameters.AddWithValue("@EventName", evt.EventName);
                command.Parameters.AddWithValue("@EventDate", evt.EventDate);
                command.Parameters.AddWithValue("@Category", evt.Category);
                command.Parameters.AddWithValue("@Location", evt.Location);
                command.Parameters.AddWithValue("@EventID", evt.EventID);

                command.ExecuteNonQuery();
            }
        }
        public void DeleteEvent(int eventID)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Events WHERE EventID = @EventID";
                command.Parameters.AddWithValue("@EventID", eventID);
                command.ExecuteNonQuery();
            }
        }
        public void CloseEvent(int eventId, bool wasSuccessful)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var betCommand = connection.CreateCommand())
                {
                   
                    betCommand.CommandText = @"
                UPDATE Bets
                SET Status = 0
                WHERE EventID = @EventID";
                    betCommand.Parameters.AddWithValue("@EventID", eventId);
                    betCommand.ExecuteNonQuery();
                }

                using (var eventCommand = connection.CreateCommand())
                {
                    
                    eventCommand.CommandText = @"
                UPDATE Events
                SET IsClosed = 0
                WHERE EventID = @EventID";
                    eventCommand.Parameters.AddWithValue("@EventID", eventId);
                    eventCommand.ExecuteNonQuery();
                }

                if (wasSuccessful)
                {
                    var bets = new List<Bet>();
                    string query = "SELECT * FROM Bets WHERE EventID = @EventID";
                    using (var betQueryCommand = new MySqlCommand(query, connection))
                    {
                        betQueryCommand.Parameters.AddWithValue("@EventID", eventId);
                        using (var reader = betQueryCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bets.Add(new Bet
                                {
                                    BetsID = reader.GetInt32("BetsID"),
                                    Amount = reader.GetDecimal("Amount"),
                                    Odds = reader.GetDecimal("Odds"),
                                    BettorsID = reader.GetInt32("BettorsID"),
                                    EventID = reader.GetInt32("EventID"),
                                    Status = reader.GetInt32("Status")
                                });
                            }
                        }
                    }

                 
                    var bettorPayouts = new Dictionary<int, decimal>();

                    foreach (var bet in bets)
                    {
                        decimal payout = bet.Amount * bet.Odds;

                       
                        if (bettorPayouts.ContainsKey(bet.BettorsID))
                        {
                            bettorPayouts[bet.BettorsID] += payout;
                        }
                        else
                        {
                            bettorPayouts[bet.BettorsID] = payout;
                        }
                    }

                    
                    foreach (var bettorId in bettorPayouts.Keys)
                    {
                        decimal currentBalance = GetBettorBalance(bettorId);
                        decimal totalPayout = bettorPayouts[bettorId];

                        UpdateBettorBalance(connection, bettorId, currentBalance + totalPayout);
                    }
                }

                Console.WriteLine($"CloseEvent executed: EventID = {eventId}, Success = {wasSuccessful}");
            }
        }

        public decimal GetBettorBalance(int bettorId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Balance FROM Bettors WHERE BettorsID = @BettorsID";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BettorsID", bettorId);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }

        private void UpdateBettorBalance(MySqlConnection connection, int bettorId, decimal newBalance)
        {
            // Update the bettor's balance directly in the database
            string updateQuery = "UPDATE Bettors SET Balance = @Balance WHERE BettorsID = @BettorsID";
            using (var updateCommand = new MySqlCommand(updateQuery, connection))
            {
                updateCommand.Parameters.AddWithValue("@Balance", newBalance);
                updateCommand.Parameters.AddWithValue("@BettorsID", bettorId);
                updateCommand.ExecuteNonQuery();
            }
        }
        public List<Bet> GetBetsForEvent(int eventId)
        {
            List<Bet> bets = new List<Bet>();


            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Bets WHERE EventID = @EventID";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EventID", eventId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Bet bet = new Bet
                            {
                                BetsID = reader.GetInt32("BetsID"),
                                BetDate = reader.GetDateTime("BetDate"),
                                Amount = reader.GetDecimal("Amount"),
                                Odds = reader.GetDecimal("Odds"),
                                Status = reader.GetInt32("Status"),
                                EventID = reader.GetInt32("EventID"),
                                BettorsID = reader.GetInt32("BettorsID"),
                                EventName = "" 
                            };

                            bets.Add(bet);
                        }
                    }
                }
            }

            return bets;
        }






    }
}

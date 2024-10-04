using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fogadas
{
    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
    }
    public class EventService
    {

        private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";


        public List<Event> GetCurrentEvents()
        {
            List<Event> events = new List<Event>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();


                    string query = "SELECT * FROM Events WHERE EventDate >= CURDATE()";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Event evt = new Event
                                {
                                    EventID = reader.GetInt32("EventID"),
                                    EventName = reader.GetString("EventName"),
                                    EventDate = reader.GetDateTime("EventDate"),
                                    Category = reader.GetString("Category"),
                                    Location = reader.GetString("Location")
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
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Events (EventName, EventDate, Category, Location) VALUES (@EventName, @EventDate, @Category, @Location)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EventName", eventName);
                        cmd.Parameters.AddWithValue("@EventDate", eventDate);
                        cmd.Parameters.AddWithValue("@Category", category);
                        cmd.Parameters.AddWithValue("@Location", location);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        return rowsAffected > 0; 

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while creating the event: " + ex.Message);

                return false; 

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
    }
}

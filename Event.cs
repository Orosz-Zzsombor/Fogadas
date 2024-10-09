using MySql.Data.MySqlClient;
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
            public decimal Odds { get; set; }
            public int IsClosed { get; set; }



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

                    // Include IsClosed in the SELECT statement
                    string query = "SELECT EventID, EventName, EventDate, Category, Location, IsClosed FROM Events WHERE EventDate >= CURDATE()";

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
                                    Location = reader.GetString("Location"),
                                    IsClosed = reader.GetInt32("IsClosed") // Add this line
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
                    var command = new MySqlCommand("INSERT INTO Events (EventName, EventDate, Category, Location) VALUES (@name, @date, @category, @location)", conn);
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
        public void CloseEvent(int eventId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Set IsClosed to 0 (closed) when the event is closed
                command.CommandText = @"
            UPDATE Events
            SET IsClosed = 0
            WHERE EventID = @EventID";

                command.Parameters.AddWithValue("@EventID", eventId);
                int rowsAffected = command.ExecuteNonQuery(); // Execute the command

                // Debug log
                Console.WriteLine($"CloseEvent executed: EventID = {eventId}, Rows affected: {rowsAffected}");
            }
        }



    }
}

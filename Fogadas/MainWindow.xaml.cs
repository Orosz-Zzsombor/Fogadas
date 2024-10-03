using MySql.Data.MySqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fogadas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private EventService eventService; // Declare the EventService
        private List<Event> events;
        public MainWindow()
        {
            InitializeComponent();
            eventService = new EventService(); // Initialize the EventService
            CreateDatabase();
            LoadAndDisplayEvents();
        }
        private void CreateDatabase()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Bets (
                  BetsID INT AUTO_INCREMENT PRIMARY KEY,   
                  BetDate DATE NOT NULL,                   
                  Odds FLOAT NOT NULL,                     
                  Amount INT NOT NULL,                     
                  BettorsID INT NOT NULL,                  
                  EventID INT NOT NULL,                    
                  Status BOOLEAN NOT NULL,             
                  FOREIGN KEY (BettorsID) REFERENCES Bettors(BettorsID),
                  FOREIGN KEY (EventID) REFERENCES Events(EventID)
                );

                CREATE TABLE  IF NOT EXISTS Bettors (
                  BettorsID INT AUTO_INCREMENT PRIMARY KEY,  
                  Username VARCHAR(50) NOT NULL,
                  Password VARCHAR(255),              
                  Balance INT NOT NULL,                      
                  Email VARCHAR(100) NOT NULL,               
                  JoinDate DATE NOT NULL,                    
                  IsActive BOOLEAN NOT NULL DEFAULT 1,       
                  Role ENUM('user', 'admin', 'organizer') NOT NULL DEFAULT 'user'
                );

                CREATE TABLE  IF NOT EXISTS Events (
                  EventID INT AUTO_INCREMENT PRIMARY KEY,     
                  EventName VARCHAR(100) NOT NULL,            
                  EventDate DATE NOT NULL,                    
                  Category VARCHAR(50) NOT NULL,             
                  Location VARCHAR(100) NOT NULL             
                );";
                command.ExecuteNonQuery();
            }
        }
        private void LoadAndDisplayEvents()
        {
            List<Event> events = eventService.GetCurrentEvents();
            DisplayEvents(events);
        }

        private void DisplayEvents(List<Event> events)
        {
            EventsStackPanel.Children.Clear(); // Clear previous entries

            foreach (var evt in events)
            {
                StackPanel eventPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                TextBlock eventText = new TextBlock
                {
                    Text = $"{evt.EventDate.ToShortDateString()} | {evt.EventName} - {evt.Category}",
                    Width = 500,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Button betButton = new Button
                {
                    Content = "FOGADÁS",
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(-50, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Button modifyButton = new Button
                {
                    Content = "MÓDOSÍTÁS",
                    Width = 100,
                    Margin = new Thickness(-320, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                // Assign the Click event handler to the button
                modifyButton.Click += (s, e) =>
                {
                    var updateEventWindow = new UpdateEventWindow(evt);
                    updateEventWindow.ShowDialog();
                    LoadAndDisplayEvents(); // Refresh the event list
                };

                betButton.Click += (s, e) => OpenEventDetails(evt);
                eventPanel.Children.Add(eventText);
                eventPanel.Children.Add(betButton);
                eventPanel.Children.Add(modifyButton);
                EventsStackPanel.Children.Add(eventPanel);
            }
        }

        private void OpenEventDetails(Event evt)
        {
            // Create and show the EventDetailsWindow
            EventDetailsWindow eventDetailsWindow = new EventDetailsWindow(evt);
            eventDetailsWindow.ShowDialog(); // Show as a dialog to keep focus on it
        }


        // Sort events by Category and update the UI
        private bool isSortedByCategoryAsc = true; // Default to ascending order
        private bool isSortedByDateAsc = true; // Default to ascending order

        private void SortByCategory_Click(object sender, RoutedEventArgs e)
        {
            List<Event> events = eventService.GetCurrentEvents();

            if (isSortedByCategoryAsc)
            {
                events = events.OrderBy(evt => evt.Category).ToList(); // Sort ascending
            }
            else
            {
                events = events.OrderByDescending(evt => evt.Category).ToList(); // Sort descending
            }

            isSortedByCategoryAsc = !isSortedByCategoryAsc; // Toggle sorting order
            DisplayEvents(events); // Method to display events in EventsStackPanel
        }

        private void SortByDate_Click(object sender, RoutedEventArgs e)
        {
            List<Event> events = eventService.GetCurrentEvents();

            if (isSortedByDateAsc)
            {
                events = events.OrderBy(evt => evt.EventDate).ToList(); // Sort ascending
            }
            else
            {
                events = events.OrderByDescending(evt => evt.EventDate).ToList(); // Sort descending
            }

            isSortedByDateAsc = !isSortedByDateAsc; // Toggle sorting order
            DisplayEvents(events); // Method to display events in EventsStackPanel
        }

        private void CreateNewEventButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of EventService to pass to the new window
            EventService eventService = new EventService();

            // Create a new instance of CreateEventWindow
            CreateEventWindow createEventWindow = new CreateEventWindow(eventService);

            // Show the window as a dialog
            bool? result = createEventWindow.ShowDialog();

            // Optionally refresh the event list if the dialog was successful
            if (result == true)
            {
                LoadAndDisplayEvents(); // Refresh the events
            }
        }
    }
}
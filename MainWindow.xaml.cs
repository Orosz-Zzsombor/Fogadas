using FogadasMokuskodas;
using MySql.Data.MySqlClient;
using System.Collections.Generic; // Ensure to include this namespace
using System.Linq; // Ensure to include this namespace for LINQ methods
using System.Windows;
using System.Windows.Controls;
using static FogadasMokuskodas.Bettor;
using System.Windows.Media;

namespace Fogadas
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
        private EventService eventService;
        private List<Event> events;

        // Class-level variables
        private Event selectedEvent; 
        private Bettor currentBettor; 

        public MainWindow()
        {
            InitializeComponent();
            eventService = new EventService();
            CreateDatabase();
            LoadAndDisplayEvents();

            
            currentBettor = SessionData.CurrentBettor; 
            SetButtonVisibility();
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
            events = eventService.GetCurrentEvents(); 
            DisplayEvents(events);
        }

        private void DisplayEvents(List<Event> events)
        {
            EventsStackPanel.Children.Clear();
            currentBettor = SessionData.CurrentBettor; 

            string currentUserRole = currentBettor?.Role ?? "guest"; 

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

                eventPanel.Children.Add(eventText); 

                // Show the bet button only for users
                if (currentUserRole == "user")
                {
                    Button betButton = new Button
                    {
                        Content = "FOGADÁS",
                        Width = 100,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(-50, 0, 10, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    betButton.Click += (s, e) =>
                    {
                        selectedEvent = evt; 
                        OpenEventDetails(evt); 
                    };

                    eventPanel.Children.Add(betButton); 
                }

                if (currentUserRole == "organizer")
                {
                    Button modifyButton = new Button
                    {
                        Content = "MÓDOSÍTÁS",
                        Width = 100,
                        Margin = new Thickness(-50, 0, 10, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    modifyButton.Click += (s, e) =>
                    {
                        var updateEventWindow = new UpdateEventWindow(evt);
                        updateEventWindow.ShowDialog();
                        LoadAndDisplayEvents();
                    };

                    eventPanel.Children.Add(modifyButton);
                }

                EventsStackPanel.Children.Add(eventPanel); 
            }
        }


        private void OpenEventDetails(Event evt)
        {
            var eventDetailsWindow = new EventDetailsWindow(evt, currentBettor); 
            eventDetailsWindow.ShowDialog();
        }

        // Sort events by Category and update the UI
        private bool isSortedByCategoryAsc = true;
        private bool isSortedByDateAsc = true;

        private void SortByCategory_Click(object sender, RoutedEventArgs e)
        {
            List<Event> events = eventService.GetCurrentEvents();

            if (isSortedByCategoryAsc)
            {
                events = events.OrderBy(evt => evt.Category).ToList();
            }
            else
            {
                events = events.OrderByDescending(evt => evt.Category).ToList();
            }

            isSortedByCategoryAsc = !isSortedByCategoryAsc;
            DisplayEvents(events);
        }

        private void SortByDate_Click(object sender, RoutedEventArgs e)
        {
            List<Event> events = eventService.GetCurrentEvents();

            if (isSortedByDateAsc)
            {
                events = events.OrderBy(evt => evt.EventDate).ToList();
            }
            else
            {
                events = events.OrderByDescending(evt => evt.EventDate).ToList();
            }

            isSortedByDateAsc = !isSortedByDateAsc;
            DisplayEvents(events);
        }

        private void CreateNewEventButton_Click(object sender, RoutedEventArgs e)
        {
            CreateEventWindow createEventWindow = new CreateEventWindow(eventService);
            bool? result = createEventWindow.ShowDialog();

            if (result == true)
            {
                LoadAndDisplayEvents();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationWindow auth = new AuthenticationWindow();
            auth.Show();
            this.Hide();

            auth.Closed += (s, args) => this.Close();
        }
        private void SetButtonVisibility()
        {
            if (currentBettor?.Role == "organizer")
            {
                CreateEventButton.Visibility = Visibility.Visible;
            }
            else
            {
                CreateEventButton.Visibility = Visibility.Collapsed; 
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to log out?",
                "Logout Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                SessionData.CurrentBettor = null;
                AuthenticationWindow loginWindow = new AuthenticationWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}

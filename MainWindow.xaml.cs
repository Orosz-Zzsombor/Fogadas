﻿using MySql.Data.MySqlClient;
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
        private EventService eventService; 
        private List<Event> events;
        public MainWindow()
        {
            InitializeComponent();
            eventService = new EventService(); 
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
            EventsStackPanel.Children.Clear();
            foreach (var evt in events)
            {
                Border eventBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3748")),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 5, 0, 5)
                };

                Grid eventGrid = new Grid();
                eventGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                eventGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                eventGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                eventGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            
                TextBlock dateText = new TextBlock
                {
                    Text = evt.EventDate.ToShortDateString(),
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(15, 0, 15, 0)
                };
                Grid.SetColumn(dateText, 0);


                StackPanel eventInfo = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(15, 0, 15, 0)
                };

                TextBlock nameText = new TextBlock
                {
                    Text = evt.EventName,
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold
                };

                TextBlock categoryText = new TextBlock
                {
                    Text = evt.Category,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                    FontSize = 12,
                    Margin = new Thickness(0, 3, 0, 0)
                };

                eventInfo.Children.Add(nameText);
                eventInfo.Children.Add(categoryText);
                Grid.SetColumn(eventInfo, 1);

           
                Button modifyButton = new Button
                {
                    Content = "MÓDOSÍTÁS",
                    Style = (Style)FindResource("EventButtonStyle"),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A5568"))
                };
                Grid.SetColumn(modifyButton, 2);

               
                Button betButton = new Button
                {
                    Content = "FOGADÁS",
                    Style = (Style)FindResource("EventButtonStyle")
                };
                Grid.SetColumn(betButton, 3);

                
                modifyButton.Click += (s, e) =>
                {
                    var updateEventWindow = new UpdateEventWindow(evt);
                    updateEventWindow.ShowDialog();
                    LoadAndDisplayEvents();
                };
                betButton.Click += (s, e) => OpenEventDetails(evt);

            
                eventGrid.Children.Add(dateText);
                eventGrid.Children.Add(eventInfo);
                eventGrid.Children.Add(modifyButton);
                eventGrid.Children.Add(betButton);

                eventBorder.Child = eventGrid;
                EventsStackPanel.Children.Add(eventBorder);
            }
        }

        private void OpenEventDetails(Event evt)
        {
            
            EventDetailsWindow eventDetailsWindow = new EventDetailsWindow(evt);
            eventDetailsWindow.ShowDialog();
        }


 
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
           
            EventService eventService = new EventService();


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

     
        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
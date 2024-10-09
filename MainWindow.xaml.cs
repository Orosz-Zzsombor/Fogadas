using FogadasMokuskodas;
using MySql.Data.MySqlClient;
using System.Collections.Generic; 
using System.Linq; 
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


        private Event selectedEvent;
        private Bettor currentBettor;

        public MainWindow()
        {
            InitializeComponent();
            eventService = new EventService();
            CreateDatabase();
            LoadAndDisplayEvents();
            UpdateBalanceDisplay();
            currentBettor = SessionData.CurrentBettor;
            UpdateUserInterface();
            SetButtonVisibility();
        }
        private void UpdateUserInterface()
        {
            if (currentBettor != null)
            {

                btnLogin.Visibility = Visibility.Collapsed;
                btnRegister.Visibility = Visibility.Collapsed;


                txtUsername.Text = currentBettor.Username;
                txtUsername.Visibility = Visibility.Visible;
            }
            else
            {

                btnLogin.Visibility = Visibility.Visible;
                btnRegister.Visibility = Visibility.Visible;
                txtUsername.Visibility = Visibility.Collapsed;
            }
        }
        private void CreateDatabase()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Bettors (
          BettorsID INT AUTO_INCREMENT PRIMARY KEY,  
          Username VARCHAR(50) NOT NULL,
          Password VARCHAR(255),              
          Balance INT NOT NULL,                      
          Email VARCHAR(100) NOT NULL,                
          JoinDate DATE NOT NULL,                    
          IsActive BOOLEAN NOT NULL DEFAULT 1,       
          Role ENUM('user', 'admin', 'organizer') NOT NULL DEFAULT 'user'
        );

        CREATE TABLE IF NOT EXISTS Events (
          EventID INT AUTO_INCREMENT PRIMARY KEY,     
          EventName VARCHAR(100) NOT NULL,            
          EventDate DATE NOT NULL,                    
          Category VARCHAR(50) NOT NULL,             
          Location VARCHAR(100) NOT NULL,
          IsClosed BOOLEAN NOT NULL DEFAULT 1
        );

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
        );";
                command.ExecuteNonQuery();
            }
        }
    
    
        public void UpdateBalanceDisplay()
        {
            if (currentBettor != null)
            {
                BalanceTextBlock.Text = $"{currentBettor.Balance:C}"; 
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

                Border eventBorder = new Border
                {
                    Background = new SolidColorBrush(evt.IsClosed == 0 ? Colors.Gray : (Color)ColorConverter.ConvertFromString("#2D3748")),
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

                eventGrid.Children.Add(dateText);
                eventGrid.Children.Add(eventInfo);


                if (evt.IsClosed == 0) 
                {

                    foreach (UIElement element in eventGrid.Children)
                    {
                        if (element is Button button)
                        {
                            button.IsEnabled = false; 
                        }
                    }
                }
                else 
                {
                    if (currentUserRole == "organizer")
                    {
                    
                        Button modifyButton = new Button
                        {
                            Content = "MÓDOSÍTÁS",
                            Style = (Style)FindResource("EventButtonStyle"),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A5568"))
                        };
                        Grid.SetColumn(modifyButton, 2);

                        modifyButton.Click += (s, e) =>
                        {
                            var updateEventWindow = new UpdateEventWindow(evt);
                            updateEventWindow.ShowDialog();
                            LoadAndDisplayEvents();
                        };

                        eventGrid.Children.Add(modifyButton);

                   
                        Button closeButton = new Button
                        {
                            Content = "Lezárás",
                            Style = (Style)FindResource("EventButtonStyle"),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53E3E")) // Red for closure
                        };
                        Grid.SetColumn(closeButton, 3);

                        closeButton.Click += (s, e) =>
                        {
                            
                            var result = MessageBox.Show("Was the event successful?", "Event Closure",
                                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            if (result != MessageBoxResult.Cancel)
                            {
                               
                                eventService.CloseEvent(evt.EventID); 

                            
                                LoadAndDisplayEvents();

                               
                                if (result == MessageBoxResult.Yes)
                                {
                                    MessageBox.Show("Event marked as successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else if (result == MessageBoxResult.No)
                                {
                                    MessageBox.Show("Event marked as unsuccessful.", "Failure", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Event closure cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        };

                        eventGrid.Children.Add(closeButton);
                    }

                    if (currentUserRole == "user")
                    {
                        // Bet Button
                        Button betButton = new Button
                        {
                            Content = "FOGADÁS",
                            Style = (Style)FindResource("EventButtonStyle")
                        };
                        Grid.SetColumn(betButton, 3);

                        betButton.Click += (s, e) => OpenEventDetails(evt);

                        eventGrid.Children.Add(betButton);
                    }
                }

                // Add the event grid to the border
                eventBorder.Child = eventGrid;
                // Finally add the event border to the stack panel
                EventsStackPanel.Children.Add(eventBorder);
            }
        }



        private void OpenEventDetails(Event evt)
        {
            var eventDetailsWindow = new EventDetailsWindow(evt, currentBettor); 
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

       

        private void CreateEventButton_Click(object sender, RoutedEventArgs e)
        {
            CreateEventWindow createEventWindow = new CreateEventWindow(eventService);
            bool? result = createEventWindow.ShowDialog();

            if (result == true)
            {
                LoadAndDisplayEvents();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
        private void btnMyBets_Click(object sender, RoutedEventArgs e)
        {
            if (currentBettor == null)
            {
                MessageBox.Show("You must be logged in to view your bets.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MyBetsWindow myBetsWindow = new MyBetsWindow(currentBettor);
            myBetsWindow.ShowDialog();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

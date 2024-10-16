﻿using FogadasMokuskodas;
using MySql.Data.MySqlClient;
using System.Collections.Generic; 
using System.Linq; 
using System.Windows;
using System.Windows.Controls;
using static FogadasMokuskodas.Bettor;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Effects;


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


                bool isOrganizer = currentBettor.Role == "organizer";

                foreach (var child in SidebarPanel.Children)
                {
                    if (child is Button button && button != btnLogout)
                    {
                        button.IsEnabled = !isOrganizer;
                        button.Foreground = isOrganizer ? Brushes.Gray : Brushes.White;
                    }
                }
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

                BalanceTextBlock.Text = $"{currentBettor.Balance:N0} Ft"; 

            }
        }
        private void LoadAndDisplayEvents()
        {
            events = eventService.GetCurrentEvents(); 
            DisplayEvents(events);
        }

        private Dictionary<string, string> categoryIcons = new Dictionary<string, string>
            {
                { "Football", "⚽️" },  
                { "Tennis", "🎾" },    
                { "Basketball", "🏀" },  
                { "Volleyball", "🏐" }  
            };
        private string GetCategoryIcon(string category)
        {
            return categoryIcons.TryGetValue(category, out var icon) ? icon : "ℹ️"; 
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
                    Background = new SolidColorBrush(evt.IsClosed == 0
                        ? (Color)ColorConverter.ConvertFromString("#1E1E1E")
                        : (Color)ColorConverter.ConvertFromString("#161B22")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30363D")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 6, 0, 6),
                    Effect = new DropShadowEffect
                    {
                        ShadowDepth = 1,
                        Opacity = 0.2,
                        BlurRadius = 4,
                        Color = Colors.Black
                    }
                };

                Grid eventGrid = new Grid();
                eventGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); 
                eventGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); 
                eventGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); 
                eventGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); 

       
                Border dateBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21262D")),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 0, 16, 0)
                };

                TextBlock dateText = new TextBlock
                {
                    Text = evt.EventDate.ToShortDateString(),
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };

                dateBorder.Child = dateText;
                Grid.SetColumn(dateBorder, 0);

              
                StackPanel eventInfo = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 0, 16, 0)
                };

                TextBlock nameText = new TextBlock
                {
                    Text = evt.EventName,
                    Foreground = Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                TextBlock categoryText = new TextBlock
                {
                    Text = $"{GetCategoryIcon(evt.Category)} {evt.Category}",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E")),
                    FontSize = 13,
                    Margin = new Thickness(0, 4, 0, 0)
                };

                eventInfo.Children.Add(nameText);
                eventInfo.Children.Add(categoryText);
                Grid.SetColumn(eventInfo, 1);

              
                Border oddsBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D333B")),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 0, 16, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                TextBlock oddsText = new TextBlock
                {
                    Text = $"{evt.Odds:N2}x",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58A6FF")),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };

                oddsBorder.Child = oddsText;
                Grid.SetColumn(oddsBorder, 2);

                eventGrid.Children.Add(dateBorder);
                eventGrid.Children.Add(eventInfo);
                eventGrid.Children.Add(oddsBorder);

                if (evt.IsClosed == 0)
                {
                    eventBorder.Opacity = 0.6;

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
                        var buttonStack = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 0, 0, 0)
                        };


                        Button modifyButton = new Button
                        {
                            Content = "MÓDOSÍTÁS",
                            Style = (Style)FindResource("EventButtonStyle"),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D333B")),
                            Margin = new Thickness(0, 0, 8, 0),
                            Padding = new Thickness(16, 8, 16, 8),
                            FontSize = 13,
                            FontWeight = FontWeights.SemiBold
                        };

                        Button closeButton = new Button
                        {
                            Content = "LEZÁRÁS",
                            Style = (Style)FindResource("EventButtonStyle"),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21262D")),
                            Padding = new Thickness(16, 8, 16, 8),
                            FontSize = 13,
                            FontWeight = FontWeights.SemiBold
                        };


                        modifyButton.Click += (s, e) =>
                        {
                            var updateEventWindow = new UpdateEventWindow(evt);
                            updateEventWindow.ShowDialog();
                            LoadAndDisplayEvents();
                        };

                        closeButton.Click += (s, e) =>
                        {
                            var closeEventWindow = new CloseEventWindow(evt, eventService);
                            bool? result = closeEventWindow.ShowDialog();

                            if (result == true)
                            {
                                LoadAndDisplayEvents();
                            }
                            else
                            {
                                MessageBox.Show("Event closure cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        };


                        buttonStack.Children.Add(modifyButton);
                        buttonStack.Children.Add(closeButton);
                        Grid.SetColumn(buttonStack, 3);
                        eventGrid.Children.Add(buttonStack);
                    }

                    if (currentUserRole == "user")
                    {

                        Button betButton = new Button
                        {
                            Content = "FOGADÁS",
                            Style = (Style)FindResource("EventButtonStyle"),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38B2AC")),
                            Padding = new Thickness(20, 8, 20, 8),
                            FontSize = 13,
                            FontWeight = FontWeights.Bold
                        };

                        betButton.Click += (s, e) => OpenEventDetails(evt);
                        Grid.SetColumn(betButton, 3);

                        eventGrid.Children.Add(betButton);
                    }
                }

                eventBorder.Child = eventGrid;

                EventsStackPanel.Children.Add(eventBorder);
            }
        }

        private void OpenEventDetails(Event evt)
        {
            var eventDetailsWindow = new EventDetailsWindow(evt, currentBettor, this); 
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
            AuthenticationWindow auth = new AuthenticationWindow();
            auth.Show();
            auth.SwitchToRegister(); 
            this.Hide();

            auth.Closed += (s, args) => this.Close();
        }
        private void OnUsernameUpdated(string newUsername)
        {
          
            txtUsername.Text = newUsername; 
        }
        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (currentBettor == null)
            {

                MessageBox.Show("You must be logged in to view your profile.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }
            var profileWindow = new ProfileWindow(currentBettor);
            profileWindow.UsernameUpdated += OnUsernameUpdated; 
            profileWindow.Show();
        }

        private void btnWallet_Click(object sender, RoutedEventArgs e)
        {
            if (currentBettor == null)
            {
                MessageBox.Show("You must be logged in to view your wallet.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var wallet = new WalletWindow(currentBettor,this);

            wallet.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hamarosan");
        }

 
    }
}

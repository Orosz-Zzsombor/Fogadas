using System;
using System.Windows;

namespace Fogadas
{
    public partial class EventDetailsWindow : Window
    {
        public EventDetailsWindow(Event evt)
        {
            InitializeComponent();
            DisplayEventDetails(evt);
        }

        private void DisplayEventDetails(Event evt)
        {
            EventNameTextBlock.Text = evt.EventName;
            EventDateTextBlock.Text = $"Date: {evt.EventDate.ToShortDateString()}";
            CategoryTextBlock.Text = $"Category: {evt.Category}";
            LocationTextBlock.Text = $"Location: {evt.Location}";
          
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void PlaceBet_Click(object sender, RoutedEventArgs e)
        {
          
            if (decimal.TryParse(BetAmountTextBox.Text, out decimal betAmount) && betAmount > 0)
            {
        
                MessageBox.Show($"Bet of {betAmount:C} placed successfully!"); 
                Close(); 
            }
            else
            {
                MessageBox.Show("Please enter a valid bet amount greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

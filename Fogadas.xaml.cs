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
            // You can add more details if needed, e.g., Description, Odds, etc.
        }

        private void PlaceBet_Click(object sender, RoutedEventArgs e)
        {
            // Validate and process the bet amount
            if (decimal.TryParse(BetAmountTextBox.Text, out decimal betAmount) && betAmount > 0)
            {
                // Logic to handle the bet placement
                MessageBox.Show($"Bet of {betAmount:C} placed successfully!"); // Placeholder for actual logic
                Close(); // Close the window after placing the bet
            }
            else
            {
                MessageBox.Show("Please enter a valid bet amount greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

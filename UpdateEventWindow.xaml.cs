using System;
using System.Windows;

namespace Fogadas
{
    public partial class UpdateEventWindow : Window
    {
        private EventService eventService;
        private Event eventToUpdate;

        public UpdateEventWindow(Event eventToUpdate)
        {
            InitializeComponent();
            this.eventService = new EventService();
            this.eventToUpdate = eventToUpdate;

            // Load existing event data into the fields
            EventIDTextBox.Text = eventToUpdate.EventID.ToString();
            EventNameTextBox.Text = eventToUpdate.EventName;
            EventDateTextBox.Text = eventToUpdate.EventDate.ToString("yyyy-MM-dd");
            EventCategoryTextBox.Text = eventToUpdate.Category;
            EventLocationTextBox.Text = eventToUpdate.Location;
        }

        private void UpdateEventButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new Event object with the updated values
            var updatedEvent = new Event
            {
                EventID = eventToUpdate.EventID,
                EventName = EventNameTextBox.Text,
                EventDate = DateTime.Parse(EventDateTextBox.Text),
                Category = EventCategoryTextBox.Text,
                Location = EventLocationTextBox.Text
            };

            // Call the event service to update the event in the database
            eventService.UpdateEvent(updatedEvent);

            // Close the window after updating
            MessageBox.Show("Event updated successfully!");
            this.Close();
        }

        private void DeleteEventButton_Click(object sender, RoutedEventArgs e)
        {
            // Ask for confirmation before deleting
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this event?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                // Call the event service to delete the event
                eventService.DeleteEvent(eventToUpdate.EventID);
                MessageBox.Show("Event deleted successfully!");
                this.Close(); // Close the window after deleting
            }
        }
    }
}

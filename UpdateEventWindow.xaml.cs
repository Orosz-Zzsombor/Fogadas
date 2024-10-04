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

          
            EventIDTextBox.Text = eventToUpdate.EventID.ToString();
            EventNameTextBox.Text = eventToUpdate.EventName;
            EventDateTextBox.Text = eventToUpdate.EventDate.ToString("yyyy-MM-dd");
            EventCategoryTextBox.Text = eventToUpdate.Category;
            EventLocationTextBox.Text = eventToUpdate.Location;
        }

        private void UpdateEventButton_Click(object sender, RoutedEventArgs e)
        {

            var updatedEvent = new Event
            {
                EventID = eventToUpdate.EventID,
                EventName = EventNameTextBox.Text,
                EventDate = DateTime.Parse(EventDateTextBox.Text),
                Category = EventCategoryTextBox.Text,
                Location = EventLocationTextBox.Text
            };

      
            eventService.UpdateEvent(updatedEvent);

        
            MessageBox.Show("Event updated successfully!");
            this.Close();
        }

        private void DeleteEventButton_Click(object sender, RoutedEventArgs e)
        {

            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this event?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
            
                eventService.DeleteEvent(eventToUpdate.EventID);
                MessageBox.Show("Event deleted successfully!");
                this.Close();
            }
        }
    }
}

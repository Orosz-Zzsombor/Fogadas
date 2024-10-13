using System;
using System.Runtime.CompilerServices;
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
                Location = EventLocationTextBox.Text,
              
            };

           
            eventService.UpdateEvent(updatedEvent);

            
            MessageBox.Show("Event updated successfully!");
            this.Close();
        }

        
        private void DeleteEventButton_Click(object sender, RoutedEventArgs e)
        {
          
            var betsForEvent = eventService.GetBetsForEvent(eventToUpdate.EventID);

         
            if (betsForEvent != null && betsForEvent.Count > 0)
            {
                MessageBox.Show("This event cannot be deleted because there are bets placed on it.", "Delete Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
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


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

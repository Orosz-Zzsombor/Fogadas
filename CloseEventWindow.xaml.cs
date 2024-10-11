using System.Windows;
using Fogadas;

namespace FogadasMokuskodas
{
    public partial class CloseEventWindow : Window
    {
        private Event _event;
        private EventService _eventService;

        public CloseEventWindow(Event evt, EventService eventService)
        {
            InitializeComponent();
            _event = evt;
            _eventService = eventService;
        }

        private void btnSuccess_Click(object sender, RoutedEventArgs e)
        {
            _eventService.CloseEvent(_event.EventID, true); 
            MessageBox.Show("Event marked as successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true; 
            Close();
        }

        private void btnFailure_Click(object sender, RoutedEventArgs e)
        {
            _eventService.CloseEvent(_event.EventID, false); 
            MessageBox.Show("Event marked as unsuccessful.", "Failure", MessageBoxButton.OK, MessageBoxImage.Warning);
            this.DialogResult = true; 
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}

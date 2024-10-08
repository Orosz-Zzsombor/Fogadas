﻿using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace Fogadas
{
    public partial class CreateEventWindow : Window
    {
        private EventService eventService;

        public CreateEventWindow(EventService service)
        {
            InitializeComponent();
            eventService = service; 
        }

        private void CreateEventButton_Click(object sender, RoutedEventArgs e)
        {
            string eventName = EventNameTextBox.Text;
            DateTime eventDate;

            
            if (!DateTime.TryParse(EventDateTextBox.Text, out eventDate) || eventDate < DateTime.Now)
            {
                MessageBox.Show("Please enter a valid future date.", "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string category = EventCategoryTextBox.Text;
            string location = EventLocationTextBox.Text;

          
            bool isCreated = eventService.CreateEvent(eventName, eventDate, category, location);

            if (isCreated)
            {
                MessageBox.Show("Event created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; 
                this.Close(); 
            }
            else
            {
                MessageBox.Show("Failed to create event. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
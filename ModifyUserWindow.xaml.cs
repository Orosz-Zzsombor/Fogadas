using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FogadasMokuskodas
{
    /// <summary>
    /// Interaction logic for ModifyUserWindow.xaml
    /// </summary>
    public partial class ModifyUserWindow : Window
    {
        public Bettor ModifiedUser { get; private set; }

        public ModifyUserWindow(Bettor user)
        {
            ModifiedUser = new Bettor
            {
                BettorsID = user.BettorsID,
                Username = user.Username,
                Email = user.Email,
                Balance = user.Balance
            };

            InitializeComponent();
            DataContext = ModifiedUser;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

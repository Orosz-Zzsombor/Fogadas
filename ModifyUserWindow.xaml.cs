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
using System.Security.Cryptography;
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
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            ModifiedUser.Password = ComputeSha256Hash(newPassword);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Windows;
using MediTrack.Core;
using MediTrack.Data.Models;

namespace MediTrack.UI
{
    public partial class MainWindow : Window
    {
        private readonly AuthService _authService;

        public MainWindow()
        {
            Application.LoadComponent(this,
                new Uri("/MediTrack.UI;component/MainWindow.xaml", UriKind.Relative));
            _authService = new AuthService();

            // Create default admin on first startup
            _authService.SeedDefaultAdmin();
            AuthService.SeedSampleClinicData();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var usernameBox = FindName("TxtUsername") as System.Windows.Controls.TextBox;
            var passwordBox = FindName("TxtPassword") as System.Windows.Controls.PasswordBox;

            string username = usernameBox?.Text.Trim() ?? string.Empty;
            string password = passwordBox?.Password ?? string.Empty;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            // Authenticate user against DB
            User? loggedInUser = _authService.AuthenticateUser(username, password);

            if (loggedInUser != null)
            {
                MessageBox.Show($"Welcome, {loggedInUser.FullName}! Role: {loggedInUser.Role}", 
                                "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                DashboardWindow dashboard = new DashboardWindow(loggedInUser);
                dashboard.Show();
                this.Close();
            }
            else
            {
                ShowError("Invalid username or password.");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow { Owner = this };
            if (registerWindow.ShowDialog() == true)
            {
                if (FindName("TxtUsername") is System.Windows.Controls.TextBox usernameBox)
                {
                    usernameBox.Text = registerWindow.RegisteredUsername;
                }

                if (FindName("TxtPassword") is System.Windows.Controls.PasswordBox passwordBox)
                {
                    passwordBox.Clear();
                }

                if (FindName("LblError") is System.Windows.Controls.TextBlock errorLabel)
                {
                    errorLabel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ShowError(string message)
        {
            if (FindName("LblError") is System.Windows.Controls.TextBlock errorLabel)
            {
                errorLabel.Text = message;
                errorLabel.Visibility = Visibility.Visible;
            }
        }
    }
}
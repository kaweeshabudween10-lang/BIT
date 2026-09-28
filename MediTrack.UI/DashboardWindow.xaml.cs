using System.Windows;
using System.Windows.Controls;
using MediTrack.Data.Models;
using MediTrack.UI.Pages;

namespace MediTrack.UI
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            // Display logged-in user information
            ((System.Windows.Controls.TextBlock)FindName("TxtWelcome")).Text = $"Welcome, {_currentUser.FullName}";
            ((System.Windows.Controls.TextBlock)FindName("TxtRole")).Text = $"Role: {_currentUser.Role}";

            // Apply Role-Based Access Control
            ApplyRolePermissions();

            // Load the dashboard as the default starting page
            MainFrame.Navigate(new DashboardPage());
        }

        private readonly User _currentUser;

        private void ApplyRolePermissions()
        {
            // Default: Hide specialized menu buttons
            var userManagementButton = (System.Windows.Controls.Control)FindName("BtnUserManagement");
            var prescriptionsButton = (System.Windows.Controls.Control)FindName("BtnPrescriptions");
            var billingButton = (System.Windows.Controls.Control)FindName("BtnBilling");
            userManagementButton.Visibility = Visibility.Collapsed;
            prescriptionsButton.Visibility = Visibility.Collapsed;
            billingButton.Visibility = Visibility.Collapsed;

            // Configure menu visibility based on user role
            switch (_currentUser.Role)
            {
                case "Admin":
                    // Admin gets access to all modules
                    userManagementButton.Visibility = Visibility.Visible;
                    prescriptionsButton.Visibility = Visibility.Visible;
                    billingButton.Visibility = Visibility.Visible;
                    break;

                case "Doctor":
                    // Doctor sees Patient records, Appointments, and Prescription drafting
                    prescriptionsButton.Visibility = Visibility.Visible;
                    break;

                case "Receptionist":
                    // Receptionist manages Patients, Appointments, and Billing/Invoices
                    billingButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Return to login screen
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        private void BtnPatients_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientPage());
        }

        private void BtnAppointments_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AppointmentsPage());
        }

        private void BtnPrescriptions_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PrescriptionsPage());
        }

        private void BtnBilling_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BillingPage());
        }

        private void BtnUserManagement_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UserManagementPage());
        }
    }
}
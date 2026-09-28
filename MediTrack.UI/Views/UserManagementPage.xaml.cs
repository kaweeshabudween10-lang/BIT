using System;
using System.Windows;
using System.Windows.Controls;
using MediTrack.Core;

namespace MediTrack.UI.Pages
{
    public partial class UserManagementPage : Page
    {
        private readonly UserService _userService = new UserService();

        public UserManagementPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                GridUsers.ItemsSource = _userService.GetAllUsers();
            }
            catch (Exception ex)
            {
                ShowError($"Unable to load user accounts: {ex.Message}");
            }
        }

        private void BtnCreateUser_Click(object sender, RoutedEventArgs e)
        {
            if (CmbRole.SelectedItem is not ComboBoxItem selectedRole ||
                selectedRole.Content is not string role)
            {
                ShowError("Please select a role.");
                return;
            }

            try
            {
                var result = _userService.RegisterUser(
                    TxtUsername.Text,
                    TxtFullName.Text,
                    TxtPassword.Password,
                    role,
                    TxtSpecialization.Text);

                MessageBox.Show(result.Message,
                    result.Success ? "Account Created" : "Unable to Create Account",
                    MessageBoxButton.OK,
                    result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (result.Success)
                {
                    TxtFullName.Clear();
                    TxtUsername.Clear();
                    TxtPassword.Clear();
                    TxtSpecialization.Clear();
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Unable to create user account: {ex.Message}");
            }
        }

        private void CmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DoctorDetailsPanel is null)
            {
                return;
            }

            bool isDoctor = CmbRole.SelectedItem is ComboBoxItem selectedRole &&
                            string.Equals(selectedRole.Content?.ToString(), "Doctor", StringComparison.OrdinalIgnoreCase);
            DoctorDetailsPanel.Visibility = isDoctor ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: int userId })
            {
                ShowError("Unable to identify the selected user.");
                return;
            }

            try
            {
                var result = _userService.ToggleUserStatus(userId);
                MessageBox.Show(result.Message,
                    result.Success ? "Account Updated" : "Unable to Update Account",
                    MessageBoxButton.OK,
                    result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);

                if (result.Success)
                {
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Unable to update user status: {ex.Message}");
            }
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(message, "User Management", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
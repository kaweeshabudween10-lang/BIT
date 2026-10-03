using System;
using System.Windows;
using System.Windows.Media;
using MediTrack.Core;

namespace MediTrack.UI;

public partial class RegisterWindow : Window
{
    private readonly UserService _userService = new();

    public string RegisteredUsername { get; private set; } = string.Empty;

    public RegisterWindow()
    {
        InitializeComponent();
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        var username = TxtUsername.Text.Trim();
        try
        {
            var result = _userService.RegisterUser(
                username,
                TxtFullName.Text,
                TxtPassword.Password,
                "Receptionist");

            ShowMessage(result.Message, result.Success);
            if (!result.Success)
            {
                return;
            }

            RegisteredUsername = username;
            MessageBox.Show(this, "Your account is ready. You can now log in.",
                "Registration complete", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ShowMessage($"Unable to register: {ex.Message}", false);
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowMessage(string message, bool success)
    {
        LblMessage.Text = message;
        LblMessage.Foreground = success ? Brushes.DarkGreen : Brushes.Firebrick;
    }
}
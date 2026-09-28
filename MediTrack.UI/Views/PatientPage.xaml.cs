using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MediTrack.Data;

namespace MediTrack.UI.Pages
{
    public partial class PatientPage : Page
    {
        public PatientPage()
        {
            InitializeComponent();
            LoadPatientData();
        }

        private void LoadPatientData()
        {
            try
            {
                using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
                GridPatients.ItemsSource = db.Patients
                    .OrderByDescending(patient => patient.PatientId)
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var query = TxtSearch.Text.Trim().ToLower();

            try
            {
                using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
                GridPatients.ItemsSource = db.Patients
                    .Where(patient => patient.FullName.ToLower().Contains(query) ||
                                      patient.NIC.ToLower().Contains(query) ||
                                      patient.Phone.ToLower().Contains(query))
                    .OrderByDescending(patient => patient.PatientId)
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
            LoadPatientData();
        }

        private static void ShowDatabaseError(Exception ex)
        {
            MessageBox.Show($"Error loading patient data: {ex.Message}",
                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
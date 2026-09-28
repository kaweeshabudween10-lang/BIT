using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using QuestPDF.Fluent;
using MediTrack.Core;
using MediTrack.Data;
using MediTrack.Data.Models;

namespace MediTrack.UI.Pages
{
    public partial class PrescriptionsPage : Page
    {
        private readonly List<Prescription> _rxItems = new List<Prescription>();

        public PrescriptionsPage()
        {
            InitializeComponent();
            LoadPeople();
        }

        private void LoadPeople()
        {
            try
            {
                using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
                CmbPrescriptionPatient.ItemsSource = db.Patients.OrderBy(patient => patient.FullName).ToList();
                CmbPrescriptionDoctor.ItemsSource = db.Users
                    .Where(user => user.IsActive && user.Role.ToLower() == "doctor")
                    .OrderBy(user => user.FullName)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load patients and doctors: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtMedName.Text) || string.IsNullOrWhiteSpace(TxtDosage.Text))
            {
                MessageBox.Show("Please enter medicine name and dosage.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _rxItems.Add(new Prescription
            {
                MedicineName = TxtMedName.Text.Trim(),
                Dosage = TxtDosage.Text.Trim(),
                Duration = TxtDuration.Text.Trim()
            });

            GridRxItems.ItemsSource = null;
            GridRxItems.ItemsSource = _rxItems;

            TxtMedName.Clear();
            TxtDosage.Clear();
            TxtDuration.Clear();
        }

        private void BtnSavePdf_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPrescriptionPatient.SelectedItem is not Patient patient ||
                CmbPrescriptionDoctor.SelectedItem is not User doctor)
            {
                MessageBox.Show("Select a patient and an active doctor before generating the prescription.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtDiagnosis.Text) || _rxItems.Count == 0)
            {
                MessageBox.Show("Please enter a diagnosis and at least one prescribed medicine.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var visit = new Visit
                {
                    Diagnosis = TxtDiagnosis.Text.Trim(),
                    Notes = TxtNotes.Text.Trim(),
                    CreatedDate = DateTime.Now
                };

                var pdfDocument = new PrescriptionPdfDocument(patient, doctor, visit, _rxItems);
                var safePatientName = string.Concat(patient.FullName.Select(character =>
                    Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
                var fileName = $"Prescription_{safePatientName}.pdf";
                var filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                pdfDocument.GeneratePdf(filePath);

                MessageBox.Show($"Prescription generated successfully!\nSaved to Desktop: {filePath}",
                    "PDF Export", MessageBoxButton.OK, MessageBoxImage.Information);

                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF Generation failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

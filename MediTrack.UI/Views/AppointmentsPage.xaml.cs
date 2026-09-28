using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using MediTrack.Core;
using MediTrack.Data;
using MediTrack.Data.Models;

namespace MediTrack.UI.Pages
{
    public partial class AppointmentsPage : Page
    {
        private readonly AppointmentService _appointmentService;
        private readonly SlotScoringService _slotScoringService;

        public AppointmentsPage()
        {
            InitializeComponent();
            _appointmentService = new AppointmentService();
            _slotScoringService = new SlotScoringService();

            DpDate.SelectedDate = DateTime.Today;
            LoadDropdownData();
            LoadAppointmentsList();
        }

        private void BookingCriteria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSmartSlots();
        }

        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSmartSlots();
        }

        private void RefreshSmartSlots()
        {
            if (CmbPatients.SelectedValue is not int patientId ||
                CmbDoctors.SelectedValue is not int doctorId ||
                !DpDate.SelectedDate.HasValue)
            {
                LstSmartSlots.ItemsSource = null;
                return;
            }

            try
            {
                LstSmartSlots.ItemsSource = _slotScoringService.GetRankedSlots(
                    doctorId, patientId, DpDate.SelectedDate.Value);
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private void LstSmartSlots_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstSmartSlots.SelectedItem is CandidateSlot selectedSlot)
            {
                TxtTime.Text = selectedSlot.SlotTime.ToString("HH:mm");
            }
        }

        private void LoadDropdownData()
        {
            try
            {
                using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
                CmbPatients.ItemsSource = db.Patients.ToList();
                CmbDoctors.ItemsSource = db.Doctors.ToList();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private void LoadAppointmentsList()
        {
            try
            {
                using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
                GridAppointments.ItemsSource = db.Appointments
                    .Include(appointment => appointment.Patient)
                    .OrderBy(appointment => appointment.DateTime)
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPatients.SelectedValue is not int patientId ||
                CmbDoctors.SelectedValue is not int doctorId ||
                !DpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please complete all form fields.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(TxtTime.Text.Trim(), out var selectedTime) ||
                selectedTime < TimeSpan.Zero || selectedTime >= TimeSpan.FromDays(1))
            {
                MessageBox.Show("Invalid time format. Please enter time as HH:mm (e.g., 14:30).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newAppointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                DateTime = DpDate.SelectedDate.Value.Date.Add(selectedTime),
                Status = "Booked",
                SlotScore = 1.0m
            };

            try
            {
                var result = _appointmentService.BookAppointment(newAppointment);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAppointmentsList();
                    RefreshSmartSlots();
                }
                else
                {
                    MessageBox.Show(result.Message, "Scheduling Conflict",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        private static void ShowDatabaseError(Exception ex)
        {
            MessageBox.Show($"Unable to access appointment data: {ex.Message}",
                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

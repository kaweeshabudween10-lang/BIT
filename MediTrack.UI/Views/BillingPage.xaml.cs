using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MediTrack.Core;
using MediTrack.Data;

namespace MediTrack.UI.Pages
{
    public partial class BillingPage : Page
    {
        private readonly BillingInventoryService _service;

        public BillingPage()
        {
            InitializeComponent();
            _service = new BillingInventoryService();
            LoadInventoryData();
        }

        private void LoadInventoryData()
        {
            try
            {
                using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
                {
                    GridInventory.ItemsSource = db.Medicines.ToList();
                }

                var lowStockItems = _service.GetLowStockMedicines();
                if (lowStockItems.Count > 0)
                {
                    BannerLowStock.Visibility = Visibility.Visible;
                    TxtLowStockAlert.Text = $"Low Stock Warning: {lowStockItems.Count} item(s) (e.g. {lowStockItems.First().Name}) reached reorder level!";
                }
                else
                {
                    BannerLowStock.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load inventory: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotal(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TxtConsultFee.Text, out var consultationFee) &&
                decimal.TryParse(TxtMedTotal.Text, out var medicineTotal))
            {
                TxtTotalPayable.Text = $"LKR {consultationFee + medicineTotal:N2}";
            }
            else
            {
                TxtTotalPayable.Text = "Enter valid amounts";
            }
        }

        private void BtnProcessInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtVisitId.Text, out var visitId) || visitId <= 0)
            {
                MessageBox.Show("Please enter a valid Visit ID.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtConsultFee.Text, out var consultationFee) || consultationFee < 0 ||
                !decimal.TryParse(TxtMedTotal.Text, out var medicineTotal) || medicineTotal < 0)
            {
                MessageBox.Show("Please enter valid, non-negative fee amounts.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_service.CreateInvoice(visitId, consultationFee, medicineTotal, out var invoiceId))
                {
                    MessageBox.Show($"Invoice #{invoiceId} created successfully!", "Payment Processed",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtVisitId.Clear();
                    LoadInventoryData();
                }
                else
                {
                    MessageBox.Show("The invoice could not be created. Check the entered amounts.",
                        "Invoice Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to process invoice: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using MediTrack.Core;
using MediTrack.Data;

namespace MediTrack.UI.Pages
{
    public partial class DashboardPage : Page
    {
        public ISeries[] RevenueSeries { get; private set; } = Array.Empty<ISeries>();
        public Axis[] XAxes { get; private set; } = Array.Empty<Axis>();

        public DashboardPage()
        {
            InitializeComponent();
            DataContext = this;

            LoadDashboardKPIs();
            LoadChartData();
            LoadWatchlistWidget();
        }

        private void LoadDashboardKPIs()
        {
            try
            {
                using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
                var today = DateTime.Today;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

                int patientCount = db.Patients.Count();
                int todayAppointments = db.Appointments.Count(appointment =>
                    appointment.DateTime >= today &&
                    appointment.DateTime < today.AddDays(1) &&
                    appointment.Status != "Cancelled");
                decimal monthRevenue = db.Invoices
                    .Where(invoice => invoice.Date >= firstDayOfMonth && invoice.Date < firstDayOfNextMonth)
                    .Select(invoice => invoice.ConsultationFee + invoice.MedicineTotal)
                    .ToList()
                    .Sum();

                TxtTotalPatients.Text = patientCount.ToString(CultureInfo.CurrentCulture);
                TxtTodayAppointments.Text = todayAppointments.ToString(CultureInfo.CurrentCulture);
                TxtMonthlyRevenue.Text = $"LKR {monthRevenue:N2}";
            }
            catch (Exception ex)
            {
                ShowDashboardError(ex);
            }
        }

        private void LoadChartData()
        {
            try
            {
                var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var firstChartMonth = currentMonth.AddMonths(-5);
                List<(DateTime Date, decimal Total)> invoices;

                using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
                {
                    invoices = db.Invoices
                        .Where(invoice => invoice.Date >= firstChartMonth && invoice.Date < currentMonth.AddMonths(1))
                        .Select(invoice => new
                        {
                            invoice.Date,
                            invoice.ConsultationFee,
                            invoice.MedicineTotal
                        })
                        .ToList()
                        .Select(invoice => (invoice.Date, invoice.ConsultationFee + invoice.MedicineTotal))
                        .ToList();
                }

                var months = Enumerable.Range(0, 6)
                    .Select(offset => firstChartMonth.AddMonths(offset))
                    .ToArray();
                var monthlyData = months
                    .Select(month => (double)invoices
                        .Where(invoice => invoice.Date >= month && invoice.Date < month.AddMonths(1))
                        .Sum(invoice => invoice.Total))
                    .ToArray();

                RevenueSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = monthlyData,
                        Name = "Revenue (LKR)"
                    }
                };
                XAxes = new Axis[]
                {
                    new Axis { Labels = months.Select(month => month.ToString("MMM", CultureInfo.CurrentCulture)).ToArray() }
                };
                ChartRevenue.Series = RevenueSeries;
                ChartRevenue.XAxes = XAxes;
            }
            catch (Exception ex)
            {
                ShowDashboardError(ex);
            }
        }

        private void LoadWatchlistWidget()
        {
            try
            {
                var watchlist = new PatientRiskService().GenerateFollowUpWatchlist();
                GridWatchlist.ItemsSource = watchlist;
                TxtAtRiskCount.Text = watchlist.Count(item => item.RiskLevel == "High")
                    .ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                ShowDashboardError(ex);
            }
        }

        private static void ShowDashboardError(Exception ex)
        {
            MessageBox.Show($"Unable to load dashboard data: {ex.Message}", "Dashboard Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
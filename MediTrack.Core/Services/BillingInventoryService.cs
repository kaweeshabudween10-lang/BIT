using System;
using System.Collections.Generic;
using System.Linq;
using MediTrack.Data;
using MediTrack.Data.Models;

namespace MediTrack.Core
{
    public class BillingInventoryService
    {
        public (bool Success, string Message) DeductMedicineStock(int medicineId, int quantityUsed)
        {
            if (quantityUsed <= 0)
            {
                return (false, "Quantity used must be greater than zero.");
            }

            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                var medicine = db.Medicines.FirstOrDefault(item => item.MedicineId == medicineId);
                if (medicine == null)
                {
                    return (false, "Medicine item not found.");
                }

                if (medicine.Quantity < quantityUsed)
                {
                    return (false, $"Insufficient stock for {medicine.Name}. Remaining: {medicine.Quantity}");
                }

                medicine.Quantity -= quantityUsed;
                db.SaveChanges();
                return (true, "Stock updated successfully.");
            }
        }

        public List<Medicine> GetLowStockMedicines()
        {
            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                return db.Medicines
                    .Where(medicine => medicine.Quantity <= medicine.ReorderLevel)
                    .ToList();
            }
        }

        public bool CreateInvoice(int visitId, decimal consultationFee, decimal medicineTotal, out int invoiceId)
        {
            invoiceId = 0;

            if (visitId <= 0 || consultationFee < 0 || medicineTotal < 0)
            {
                return false;
            }

            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                if (!db.Visits.Any(visit => visit.VisitId == visitId))
                {
                    return false;
                }

                var invoice = new Invoice
                {
                    VisitId = visitId,
                    ConsultationFee = consultationFee,
                    MedicineTotal = medicineTotal,
                    Status = "Paid",
                    Date = DateTime.Now
                };

                db.Invoices.Add(invoice);
                db.SaveChanges();

                invoiceId = invoice.InvoiceId;
                return true;
            }
        }
    }
}
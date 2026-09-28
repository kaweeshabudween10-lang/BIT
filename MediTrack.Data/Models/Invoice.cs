using System;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Data.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }
        public int VisitId { get; set; }
        public decimal ConsultationFee { get; set; }
        public decimal MedicineTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
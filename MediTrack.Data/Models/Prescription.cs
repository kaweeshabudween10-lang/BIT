using System.ComponentModel.DataAnnotations;

namespace MediTrack.Data.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }
        public int VisitId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }
}
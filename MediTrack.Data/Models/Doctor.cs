using System.ComponentModel.DataAnnotations;

namespace MediTrack.Data.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string AvailableDays { get; set; } = string.Empty;
        public string AvgConsultMinutesByType { get; set; } = string.Empty;
    }
}
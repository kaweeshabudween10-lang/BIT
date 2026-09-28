using System;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Data.Models
{
    public class Visit
    {
        [Key]
        public int VisitId { get; set; }
        public int AppointmentId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool FollowUpRequired { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
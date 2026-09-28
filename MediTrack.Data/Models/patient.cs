using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Data.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NIC { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public bool ChronicConditionFlag { get; set; } // Used for the Smart Follow-up Engine

        // --- NAVIGATION PROPERTY ---
        // One Patient can have many Appointments
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
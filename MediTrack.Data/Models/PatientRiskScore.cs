using System;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Data.Models
{
    public class PatientRiskScore
    {
        [Key]
        public int RiskScoreId { get; set; }
        public int PatientId { get; set; }
        public decimal Score { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime LastCalculated { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
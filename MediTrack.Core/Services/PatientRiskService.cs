using System;
using System.Collections.Generic;
using System.Linq;
using MediTrack.Data;
using MediTrack.Data.Models;

namespace MediTrack.Core
{
    public class PatientRiskItem
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int CalculatedScore { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public List<string> RiskReasons { get; set; } = new List<string>();
    }

    public class PatientRiskService
    {
        private readonly Func<AppDbContext> _createDbContext;

        public PatientRiskService()
            : this(() => new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
        {
        }

        public PatientRiskService(Func<AppDbContext> createDbContext)
        {
            _createDbContext = createDbContext ?? throw new ArgumentNullException(nameof(createDbContext));
        }

        /// <summary>
        /// Evaluates patients against clinic follow-up rules and returns a sorted watchlist.
        /// </summary>
        public List<PatientRiskItem> GenerateFollowUpWatchlist()
        {
            using (var db = _createDbContext())
            {
                var patients = db.Patients.ToList();
                var appointments = db.Appointments.ToList();
                var visits = db.Visits.ToList();
                DateTime now = DateTime.Now;
                DateTime threeMonthsAgo = now.AddMonths(-3);

                var appointmentPatientIds = appointments.ToDictionary(
                    appointment => appointment.AppointmentId,
                    appointment => appointment.PatientId);

                var visitsByPatient = visits
                    .Where(visit => appointmentPatientIds.ContainsKey(visit.AppointmentId))
                    .GroupBy(visit => appointmentPatientIds[visit.AppointmentId])
                    .ToDictionary(group => group.Key, group => group.ToList());

                var recentNoShowsByPatient = appointments
                    .Where(appointment => appointment.Status == "No-show" &&
                                          appointment.DateTime >= threeMonthsAgo &&
                                          appointment.DateTime <= now)
                    .GroupBy(appointment => appointment.PatientId)
                    .ToDictionary(group => group.Key, group => group.Count());

                var watchlist = new List<PatientRiskItem>();

                foreach (var patient in patients)
                {
                    int riskScore = 0;
                    var reasons = new List<string>();
                    var patientVisits = visitsByPatient.TryGetValue(patient.PatientId, out var matchedVisits)
                        ? matchedVisits
                        : new List<Visit>();

                    var lastVisit = patientVisits
                        .OrderByDescending(visit => visit.CreatedDate)
                        .FirstOrDefault();

                    if (patient.ChronicConditionFlag &&
                        (lastVisit == null || lastVisit.CreatedDate < threeMonthsAgo))
                    {
                        riskScore += 40;
                        reasons.Add("Chronic condition with no visit in 3+ months");
                    }

                    int noShowCount = recentNoShowsByPatient.TryGetValue(patient.PatientId, out var count)
                        ? count
                        : 0;
                    if (noShowCount >= 2)
                    {
                        riskScore += 30;
                        reasons.Add($"{noShowCount} recorded missed appointments (no-shows) in the past 3 months");
                    }

                    var latestRequiredFollowUp = patientVisits
                        .Where(visit => visit.FollowUpRequired)
                        .OrderByDescending(visit => visit.CreatedDate)
                        .FirstOrDefault();

                    bool hasCompletedFollowUp = latestRequiredFollowUp != null &&
                        patientVisits.Any(visit => visit.CreatedDate > latestRequiredFollowUp.CreatedDate);
                    if (latestRequiredFollowUp != null && !hasCompletedFollowUp)
                    {
                        riskScore += 35;
                        reasons.Add("Doctor marked follow-up required with no later visit recorded");
                    }

                    if (riskScore == 0)
                    {
                        continue;
                    }

                    string riskLevel = "Low";
                    if (riskScore >= 60)
                    {
                        riskLevel = "High";
                    }
                    else if (riskScore >= 30)
                    {
                        riskLevel = "Medium";
                    }

                    watchlist.Add(new PatientRiskItem
                    {
                        PatientId = patient.PatientId,
                        PatientName = patient.FullName,
                        Phone = patient.Phone,
                        CalculatedScore = riskScore,
                        RiskLevel = riskLevel,
                        RiskReasons = reasons
                    });
                }

                return watchlist.OrderByDescending(item => item.CalculatedScore).ToList();
            }
        }
    }
}

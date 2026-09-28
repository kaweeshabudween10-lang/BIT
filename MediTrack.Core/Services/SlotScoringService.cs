using System;
using System.Collections.Generic;
using System.Linq;
using MediTrack.Data;

namespace MediTrack.Core
{
    public class CandidateSlot
    {
        public DateTime SlotTime { get; set; }
        public double Score { get; set; }
        public string Recommendation { get; set; } = "Available";
        public string Reason { get; set; } = string.Empty;
    }

    public class SlotScoringService
    {
        /// <summary>
        /// Evaluates candidate time slots for a doctor on a given date and returns ranked slots.
        /// </summary>
        public List<CandidateSlot> GetRankedSlots(int doctorId, int patientId, DateTime date)
        {
            var candidateSlots = new List<CandidateSlot>();

            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                var existingAppointments = db.Appointments
                    .Where(appointment => appointment.DoctorId == doctorId &&
                                          appointment.Status != "Cancelled" &&
                                          appointment.DateTime.Date == date.Date)
                    .ToList();

                int pastNoShows = db.Appointments
                    .Count(appointment => appointment.PatientId == patientId && appointment.Status == "No-show");

                DateTime startTime = date.Date.AddHours(9);
                DateTime endTime = date.Date.AddHours(17);
                DateTime now = DateTime.Now;

                for (DateTime slot = startTime; slot < endTime; slot = slot.AddMinutes(30))
                {
                    if (slot <= now)
                    {
                        continue;
                    }

                    double score = 100.0;
                    var reasons = new List<string>();

                    bool isBooked = existingAppointments.Any(appointment =>
                        Math.Abs((appointment.DateTime - slot).TotalMinutes) < 30);
                    if (isBooked)
                    {
                        continue;
                    }

                    bool hasPriorAppointment = existingAppointments.Any(appointment =>
                        (slot - appointment.DateTime).TotalMinutes == 30);
                    bool hasNextAppointment = existingAppointments.Any(appointment =>
                        (appointment.DateTime - slot).TotalMinutes == 30);

                    if (!hasPriorAppointment && !hasNextAppointment)
                    {
                        score += 20;
                        reasons.Add("Optimal buffer time");
                    }

                    if (pastNoShows >= 2)
                    {
                        if (slot.Hour < 12)
                        {
                            score += 15;
                            reasons.Add("Morning slot preferred for attendance record");
                        }
                        else
                        {
                            score -= 25;
                            reasons.Add("Late afternoon slots discouraged for prior no-shows");
                        }
                    }

                    string recommendation = "Available";
                    if (score >= 115)
                    {
                        recommendation = "Recommended";
                    }
                    else if (score < 85)
                    {
                        recommendation = "Not Recommended";
                    }

                    candidateSlots.Add(new CandidateSlot
                    {
                        SlotTime = slot,
                        Score = score,
                        Recommendation = recommendation,
                        Reason = string.Join("; ", reasons)
                    });
                }
            }

            return candidateSlots.OrderByDescending(slot => slot.Score).ToList();
        }
    }
}
using System;
using System.Linq;
using MediTrack.Data;
using MediTrack.Data.Models;

namespace MediTrack.Core
{
    public class AppointmentService
    {
        /// <summary>
        /// Checks whether a doctor is already booked in the proposed time window.
        /// </summary>
        public bool HasSchedulingConflict(int doctorId, DateTime proposedStartTime, int durationMinutes = 30)
        {
            if (doctorId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(doctorId), "Doctor ID must be greater than zero.");
            }

            if (durationMinutes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationMinutes), "Appointment duration must be greater than zero.");
            }

            DateTime proposedEndTime = proposedStartTime.AddMinutes(durationMinutes);

            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                var existingAppointments = db.Appointments
                    .Where(appointment => appointment.DoctorId == doctorId &&
                                          appointment.Status != "Cancelled" &&
                                          appointment.DateTime.Date == proposedStartTime.Date)
                    .ToList();

                foreach (var appointment in existingAppointments)
                {
                    DateTime existingStart = appointment.DateTime;
                    DateTime existingEnd = existingStart.AddMinutes(30);

                    if (proposedStartTime < existingEnd && proposedEndTime > existingStart)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Saves a new appointment if the doctor has no overlapping appointment.
        /// </summary>
        public (bool Success, string Message) BookAppointment(Appointment newAppointment)
        {
            if (newAppointment is null)
            {
                return (false, "Appointment details are required.");
            }

            if (newAppointment.DoctorId <= 0 || newAppointment.PatientId <= 0)
            {
                return (false, "A valid patient and doctor must be selected.");
            }

            if (newAppointment.DateTime <= DateTime.Now)
            {
                return (false, "Appointments must be scheduled for a future date and time.");
            }

            if (HasSchedulingConflict(newAppointment.DoctorId, newAppointment.DateTime))
            {
                return (false, "This doctor already has an appointment booked at or near this time slot.");
            }

            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                db.Appointments.Add(newAppointment);
                db.SaveChanges();
            }

            return (true, "Appointment booked successfully!");
        }
    }
}
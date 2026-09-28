using Microsoft.EntityFrameworkCore;
using MediTrack.Data.Models;

namespace MediTrack.Data
{
    public static class AppointmentQueries
    {
        public static List<Appointment> GetAppointmentsWithPatients(AppDbContext db)
        {
            return db.Appointments
                .Include(appointment => appointment.Patient)
                .ToList();
        }
    }
}
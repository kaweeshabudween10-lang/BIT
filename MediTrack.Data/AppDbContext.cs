using Microsoft.EntityFrameworkCore;
using MediTrack.Data.Models;

namespace MediTrack.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Visit> Visits => Set<Visit>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<Medicine> Medicines => Set<Medicine>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<PatientRiskScore> PatientRiskScores => Set<PatientRiskScore>();
    }
}
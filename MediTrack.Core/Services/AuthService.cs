using System.Linq;
using Microsoft.EntityFrameworkCore;
using MediTrack.Data;
using MediTrack.Data.Models;

namespace MediTrack.Core
{
    public class AuthService
    {
        // Validates user login credentials
        public User? AuthenticateUser(string username, string password)
        {
            using (var db = CreateDbContext())
            {
                // Find active user matching the username
                var user = db.Users.FirstOrDefault(u => u.Username == username && u.IsActive);

                if (user == null)
                {
                    return null; // User not found or inactive
                }

                // Verify entered password against database hash
                bool isValidPassword = PasswordHasher.VerifyPassword(password, user.PasswordHash);

                return isValidPassword ? user : null;
            }
        }

        // Helper method to seed initial Admin account if database is empty
        public void SeedDefaultAdmin()
        {
            using (var db = CreateDbContext())
            {
                db.Database.Migrate();

                if (!db.Users.Any())
                {
                    var admin = new User
                    {
                        FullName = "System Administrator",
                        Username = "admin",
                        PasswordHash = PasswordHasher.HashPassword("admin123"), // Default password
                        Role = "Admin",
                        IsActive = true
                    };

                    db.Users.Add(admin);
                    db.SaveChanges();
                }
            }
        }

        public static void SeedSampleClinicData()
        {
            using (var db = CreateDbContext())
            {
                var samplePatients = new[]
                {
                    new Patient { FullName = "Kavindu Perera", NIC = "199512345678", DOB = new DateTime(1995, 5, 12), Gender = "Male", Phone = "0771234567", Address = "12 Lake Road, Colombo", BloodGroup = "O+", Allergies = "Penicillin", ChronicConditionFlag = true },
                    new Patient { FullName = "Nipuni Silva", NIC = "199887654321", DOB = new DateTime(1998, 9, 20), Gender = "Female", Phone = "0719876543", Address = "8 Temple Lane, Kandy", BloodGroup = "A+", Allergies = "None", ChronicConditionFlag = false },
                    new Patient { FullName = "Amara Jayasinghe", NIC = "199203140001", DOB = new DateTime(1992, 3, 14), Gender = "Female", Phone = "0701112201", Address = "15 Park Street, Colombo", BloodGroup = "B+", Allergies = "None", ChronicConditionFlag = false },
                    new Patient { FullName = "Ruwan Fernando", NIC = "198811260002", DOB = new DateTime(1988, 11, 26), Gender = "Male", Phone = "0701112202", Address = "23 Hill Road, Gampaha", BloodGroup = "AB+", Allergies = "Dust", ChronicConditionFlag = false },
                    new Patient { FullName = "Dinithi Wickramasinghe", NIC = "200104080003", DOB = new DateTime(2001, 4, 8), Gender = "Female", Phone = "0701112203", Address = "41 Station Road, Matara", BloodGroup = "O-", Allergies = "None", ChronicConditionFlag = false },
                    new Patient { FullName = "Sahan Jayawardena", NIC = "197706190004", DOB = new DateTime(1977, 6, 19), Gender = "Male", Phone = "0701112204", Address = "6 Garden Avenue, Negombo", BloodGroup = "A-", Allergies = "Sulfa drugs", ChronicConditionFlag = true },
                    new Patient { FullName = "Tharushi Bandara", NIC = "199610020005", DOB = new DateTime(1996, 10, 2), Gender = "Female", Phone = "0701112205", Address = "19 Hill Street, Kandy", BloodGroup = "B-", Allergies = "None", ChronicConditionFlag = false },
                    new Patient { FullName = "Nimal Abeysekara", NIC = "198305170006", DOB = new DateTime(1983, 5, 17), Gender = "Male", Phone = "0701112206", Address = "32 Main Road, Kurunegala", BloodGroup = "AB-", Allergies = "Latex", ChronicConditionFlag = false }
                };

                foreach (var patient in samplePatients)
                {
                    if (!db.Patients.Any(existing => existing.NIC == patient.NIC))
                    {
                        db.Patients.Add(patient);
                    }
                }

                db.SaveChanges();

                var sampleDoctors = new[]
                {
                    new { Username = "demo.dr.perera", FullName = "Dr. Anura Perera", Specialization = "Cardiology" },
                    new { Username = "demo.dr.silva", FullName = "Dr. Malini Silva", Specialization = "Pediatrics" },
                    new { Username = "demo.dr.fernando", FullName = "Dr. Nuwan Fernando", Specialization = "General Medicine" }
                };

                foreach (var sampleDoctor in sampleDoctors)
                {
                    var doctorUser = db.Users.FirstOrDefault(user => user.Username == sampleDoctor.Username);
                    if (doctorUser is null)
                    {
                        doctorUser = new User
                        {
                            Username = sampleDoctor.Username,
                            FullName = sampleDoctor.FullName,
                            PasswordHash = PasswordHasher.HashPassword(Guid.NewGuid().ToString("N")),
                            Role = "Doctor",
                            IsActive = true
                        };
                        db.Users.Add(doctorUser);
                        db.SaveChanges();
                    }

                    if (doctorUser.Role == "Doctor" && !db.Doctors.Any(doctor => doctor.UserId == doctorUser.UserId))
                    {
                        db.Doctors.Add(new Doctor
                        {
                            UserId = doctorUser.UserId,
                            Specialization = sampleDoctor.Specialization,
                            AvailableDays = "Mon,Tue,Wed,Thu,Fri",
                            AvgConsultMinutesByType = "30"
                        });
                    }
                }

                db.SaveChanges();

                var sampleMedicines = new[]
                {
                    new Medicine { Name = "Paracetamol 500 mg", Quantity = 250, ReorderLevel = 40, UnitPrice = 5.00m },
                    new Medicine { Name = "Cetirizine 10 mg", Quantity = 90, ReorderLevel = 20, UnitPrice = 12.50m },
                    new Medicine { Name = "Amoxicillin 250 mg", Quantity = 120, ReorderLevel = 25, UnitPrice = 18.00m },
                    new Medicine { Name = "Omeprazole 20 mg", Quantity = 75, ReorderLevel = 20, UnitPrice = 15.00m },
                    new Medicine { Name = "Salbutamol inhaler", Quantity = 18, ReorderLevel = 5, UnitPrice = 450.00m }
                };

                foreach (var medicine in sampleMedicines)
                {
                    if (!db.Medicines.Any(existing => existing.Name == medicine.Name))
                    {
                        db.Medicines.Add(medicine);
                    }
                }

                db.SaveChanges();

                if (!db.Appointments.Any())
                {
                    var patientIds = db.Patients
                        .Where(patient => samplePatients.Select(sample => sample.NIC).Contains(patient.NIC))
                        .OrderBy(patient => patient.PatientId)
                        .Select(patient => patient.PatientId)
                        .ToArray();
                    var doctorIds = db.Doctors
                        .Join(db.Users, doctor => doctor.UserId, user => user.UserId,
                            (doctor, user) => new { user.Username, doctor.DoctorId })
                        .Where(doctor => sampleDoctors.Select(sample => sample.Username).Contains(doctor.Username))
                        .OrderBy(doctor => doctor.DoctorId)
                        .Select(doctor => doctor.DoctorId)
                        .ToArray();

                    if (patientIds.Length >= 3 && doctorIds.Length >= 3)
                    {
                        db.Appointments.AddRange(
                            new Appointment { PatientId = patientIds[0], DoctorId = doctorIds[0], DateTime = DateTime.Today.AddDays(1).AddHours(9), Status = "Booked", SlotScore = 1.0m },
                            new Appointment { PatientId = patientIds[1], DoctorId = doctorIds[1], DateTime = DateTime.Today.AddDays(2).AddHours(10), Status = "Booked", SlotScore = 1.0m },
                            new Appointment { PatientId = patientIds[2], DoctorId = doctorIds[2], DateTime = DateTime.Today.AddDays(3).AddHours(11), Status = "Booked", SlotScore = 1.0m });
                        db.SaveChanges();
                    }
                }
            }
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=meditrack.db")
                .Options;

            return new AppDbContext(options);
        }
    }
}
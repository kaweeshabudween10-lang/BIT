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

        public static void SeedSamplePatients()
        {
            using (var db = CreateDbContext())
            {
                if (!db.Patients.Any())
                {
                    db.Patients.AddRange(
                        new Patient
                        {
                            FullName = "Kavindu Perera",
                            NIC = "199512345678",
                            DOB = new System.DateTime(1995, 5, 12),
                            Gender = "Male",
                            Phone = "0771234567",
                            BloodGroup = "O+",
                            Allergies = "Penicillin",
                            ChronicConditionFlag = true
                        },
                        new Patient
                        {
                            FullName = "Nipuni Silva",
                            NIC = "199887654321",
                            DOB = new System.DateTime(1998, 9, 20),
                            Gender = "Female",
                            Phone = "0719876543",
                            BloodGroup = "A+",
                            Allergies = "None",
                            ChronicConditionFlag = false
                        });

                    db.SaveChanges();
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
using System;
using System.Collections.Generic;
using System.Linq;
using MediTrack.Data;
using MediTrack.Data.Models;

namespace MediTrack.Core
{
    public class UserService
    {
        public List<User> GetAllUsers()
        {
            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                return db.Users.ToList();
            }
        }

        public (bool Success, string Message) RegisterUser(
            string username, string fullName, string rawPassword, string role, string? specialization = null)
        {
            username = username?.Trim() ?? string.Empty;
            fullName = fullName?.Trim() ?? string.Empty;
            role = role?.Trim() ?? string.Empty;
            specialization = specialization?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(rawPassword) ||
                string.IsNullOrWhiteSpace(role))
            {
                return (false, "Username, full name, password, and role are required.");
            }

            role = role.ToLowerInvariant() switch
            {
                "admin" => "Admin",
                "doctor" => "Doctor",
                "receptionist" => "Receptionist",
                _ => string.Empty
            };

            if (role.Length == 0)
            {
                return (false, "Role must be Admin, Doctor, or Receptionist.");
            }

            if (role == "Doctor" && specialization.Length == 0)
            {
                return (false, "A specialization is required for doctor accounts.");
            }

            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                if (db.Users.Any(user => user.Username.ToLower() == username.ToLower()))
                {
                    return (false, "Username already exists. Please choose a different username.");
                }

                var newUser = new User
                {
                    Username = username,
                    FullName = fullName,
                    PasswordHash = PasswordHasher.HashPassword(rawPassword),
                    Role = role,
                    IsActive = true
                };

                using var transaction = db.Database.BeginTransaction();
                db.Users.Add(newUser);
                db.SaveChanges();

                if (role == "Doctor")
                {
                    db.Doctors.Add(new Doctor
                    {
                        UserId = newUser.UserId,
                        Specialization = specialization,
                        AvailableDays = "Mon,Tue,Wed,Thu,Fri,Sat,Sun",
                        AvgConsultMinutesByType = "30"
                    });
                    db.SaveChanges();
                }

                transaction.Commit();
                return (true, "User created successfully!");
            }
        }

        public (bool Success, string Message) ToggleUserStatus(int userId)
        {
            using (var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>()))
            {
                var user = db.Users.Find(userId);
                if (user == null)
                {
                    return (false, "User not found.");
                }

                user.IsActive = !user.IsActive;
                db.SaveChanges();
                return (true, $"User status updated to {(user.IsActive ? "Active" : "Inactive")}.");
            }
        }
    }
}
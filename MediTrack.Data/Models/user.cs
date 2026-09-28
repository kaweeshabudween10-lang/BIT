using System.ComponentModel.DataAnnotations;

namespace MediTrack.Data.Models
{
    public class User
    {
        [Key] // Tells the database this is the Primary Key
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Doctor, or Receptionist
        public bool IsActive { get; set; } = true;
    }
}
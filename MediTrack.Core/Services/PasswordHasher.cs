using BCrypt.Net;

namespace MediTrack.Core
{
    public static class PasswordHasher
    {
        // Hashes a plain-text password before saving to DB
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verifies if an entered password matches the stored hash
        public static bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
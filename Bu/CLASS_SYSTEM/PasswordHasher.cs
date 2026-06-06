using System;

namespace Bu.CLASS_SYSTEM
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword)) 
                return false;

            // Trim database password to remove fixed-length padding (e.g. Oracle CHAR types)
            string trimmedHash = hashedPassword.Trim();
            string trimmedPassword = password.Trim();

            // If it is stored as plain text, match directly
            if (!trimmedHash.StartsWith("$2a$") && !trimmedHash.StartsWith("$2b$") && !trimmedHash.StartsWith("$2y$"))
            {
                return trimmedPassword.Equals(trimmedHash);
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(trimmedPassword, trimmedHash);
            }
            catch (Exception)
            {
                // Fallback in case of legacy format/parsing errors
                return trimmedPassword.Equals(trimmedHash);
            }
        }
    }
}

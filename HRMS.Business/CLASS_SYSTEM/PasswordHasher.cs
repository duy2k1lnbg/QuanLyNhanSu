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

            // Chế độ bảo mật nghiêm ngặt: Chỉ chấp nhận mật khẩu đã hash BCrypt ($2a$, $2b$, $2y$)
            if (!trimmedHash.StartsWith("$2a$") && !trimmedHash.StartsWith("$2b$") && !trimmedHash.StartsWith("$2y$"))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(trimmedPassword, trimmedHash);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

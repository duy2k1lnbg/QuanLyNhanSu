using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HRMS_API.Services
{
    public class JwtUserClaims
    {
        [JsonProperty("nameid")]
        public string UserId { get; set; }

        [JsonProperty("unique_name")]
        public string Username { get; set; }

        [JsonProperty("name")]
        public string FullName { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("rights")]
        public List<string> Rights { get; set; } = new List<string>();

        [JsonProperty("iat")]
        public long IssuedAt { get; set; }

        [JsonProperty("exp")]
        public long ExpiresAt { get; set; }
    }

    public static class JwtService
    {
        private static readonly string DefaultSecret = "HRMS_Secret_Key_Super_Secure_2026_Enterprise_Key_999";

        public static string SecretKey
        {
            get
            {
                var secret = ConfigurationManager.AppSettings["JwtSecret"];
                return !string.IsNullOrWhiteSpace(secret) ? secret : DefaultSecret;
            }
        }

        public static int ExpireHours
        {
            get
            {
                var hoursStr = ConfigurationManager.AppSettings["JwtExpireHours"];
                if (int.TryParse(hoursStr, out int hours) && hours > 0)
                {
                    return hours;
                }
                return 24; // Mặc định 24 giờ
            }
        }

        /// <summary>
        /// Tạo JSON Web Token chuẩn RFC 7519 có chữ ký HMAC-SHA256
        /// </summary>
        public static string GenerateToken(int userId, string username, string fullName, bool isAdmin, List<string> rights)
        {
            var header = new
            {
                alg = "HS256",
                typ = "JWT"
            };

            var now = DateTimeOffset.UtcNow;
            var exp = now.AddHours(ExpireHours);

            var payload = new JwtUserClaims
            {
                UserId = userId.ToString(),
                Username = username,
                FullName = fullName ?? username,
                Role = isAdmin ? "Admin" : "User",
                IsAdmin = isAdmin,
                Rights = rights ?? new List<string>(),
                IssuedAt = now.ToUnixTimeSeconds(),
                ExpiresAt = exp.ToUnixTimeSeconds()
            };

            string headerJson = JsonConvert.SerializeObject(header);
            string payloadJson = JsonConvert.SerializeObject(payload);

            string encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
            string encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

            string dataToSign = $"{encodedHeader}.{encodedPayload}";
            string signature = ComputeHmacSha256Signature(dataToSign, SecretKey);

            return $"{dataToSign}.{signature}";
        }

        /// <summary>
        /// Xác thực tính hợp lệ của Token và giải mã Claims
        /// </summary>
        public static bool ValidateToken(string token, out JwtUserClaims claims, out ClaimsPrincipal principal)
        {
            claims = null;
            principal = null;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            token = token.Trim();
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring(7).Trim();
            }

            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            string encodedHeader = parts[0];
            string encodedPayload = parts[1];
            string signature = parts[2];

            // 1. Kiểm tra chữ ký điện tử HMAC-SHA256
            string dataToSign = $"{encodedHeader}.{encodedPayload}";
            string expectedSignature = ComputeHmacSha256Signature(dataToSign, SecretKey);

            if (!CryptographicEquals(signature, expectedSignature))
            {
                return false;
            }

            // 2. Giải mã Payload
            try
            {
                byte[] payloadBytes = Base64UrlDecode(encodedPayload);
                string payloadJson = Encoding.UTF8.GetString(payloadBytes);
                claims = JsonConvert.DeserializeObject<JwtUserClaims>(payloadJson);

                if (claims == null)
                {
                    return false;
                }

                // 3. Kiểm tra thời hạn hiệu lực (exp)
                var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (claims.ExpiresAt > 0 && claims.ExpiresAt < nowUnix)
                {
                    return false; // Token đã hết hạn
                }

                // 4. Tạo ClaimsPrincipal phục vụ ASP.NET Request Context
                var identityClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, claims.UserId),
                    new Claim(ClaimTypes.Name, claims.Username),
                    new Claim("FullName", claims.FullName ?? ""),
                    new Claim(ClaimTypes.Role, claims.Role ?? "User"),
                    new Claim("IsAdmin", claims.IsAdmin.ToString())
                };

                if (claims.Rights != null)
                {
                    foreach (var right in claims.Rights)
                    {
                        identityClaims.Add(new Claim("Right", right));
                    }
                }

                var identity = new ClaimsIdentity(identityClaims, "Jwt");
                principal = new ClaimsPrincipal(identity);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ComputeHmacSha256Signature(string data, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hash = hmac.ComputeHash(dataBytes);
                return Base64UrlEncode(hash);
            }
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Loại bỏ padding '='
            output = output.Replace('+', '-');
            output = output.Replace('/', '_');
            return output;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+');
            output = output.Replace('_', '/');

            switch (output.Length % 4)
            {
                case 0: break;
                case 2: output += "=="; break;
                case 3: output += "="; break;
                default: throw new FormatException("Invalid Base64Url string");
            }

            return Convert.FromBase64String(output);
        }

        private static bool CryptographicEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}

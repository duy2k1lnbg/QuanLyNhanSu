using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Bu.Services.AI_Services.Security
{
    public class AiHmacProof
    {
        public int ActorUserId { get; set; }
        public string CapabilityOrAction { get; set; }
        public string Audience { get; set; }
        public string Nonce { get; set; }
        public long ExpEpochSeconds { get; set; }
        public string SignatureHex { get; set; }

        public string ToPayloadString()
        {
            return $"{ActorUserId}|{CapabilityOrAction}|{Audience}|{Nonce}|{ExpEpochSeconds}";
        }
    }

    public interface IAiHmacProofService
    {
        AiHmacProof GenerateProof(int actorUserId, string capabilityOrAction, int validSeconds = 30);
        bool VerifyProof(AiHmacProof proof);
    }

    public class AiHmacProofService : IAiHmacProofService
    {
        public const string DefaultAudience = "HRMS_AI_ORACLE";
        private readonly byte[] _secretKeyBytes;

        public AiHmacProofService(string secretKey = null)
        {
            string key = secretKey ?? ConfigurationManager.AppSettings["AiProofSigningKey"];
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ConfigurationErrorsException("Security violation: AiProofSigningKey is missing from configuration.");
            }
            _secretKeyBytes = Encoding.UTF8.GetBytes(key);
        }

        public AiHmacProof GenerateProof(int actorUserId, string capabilityOrAction, int validSeconds = 30)
        {
            if (actorUserId <= 0) throw new ArgumentOutOfRangeException(nameof(actorUserId), "Actor ID must be positive.");
            if (string.IsNullOrWhiteSpace(capabilityOrAction)) throw new ArgumentNullException(nameof(capabilityOrAction));

            var nonce = Guid.NewGuid().ToString("N");
            long exp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + validSeconds;
            var proof = new AiHmacProof
            {
                ActorUserId = actorUserId,
                CapabilityOrAction = capabilityOrAction,
                Audience = DefaultAudience,
                Nonce = nonce,
                ExpEpochSeconds = exp
            };

            string payload = proof.ToPayloadString();
            using (var hmac = new HMACSHA256(_secretKeyBytes))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                proof.SignatureHex = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }

            return proof;
        }

        public bool VerifyProof(AiHmacProof proof)
        {
            if (proof == null || string.IsNullOrWhiteSpace(proof.SignatureHex)) return false;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now > proof.ExpEpochSeconds) return false;

            string payload = proof.ToPayloadString();
            using (var hmac = new HMACSHA256(_secretKeyBytes))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                string expectedSig = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                return string.Equals(expectedSig, proof.SignatureHex, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

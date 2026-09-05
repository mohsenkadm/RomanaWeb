using System.Security.Cryptography;

namespace RomanaWeb.Classes
{
    /// <summary>Central OTP policy: 6-digit codes, expiry, and send/verify rate limits.</summary>
    public static class OtpSettings
    {
        public const int CodeLength = 6;
        public const int ExpiryMinutes = 5;
        public const int MaxSendsPerWindow = 3;
        public const int WindowMinutes = 15;
        public const int MinSecondsBetweenSends = 60;
        public const int MaxVerifyFailures = 5;

        /// <summary>Cryptographically secure 6-digit OTP (100000–999999).</summary>
        public static string GenerateCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        public static bool IsValidCodeFormat(string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != CodeLength)
                return false;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] < '0' || code[i] > '9')
                    return false;
            }
            return true;
        }
    }
}

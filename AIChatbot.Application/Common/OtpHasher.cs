using System.Security.Cryptography;
using System.Text;

namespace AIChatbot.Application.Common
{
    public static class OtpHasher
    {
        public static string Hash(string otp)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(bytes);
        }

        public static bool Verify(string otp, string hash)
        {
            return Hash(otp) == hash;
        }
    }
}
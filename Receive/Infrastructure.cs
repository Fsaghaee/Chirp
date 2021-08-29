using System.Security.Cryptography;
using System.Text;

namespace ChirpConsoleExample
{
    public static class Infrastructure
    {
        private static string Md5Generator(this string token)
        {
            using (var md5 = MD5.Create())
            {
                var tokenBytes = Encoding.ASCII.GetBytes(token);

                var hashBytes = md5.ComputeHash(tokenBytes);

                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static bool IsEqual(string token1, string token2)
        {
            return token1.Md5Generator() == token2.Md5Generator();
        }
    }
}
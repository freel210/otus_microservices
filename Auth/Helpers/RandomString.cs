using System.Security.Cryptography;
using System.Text;

namespace Authentication.Helpers
{
    public static class RandomString
    {
        public static string NewToken(int length)
        {
            return NewPassword(length, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
        }

        public static string NewPassword(int length, string passwordCharset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789*&^%$@!_")
        {
            StringBuilder stringBuilder = new(length);
            byte[] bytes = RandomNumberGenerator.GetBytes(length);
            byte[] array = bytes;
            foreach (byte b in array)
            {
                stringBuilder.Append(passwordCharset[b % passwordCharset.Length]);
            }

            return stringBuilder.ToString();
        }
    }
}

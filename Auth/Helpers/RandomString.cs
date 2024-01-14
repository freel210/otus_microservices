using System.Security.Cryptography;
using System.Text;

namespace Auth.Helpers
{
    public static class RandomString
    {
        public static string NewMark(int length)
        {
            System.Random random = new System.Random();
            StringBuilder stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append("0123456789"[random.Next("0123456789".Length)]);
            }

            return stringBuilder.ToString();
        }

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

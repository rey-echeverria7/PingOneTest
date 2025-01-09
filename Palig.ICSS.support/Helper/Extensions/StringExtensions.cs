using System.Text;


namespace Palig.ICSS.support.Helper.Extensions
{
    public static class StringExtensions
    {
        public static string EncodeBase64(this string value)
        {
            try {
                var valueBytes = Encoding.UTF8.GetBytes(value);
                return Convert.ToBase64String(valueBytes);
            }
            catch {
                return string.Empty;
            }
            
        }

        public static string DecodeBase64(this string value)
        {
            try
            {
                var valueBytes = System.Convert.FromBase64String(value);
                return Encoding.UTF8.GetString(valueBytes);
            }
            catch { return string.Empty; }

        }
    }
}

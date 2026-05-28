using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace QLXeMay.Services
{
    public static class Validator
    {
        public static bool IsNotEmpty(string text)
            => !string.IsNullOrWhiteSpace(text);

        public static bool IsValidPhone(string phone)
            => !string.IsNullOrWhiteSpace(phone) &&
               Regex.IsMatch(phone, @"^(0[3|5|7|8|9])+([0-9]{8})$");

        public static bool IsPositiveNumber(string text, out decimal value)
        {
            bool ok = decimal.TryParse(text, out value);
            return ok && value > 0;
        }

        public static bool IsPositiveInteger(string text, out int value)
        {
            bool ok = int.TryParse(text, out value);
            return ok && value > 0;
        }

        public static bool IsPasswordStrong(string password)
            => !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
    }
}

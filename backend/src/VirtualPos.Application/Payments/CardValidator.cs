using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Application.Payments;

public static class CardValidator
{
    public static bool IsValidLuhn(string digits)
    {
        if (digits.Length < 12) return false;

        int sum = 0;
        bool shouldDouble = false;

        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int digit = digits[i] - '0';
            if (shouldDouble)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
            shouldDouble = !shouldDouble;
        }

        return sum % 10 == 0;
    }

    public static bool IsValidExpiry(string month, string year)
    {
        if (!int.TryParse(month, out int m) || m is < 1 or > 12) return false;
        if (year.Length != 2 || !int.TryParse(year, out int y)) return false;

        int fullYear = 2000 + y;
        var lastDay = new DateTime(fullYear, m, DateTime.DaysInMonth(fullYear, m), 23, 59, 59);
        return lastDay >= DateTime.Now;
    }
}

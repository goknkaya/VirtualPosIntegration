using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Application.Payments;

public static class CardMasker
{
    public static string Mask(string cardNumber)
    {
        if (cardNumber.Length < 10)
            return new string('*', cardNumber.Length);

        return $"{cardNumber[..6]}{new string('*', cardNumber.Length - 10)}{cardNumber[^4..]}";
    }
}

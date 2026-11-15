
using System.Security.Cryptography;
using System.Text;

namespace VerityDQ.Application.Extensions;

public static class UniquenessKeyExtensions
{
    public static string GetSHA256Hash(this string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static string NormalizeString14(this string str)
    {
        string[] charsToRemove =
        [
            "_", "-", "#", "*", ".", "@", "~",
            " ", "\t", "\n", "\r", "\r\n"
        ];

        foreach (string character in charsToRemove)
        {
            str = str.Replace(character, string.Empty);
        }

        str = str.Replace("&", "AND");

        if (str.Length < 14)
        {
            str = str.PadRight(14, '0');
        }

        return str;
    }

    public static string NormalizeString(
        this string str,
        string parms = "")
    {
        string[] charsToRemove =
        [
            "_", "-", "#", "*", ".", "@", "~",
            " ", "\t", "\n", "\r", "\r\n"
        ];

        if (!string.IsNullOrEmpty(parms))
        {
            charsToRemove = parms
                .Select(c => c.ToString())
                .ToArray();
        }

        foreach (string character in charsToRemove)
        {
            str = str.Replace(character, string.Empty);
        }

        return str.Replace("&", "AND").ToUpperInvariant();
    }
}

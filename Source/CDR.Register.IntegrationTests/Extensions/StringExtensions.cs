using System;
using System.Linq;

namespace CDR.Register.IntegrationTests.Extensions
{
    public static class StringExtensions
    {
        public static string GenerateRandomString(this int length)
        {
            const string allowableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(allowableChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetLastFieldFromJsonPath(this string path)
        {
            string[] pathNames = path.Split('.');
            if (pathNames.Length == 1)
            {
                return path;
            }
            else
            {
                return pathNames[pathNames.Length - 1];
            }
        }
    }
}

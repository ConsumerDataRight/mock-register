#nullable enable

using System;

namespace CDR.Register.IntegrationTests
{
    public static class ConnectionStringCheck
    {
        internal const string PRODUCTION_SERVER = "prod.database.windows.net"; // public code so only use substring for matching

        // MJS - Whitelist would be better since if production database ever changes server this blacklist will fail unless someone remembers to update
        private static readonly string[] Blacklist = new string[] { PRODUCTION_SERVER };

        public static string? Check(string? connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                // Reject if blacklisted string found in connectionString
                foreach (string blacklisted in Blacklist)
                {
                    if (connectionString.ToUpper().Trim().Contains(blacklisted.ToUpper().Trim()))
                    {
                        throw new Exception($"{blacklisted} is blacklisted. Cannot connect to this server");  // nb: don't show connectionString since it contains password
                    }
                }
            }

            return connectionString;
        }
    }
}

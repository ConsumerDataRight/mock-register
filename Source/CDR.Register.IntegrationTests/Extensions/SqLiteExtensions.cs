using System;
using Microsoft.Data.Sqlite;

namespace CDR.Register.IntegrationTests.Extensions
{
    static public class SqLiteExtensions
    {
        /// <summary>
        /// Execute scalar command and return result as Int32. Throw error if no results or conversion error
        /// </summary>
        static public Int32 ExecuteScalarInt32(this SqliteCommand command)
        {
            var res = command.ExecuteScalar();

            if (res == DBNull.Value || res == null)
            {
                throw new Exception("Command returns no results");
            }

            return Convert.ToInt32(res);
        }

        /// <summary>
        /// Execute scalar command and return result as string. Throw error if no results or conversion error
        /// </summary>
        static public string ExecuteScalarString(this SqliteCommand command)
        {
            var res = command.ExecuteScalar();

            if (res == DBNull.Value || res == null)
            {
                throw new Exception("Command returns no results");
            }

            return Convert.ToString(res);
        }
    }
}

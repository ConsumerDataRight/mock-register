using System;
using Microsoft.Data.SqlClient;

namespace CDR.Register.IntegrationTests.Extensions
{
    public static class SqlExtensions
    {
        /// <summary>
        /// Execute scalar command and return result as Int32. Throw error if no results or conversion error.
        /// </summary>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static int ExecuteScalarInt32(this SqlCommand command)
        {
            var res = command.ExecuteScalar();

            if (res == DBNull.Value || res == null)
            {
                throw new Exception("Command returns no results");
            }

            return Convert.ToInt32(res);
        }

        /// <summary>
        /// Execute scalar command and return result as string. Throw error if no results or conversion error.
        /// </summary>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static string ExecuteScalarString(this SqlCommand command)
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

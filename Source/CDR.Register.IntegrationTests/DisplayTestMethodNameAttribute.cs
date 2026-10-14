using System;
using System.Reflection;
using Serilog;

namespace CDR.Register.IntegrationTests
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class DisplayTestMethodNameAttribute : Attribute
    {
        private static int _testCount = 0;

        public static void LogTestName(MethodInfo methodUnderTest)
        {
            if (methodUnderTest == null)
            {
                return;
            }

            var className = methodUnderTest.DeclaringType?.Name ?? "Unknown";
            var methodName = methodUnderTest.Name;
            var fullName = $"{className}.{methodName}";

            Log.Information($"********** Test #{++_testCount} - {fullName} **********");
            Console.WriteLine($"Test #{_testCount} - {fullName}");
        }
    }
}

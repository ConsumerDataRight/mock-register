using System;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace CDR.Register.Domain.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Attempts to match the input with the <see cref="DescriptionAttribute"/> on the enum value, if present otherwise the value itself.
        /// </summary>
        /// <remarks>This is case-insensitive.</remarks>
        /// <typeparam name="T">The type of <see cref="Enum"/>.</typeparam>
        /// <param name="input">The input string to match against the enum value name or description.</param>
        /// <param name="value">The value of <typeparamref name="T"/> with the matching name or description.</param>
        /// <returns>A flag indicating whether a matching enum value was found.</returns>
        public static bool TryParseFromDescription<T>(this string input, out T value)
            where T : struct, Enum
        {
            var type = typeof(T);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null &&
                    attribute.Description.Equals(input, StringComparison.OrdinalIgnoreCase))
                {
                    value = (T)field.GetValue(null)!;
                    return true;
                }
            }

            if (Enum.TryParse(input, ignoreCase: true, out value))
            {
                return Enum.IsDefined<T>(value);
            }

            return false;
        }

        /// <summary>
        /// Gets the description for a value of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Enum"/>.</typeparam>
        /// <param name="value">The enum value to fetch a name for.</param>
        /// <returns>The description of the enum value.</returns>
        public static string GetDescription<T>(this T value)
            where T : struct, Enum
        {
            var type = typeof(T);
            var name = value.ToString();

            var field = type.GetField(name);
            if (field == null)
            {
                return null;
            }

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description;
        }

        /// <summary>
        /// Matches the input with the <see cref="DescriptionAttribute"/> on the enum value, if present otherwise the value itself.
        /// </summary>
        /// <remarks>This is case-insensitive.</remarks>
        /// <typeparam name="T">The type of <see cref="Enum"/>.</typeparam>
        /// <param name="input">The input string to match against the enum value name or description.</param>
        /// <returns>A flag indicating whether a matching enum value was found.</returns>
        public static T ParseFromDescription<T>(this string input)
            where T : struct, Enum
        {
            var type = typeof(T);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null &&
                    attribute.Description.Equals(input, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null)!;
                }
            }

            return Enum.Parse<T>(input, true);
        }
    }
}

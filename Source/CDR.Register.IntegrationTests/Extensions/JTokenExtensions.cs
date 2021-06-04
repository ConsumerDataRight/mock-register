using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace CDR.Register.IntegrationTests.Extensions
{
    static public class JTokenExtensions
    {
        /// <summary>
        /// Remove child JToken from JToken
        /// </summary>
        /// <param name="jToken">JToken from which to remove child JToken</param>
        /// <param name="key">Key of child to remove</param>
        static public void Remove(this JToken jToken, string key)
        {
            var t = jToken[key];

            if (t == null)
            {
                throw new Exception($"Key '{key}' not found");
            }

            switch (t.Type)
            {
                case JTokenType.String:
                    t.Parent.Remove();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Should a JToken be removed?
        /// </summary>
        public delegate bool ShouldRemove(JToken jToken);

        /// <summary>
        /// Recursively remove child JTokens from a JToken
        /// </summary>
        /// <param name="rootJToken">Root JToken to start processing from</param>
        /// <param name="shouldRemove">Should a token be removed?</param>
        public static void RemoveJTokens(this JToken rootJToken, ShouldRemove shouldRemove)
        {
            List<JToken> jTokensToRemove = new();

            void DoJToken(JToken jToken)
            {
                // Find nodes that are empty
                foreach (var childJToken in jToken.Children())
                {
                    // Should this token be removed?
                    if (shouldRemove(childJToken))
                    {
                        jTokensToRemove.Add(childJToken);
                    }
                    // Not marked for removal, so process children if applicable
                    else if (childJToken.HasValues)
                    {
                        // Process children
                        DoJToken(childJToken);
                    }
                }
            }

            // Build list of JTokens to remove
            DoJToken(rootJToken);

            // Remove the JTokens marked for removal
            foreach (var jTokenToRemove in jTokensToRemove)
            {
                jTokenToRemove.Parent.Remove();
            }
        }

        /// <summary>
        /// Recursively remove null JTokens from a root JToken
        /// </summary>
        public static void RemoveNulls(this JToken rootJToken)
        {
            RemoveJTokens(rootJToken, (jToken) => jToken.Type == JTokenType.Null);
        }

        /// <summary>
        /// Recursively remove empty array JTokens from a root JToken
        /// </summary>
        public static void RemoveEmptyArrays(this JToken rootJToken)
        {
            RemoveJTokens(rootJToken, (jToken) => jToken.Type == JTokenType.Array && !jToken.HasValues);
        }

        public static void RemovePath(this JToken jToken, string jsonPath)
        {
            var tokens = jToken.SelectTokens(jsonPath).ToList();

            foreach (var token in tokens)
            {
                token.Parent.Remove();
            }
        }

        public delegate JToken Replace(JToken token);
        public static void ReplacePath(this JToken jToken, string jsonPath, Replace replace)
        {
            var tokens = jToken.SelectTokens(jsonPath);

            foreach (var token in tokens)
            {
                token.Replace(replace(token));
            }
        }

        /// <summary>
        /// Sort tokens under jsonPath by key
        /// </summary>
        public static void Sort(this JToken rootToken, string jsonPath, string key)
        {
            var tokens = rootToken.SelectTokens(jsonPath);

            foreach (var token in tokens)
            {
                if (token.Type != JTokenType.Array)
                {
                    throw new Exception($"{token.Path} is not an array");
                }

                var array = token as JArray;

                var sorted = new JArray(array.OrderBy(obj => obj[key]));

                token.Replace(sorted);
            }
        }

        /// <summary>
        /// Sort array by key
        /// </summary>
        public static void SortArray(this JToken token, string key)
        {
            // Check token is array
            if (token.Type != JTokenType.Array)
            {
                throw new Exception($"{token.Path} is not an array");
            }

            // Exit if nothing to srot
            if (!token.HasValues)
            {
                return;
            }

            // Sort the children by key
            var sortedChildTokens = token.Children().OrderBy(childToken => childToken[key]);

            // And replace
            token.Replace(new JArray(sortedChildTokens));
        }

        /// <summary>
        /// Sort path arrays by key
        /// </summary>
        /// <param name="path">For arrays matching this path</param>
        /// <param name="key">Key to sort array by</param>
        public static void SortArray(this JToken token, string path, string key)
        {
            var tokens = token.SelectTokens(path);

            foreach (var _token in tokens)
            {
                _token.SortArray(key);
            }
        }
    }
}
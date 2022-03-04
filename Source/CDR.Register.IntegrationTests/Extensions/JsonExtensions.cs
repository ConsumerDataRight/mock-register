using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace CDR.Register.IntegrationTests.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Strip comments from json string. 
        /// The json will be reserialized so it's formatting may change (ie whitespace/indentation etc)
        /// </summary>
        public static string JsonStripComments(this string json)
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                WriteIndented = true
            };

            var jsonObject = JsonSerializer.Deserialize<object>(json, options);

            return JsonSerializer.Serialize(jsonObject);
        }

        /// <summary>
        /// Compare json. 
        /// Json is converted to JTokens prior to comparision, thus formatting is ignore.
        /// Returns true if json is equivalent, otherwise false.
        /// </summary>
        public static bool JsonCompare(this string json, string jsonToCompare)
        {
            var jsonToken = JToken.Parse(json);
            var jsonToCompareToken = JToken.Parse(jsonToCompare);
            return JToken.DeepEquals(jsonToken, jsonToCompareToken);
        }
    }
}

using System.Text;

namespace CDR.Register.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Key/Value pair builder
    /// </summary>
    public class KeyValuePairBuilder
    {
        private readonly string _delimiter;

        private readonly StringBuilder _sb = new StringBuilder();
        /// <summary>
        /// Get key/value pairs as string
        /// </summary>
        public string Value => _sb.ToString();

        private int _count = 0;
        /// <summary>
        /// Number of key/value pairs
        /// </summary>
        public int Count => _count;

        public KeyValuePairBuilder(string delimiter = "&")
        {
            _delimiter = delimiter;
        }

        /// <summary>
        /// Append key/value pair
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        public void Add(string key, string value)
        {
            if (_sb.Length > 0)
            {
                _sb.Append(_delimiter);
            }

            _sb.Append($"{key}={value}");

            _count++;
        }

        /// <summary>
        /// Add key/value pair
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        public void Add(string key, int value)
        {
            Add(key, value.ToString());
        }

        /// <summary>
        /// Add key/value pair
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        public void Add(string key, long value)
        {
            Add(key, value.ToString());
        }
    }
}

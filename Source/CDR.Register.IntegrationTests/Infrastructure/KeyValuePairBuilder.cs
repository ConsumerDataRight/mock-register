using System.Text;

namespace CDR.Register.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Key/Value pair builder.
    /// </summary>
    public class KeyValuePairBuilder
    {
        private readonly string _delimiter;

        private readonly StringBuilder _sb = new StringBuilder();

        private int _count = 0;

        public KeyValuePairBuilder(string delimiter = "&")
        {
            this._delimiter = delimiter;
        }

        /// <summary>
        /// Gets key/value pairs as string.
        /// </summary>
        public string Value => this._sb.ToString();

        /// <summary>
        /// Gets number of key/value pairs.
        /// </summary>
        public int Count => this._count;

        /// <summary>
        /// Append key/value pair.
        /// </summary>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        public void Add(string key, string value)
        {
            if (this._sb.Length > 0)
            {
                this._sb.Append(this._delimiter);
            }

            this._sb.Append($"{key}={value}");

            this._count++;
        }

        /// <summary>
        /// Add key/value pair.
        /// </summary>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        public void Add(string key, int value)
        {
            this.Add(key, value.ToString());
        }

        /// <summary>
        /// Add key/value pair.
        /// </summary>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        public void Add(string key, long value)
        {
            this.Add(key, value.ToString());
        }
    }
}

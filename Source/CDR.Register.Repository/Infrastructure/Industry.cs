using System.ComponentModel;
using System.Runtime.Serialization;

namespace CDR.Register.Repository.Infrastructure
{
    /// <summary>
    /// The industry.
    /// </summary>
    public enum Industry
    {
        /// <summary>
        /// All industries.
        /// </summary>
        [Description("all")]
        [EnumMember(Value = "all")]
        ALL = 0,

        /// <summary>
        /// Banking.
        /// </summary>
        [Description("banking")]
        [EnumMember(Value = "banking")]
        BANKING = 1,

        /// <summary>
        /// Energy.
        /// </summary>
        [Description("energy")]
        [EnumMember(Value = "energy")]
        ENERGY = 2,

        /// <summary>
        /// Non-Bank Lending.
        /// </summary>
        [Description("non-bank-lending")]
        [EnumMember(Value = "non-bank-lending")]
        NONBANKLENDING = 3,

        /// <summary>
        /// Telecoms Company.
        /// </summary>
        [Description("telco")]
        [EnumMember(Value = "telco")]
        TELCO = 4,
    }
}

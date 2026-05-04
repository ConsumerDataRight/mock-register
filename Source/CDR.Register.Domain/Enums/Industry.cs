using System.ComponentModel;
using System.Runtime.Serialization;

namespace CDR.Register.Domain.Enums
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
        All = 0,

        /// <summary>
        /// Banking.
        /// </summary>
        [Description("banking")]
        [EnumMember(Value = "banking")]
        Banking = 1,

        /// <summary>
        /// Energy.
        /// </summary>
        [Description("energy")]
        [EnumMember(Value = "energy")]
        Energy = 2,

        /// <summary>
        /// Non-Bank Lending.
        /// </summary>
        [Description("non-bank-lending")]
        [EnumMember(Value = "non-bank-lending")]
        NonBankLending = 3,

        /// <summary>
        /// Telecoms Company.
        /// </summary>
        [Description("telco")]
        [EnumMember(Value = "telco")]
        Telco = 4,
    }
}

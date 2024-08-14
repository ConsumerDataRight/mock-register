using Newtonsoft.Json;

namespace CDR.Register.SSA.API.Business
{
    public static class SerializationExtensions
    {
        public static string ToJson(this object value)
        {
            var result = JsonConvert.SerializeObject(value);
            return result;
        }
    }
}

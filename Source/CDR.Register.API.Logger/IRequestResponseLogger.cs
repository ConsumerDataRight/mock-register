using Serilog;

namespace CDR.Register.API.Logger
{
    public interface IRequestResponseLogger
    {
        ILogger Log { get; }
    }
}

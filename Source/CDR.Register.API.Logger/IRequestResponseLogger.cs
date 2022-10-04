namespace CDR.Register.API.Logger
{
    using Serilog;

    public interface IRequestResponseLogger
    {
        ILogger Log { get; }
    }
}
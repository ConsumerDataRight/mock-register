using Microsoft.IdentityModel.Tokens;

namespace CDR.Register.SSA.API.Business
{
    public interface ICertificateService
    {
        string Kid { get; }
        SignatureProvider SignatureProvider { get; }
        Register.API.Infrastructure.Models.JsonWebKeySet JsonWebKeySet { get; }
    }
}

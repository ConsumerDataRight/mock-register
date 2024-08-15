using CDR.Register.Repository.Infrastructure;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public enum RegisterAuthorisationPolicy
    {
        [AuthorisationPolicy("DataHolderBrandsApiMultiIndustry", CdsRegistrationScopes.Read, true, false,false)]
        DataHolderBrandsApiMultiIndustry,
        [AuthorisationPolicy("GetSSAMultiIndustry", CdsRegistrationScopes.Read, true, false, false)]
        GetSSAMultiIndustry
    }
}

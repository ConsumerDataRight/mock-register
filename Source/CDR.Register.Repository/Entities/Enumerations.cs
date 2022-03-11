using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDR.Register.Repository.Entities.erations
{
    public enum Industry
    {
        ALL = 0,
        BANKING,
        ENERGY,
        TELCO
    }

    public enum OrganisationType : int
    {
        Unknown = 0,
        SoleTrader = 1,
        Company = 2,
        Partnership = 3,
        Trust = 4,
        GovernmentEntity = 5,
        Other = 6
    }

    public enum AccreditationLevel : int
    {
        //Sponsored by Default 
        Sponsored = 0,
        Unrestricted = 1
    }

    public enum ParticipationStatus : int
    {
        Unknown = 0,
        Active = 1,
        Removed = 2,
        Suspended = 3,
        Revoked = 4,
        Surrendered = 5,
        Inactive = 6
    }

    public enum LegalEntityStatus : int
    {
        Active = 1,
        Removed = 2
    }

    public enum BrandStatus : int
    {
        Unknown = 0,
        Active = 1,
        Inactive = 2,
        Removed = 3
    }

    public enum ParticipationType : int
    {
        Unknown = 0,
        Dh = 1,
        Dr = 2
    }

    public enum RegisterUType : int
    {
        Unknown = 0,
        SignedJwt = 1
    }

    public enum SoftwareProductStatus : int
    {
        Unknown = 0,
        Active = 1,
        Inactive = 2,
        Removed = 3
    }
}

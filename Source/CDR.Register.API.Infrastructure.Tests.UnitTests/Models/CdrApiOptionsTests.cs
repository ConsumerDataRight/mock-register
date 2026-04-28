using CDR.Register.API.Infrastructure.Models;
using Xunit;

namespace CDR.Register.API.Infrastructure.Tests.UnitTests.Models
{
    [Trait("Category", "UnitTests")]
    public partial class CdrApiOptionsTests
    {
        // DH Brands Endpoint Versions
        public const int DhBrandsVersionMin = 2;
        public const int DhBrandsVersionMax = 3;

        // DH Status Endpoint Versions
        public const int DhStatusVersionMin = 1;
        public const int DhStatusVersionMax = 2;

        // ADR Endpoint Versions
        public const int AdrVersionMin = 3;
        public const int AdrVersionMax = 4;

        // ADR Status Endpoint Versions
        public const int AdrStatusVersionMin = 2;
        public const int AdrStatusVersionMax = 3;

        // ADR Software Product Status Endpoint Versions
        public const int AdrSoftwareProductStatusVersionMin = 2;
        public const int AdrSoftwareProductStatusVersionMax = 3;

        // ADR SSA
        public const int AdrSsaVersionMin = 3;
        public const int AdrSsaVersionMax = 4;

        private readonly CdrApiOptions _underTest = new();

#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
        [Theory]
        // DH Brands
        [InlineData("/cdr-Register/v1/all/data-holders/brands", DhBrandsVersionMin, DhBrandsVersionMax)]
        [InlineData("/cdr-register/v1/Banking/data-holders/brands", DhBrandsVersionMin, DhBrandsVersionMax)]
        [InlineData("/cdr-register/v1/energy/Data-Holders/brands", DhBrandsVersionMin, DhBrandsVersionMax)]
        [InlineData("/cdr-register/v1/non-bank-lending/data-holders/Brands", DhBrandsVersionMin, DhBrandsVersionMax)]
        [InlineData("/cdr-register/v1/telco/data-holders/brands", DhBrandsVersionMin, DhBrandsVersionMax)]
        // DH Status
        [InlineData("/CDR-register/v1/all/data-holders/status", DhStatusVersionMin, DhStatusVersionMax)]
        [InlineData("/cdr-register/v1/banking/Data-Holders/status", DhStatusVersionMin, DhStatusVersionMax)]
        [InlineData("/cdr-register/v1/Energy/data-holders/status", DhStatusVersionMin, DhStatusVersionMax)]
        [InlineData("/cdr-register/v1/non-bank-lending/data-holders/Status", DhStatusVersionMin, DhStatusVersionMax)]
        [InlineData("/cdr-register/V1/telco/data-holders/status", DhStatusVersionMin, DhStatusVersionMax)]
        // Adr
        [InlineData("/CDR-register/v1/all/data-recipients", AdrVersionMin, AdrVersionMax)]
        [InlineData("/cdr-register/v1/banking/Data-Recipients", AdrVersionMin, AdrVersionMax)]
        [InlineData("/cdr-register/v1/Energy/data-recipients", AdrVersionMin, AdrVersionMax)]
        [InlineData("/cdr-register/v1/non-bank-lending/data-recipients", AdrVersionMin, AdrVersionMax)]
        [InlineData("/cdr-register/V1/telco/data-recipients", AdrVersionMin, AdrVersionMax)]
        // ADR Status
        [InlineData("/CDR-register/v1/all/data-recipients/status", AdrStatusVersionMin, AdrStatusVersionMax)]
        [InlineData("/cdr-register/v1/banking/Data-Recipients/status", AdrStatusVersionMin, AdrStatusVersionMax)]
        [InlineData("/cdr-register/v1/Energy/data-recipients/status", AdrStatusVersionMin, AdrStatusVersionMax)]
        [InlineData("/cdr-register/v1/non-bank-lending/data-recipients/Status", AdrStatusVersionMin, AdrStatusVersionMax)]
        [InlineData("/cdr-register/V1/telco/data-recipients/status", AdrStatusVersionMin, AdrStatusVersionMax)]
        // ADR Software Product Status
        [InlineData("/CDR-register/v1/all/data-recipients/brands/software-products/status", AdrSoftwareProductStatusVersionMin, AdrSoftwareProductStatusVersionMax)]
        [InlineData("/cdr-register/v1/banking/Data-Recipients/brands/software-products/status", AdrSoftwareProductStatusVersionMin, AdrSoftwareProductStatusVersionMax)]
        [InlineData("/cdr-register/v1/Energy/data-recipients/brands/software-products/status", AdrSoftwareProductStatusVersionMin, AdrSoftwareProductStatusVersionMax)]
        [InlineData("/cdr-register/v1/non-bank-lending/data-recipients/Brands/software-products/Status", AdrSoftwareProductStatusVersionMin, AdrSoftwareProductStatusVersionMax)]
        [InlineData("/cdr-register/V1/telco/data-recipients/brands/Software-Products/status", AdrSoftwareProductStatusVersionMin, AdrSoftwareProductStatusVersionMax)]
        // ADR SSA
        [InlineData("/CDR-register/v1/all/data-recipients/brands/da7da031-ad91-449e-83e6-d07e7328ef66/software-products/69A28fF3-413F-4255-aF86-3Bd6df7f6Bc3/ssa", AdrSsaVersionMin, AdrSsaVersionMax)]
        [InlineData("/cdr-register/v1/banking/Data-Recipients/brands/da7da031-ad91-449e-83e6-d07e7328ef66/software-products/69A28FF3-413F-4255-AF86-3BD6DF7F6BC3/ssa", AdrSsaVersionMin, AdrSsaVersionMax)]
        [InlineData("/cdr-register/v1/non-bank-lending/data-recipients/Brands/DA7DA031-AD91-449E-83E6-D07E7328EF66/software-products/69a28ff3-413f-4255-af86-3bd6df7f6bc3/ssa", AdrSsaVersionMin, AdrSsaVersionMax)]
        [InlineData("/cdr-register/V1/telco/data-recipients/brands/da7da031-ad91-449e-83e6-d07e7328ef66/Software-Products/69a28ff3-413f-4255-af86-3bd6df7f6bc3/ssa", AdrSsaVersionMin, AdrSsaVersionMax)]
#pragma warning restore SA1515 // Single-line comment should be preceded by blank line
        public void GetApiEndpointVersionOption_ShouldReturnEndpoint(string path, int min, int max)
        {
            var result = this._underTest.GetApiEndpointVersionOption(path);
            Assert.NotNull(result);
            Assert.True(result.IsVersioned);
            Assert.Equal(result.CurrentMinVersion, min);
            Assert.Equal(result.CurrentMaxVersion, max);
        }
    }
}

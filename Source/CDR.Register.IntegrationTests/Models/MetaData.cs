using System;
using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.Models;

public class MetaData
{
    [JsonProperty("legalEntities")]
    public LegalEntity[] LegalEntities { get; set; }
}

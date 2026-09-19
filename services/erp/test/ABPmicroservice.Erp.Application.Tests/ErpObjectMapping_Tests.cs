using AutoMapper;
using Xunit;

namespace ABPmicroservice.Erp;

public class ErpObjectMapping_Tests
{
    // Every DTO member must have a source: a DTO field the entity does not have fails here,
    // not as a silent null in an API response.
    [Fact]
    public void Entity_To_Dto_Mappings_Are_Complete()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<ErpApplicationAutoMapperProfile>());

        configuration.AssertConfigurationIsValid();
    }
}

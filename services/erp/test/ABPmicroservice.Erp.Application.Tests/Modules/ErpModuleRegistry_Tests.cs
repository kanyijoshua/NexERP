using System;
using System.Linq;
using Shouldly;
using Volo.Abp.Application.Services;
using Xunit;

namespace ABPmicroservice.Erp.Modules;

public class ErpModuleRegistry_Tests
{
    /// <summary>
    /// The filter that closes a disabled module's endpoints finds the module from the service's
    /// namespace. An application service in an area no module claims would be left wide open while
    /// its screens were hidden, so this fails the moment one is added.
    /// </summary>
    [Fact]
    public void Every_Application_Service_Belongs_To_A_Module()
    {
        var unclaimed = typeof(ErpApplicationModule)
            .Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IApplicationService).IsAssignableFrom(t))
            .Where(t => ErpModuleRegistry.ForType(t) == null)
            .Select(t => $"{t.Name} ({ErpModuleRegistry.AreaOf(t)})")
            .ToList();

        unclaimed.ShouldBeEmpty(
            $"these services are in no module, so switching a module off would not close them: {string.Join(", ", unclaimed)}"
        );
    }

    /// <summary>Two modules claiming one area would make the lookup depend on declaration order.</summary>
    [Fact]
    public void No_Area_Belongs_To_Two_Modules()
    {
        var duplicated = ErpModuleRegistry
            .All.SelectMany(m => m.Areas)
            .GroupBy(a => a, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        duplicated.ShouldBeEmpty();
    }

    [Fact]
    public void Codes_Are_Unique()
    {
        ErpModuleRegistry.All.Select(m => m.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            .ShouldBe(ErpModuleRegistry.All.Count);
    }

    [Fact]
    public void Every_Dependency_Names_A_Module_That_Exists()
    {
        foreach (var module in ErpModuleRegistry.All)
        {
            foreach (var dependency in module.DependsOn)
            {
                ErpModuleRegistry.Find(dependency).ShouldNotBeNull($"{module.Code} depends on {dependency}");
            }
        }
    }

    /// <summary>
    /// A core module that depended on a switchable one could be turned off through the back door.
    /// </summary>
    [Fact]
    public void A_Core_Module_Never_Depends_On_A_Switchable_One()
    {
        foreach (var core in ErpModuleRegistry.All.Where(m => m.IsCore))
        {
            foreach (var dependency in core.DependsOn)
            {
                ErpModuleRegistry.Get(dependency).IsCore.ShouldBeTrue();
            }
        }
    }

    [Fact]
    public void Reads_The_Area_From_A_Namespace()
    {
        ErpModuleRegistry.AreaOf(typeof(ErpModuleRegistry)).ShouldBe("Modules");
        ErpModuleRegistry.AreaOf(typeof(string)).ShouldBeNull();
        ErpModuleRegistry.ForType(typeof(string)).ShouldBeNull();
    }
}

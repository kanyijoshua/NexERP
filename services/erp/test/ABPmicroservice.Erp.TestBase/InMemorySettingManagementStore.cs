using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace ABPmicroservice.Erp;

/// <summary>ABP's setting store, kept in memory for tests that have no settings table.</summary>
public class InMemorySettingManagementStore : ISettingManagementStore
{
    private readonly ConcurrentDictionary<(string Name, string ProviderName, string ProviderKey), string> _values = new();

    public Task<string> GetOrNullAsync(string name, string providerName, string providerKey)
    {
        return Task.FromResult(_values.GetValueOrDefault((name, providerName, providerKey)));
    }

    public Task<List<SettingValue>> GetListAsync(string providerName, string providerKey)
    {
        return Task.FromResult(
            _values
                .Where(v => v.Key.ProviderName == providerName && v.Key.ProviderKey == providerKey)
                .Select(v => new SettingValue(v.Key.Name, v.Value))
                .ToList()
        );
    }

    public async Task<List<SettingValue>> GetListAsync(string[] names, string providerName, string providerKey)
    {
        var all = await GetListAsync(providerName, providerKey);

        return all.Where(v => names.Contains(v.Name)).ToList();
    }

    public Task SetAsync(string name, string value, string providerName, string providerKey)
    {
        _values[(name, providerName, providerKey)] = value;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string name, string providerName, string providerKey)
    {
        _values.TryRemove((name, providerName, providerKey), out _);
        return Task.CompletedTask;
    }
}

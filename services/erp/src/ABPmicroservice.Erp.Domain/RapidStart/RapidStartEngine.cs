using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// RapidStart Migration & Seeding Engine.
/// Mirrors Business Central RapidStart Services.
/// Imports and validates configuration packages for fast tenant setup.
/// </summary>
public class RapidStartEngine : DomainService
{
    private readonly IRepository<ConfigPackage, Guid> _configPackageRepository;

    public RapidStartEngine(IRepository<ConfigPackage, Guid> configPackageRepository)
    {
        _configPackageRepository = configPackageRepository;
    }

    public async Task<ConfigPackage> ImportPackageJsonAsync(string packageCode, string packageName, string jsonContent)
    {
        var package = new ConfigPackage(GuidGenerator.Create(), packageCode, packageName);
        
        using var doc = JsonDocument.Parse(jsonContent);
        if (doc.RootElement.TryGetProperty("tables", out var tablesElement))
        {
            int tableIdCounter = 1;
            foreach (var tableObj in tablesElement.EnumerateArray())
            {
                string tableName = tableObj.GetProperty("tableName").GetString();
                var packageTable = new ConfigPackageTable(GuidGenerator.Create(), package.Id, tableIdCounter++, tableName);
                
                if (tableObj.TryGetProperty("records", out var recordsElement))
                {
                    packageTable.NoOfRecords = recordsElement.GetArrayLength();
                }

                package.Tables.Add(packageTable);
            }
        }

        await _configPackageRepository.InsertAsync(package);
        return package;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// Who may do what with a table through RapidStart. A package moves data in bulk, so it is held to
/// the same permissions as the screens: staging or exporting a table needs the right to read it,
/// applying to it needs the right to create and change its records. RapidStart's own permissions
/// come on top of these, never instead of them.
/// </summary>
public class RapidStartAccess : ITransientDependency
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ConfigTableRegistry _tables;

    public RapidStartAccess(IAuthorizationService authorizationService, ConfigTableRegistry tables)
    {
        _authorizationService = authorizationService;
        _tables = tables;
    }

    public async Task<bool> CanReadAsync(string entityName)
    {
        var permission = ErpEntityPermissions.Find(entityName);
        return permission != null && await _authorizationService.IsGrantedAsync(permission);
    }

    public async Task<bool> CanWriteAsync(string entityName)
    {
        var permissions = ErpEntityPermissions.FindWrite(entityName);
        if (permissions == null)
        {
            return false;
        }

        foreach (var permission in permissions)
        {
            if (!await _authorizationService.IsGrantedAsync(permission))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<ConfigTableProfile> CheckReadAsync(string entityName)
    {
        var profile = _tables.Get(entityName);
        await _authorizationService.CheckAsync(ErpEntityPermissions.Find(profile.Name) ?? NotExportable(profile));
        return profile;
    }

    public async Task<ConfigTableProfile> CheckWriteAsync(string entityName)
    {
        var profile = _tables.Get(entityName);
        foreach (var permission in ErpEntityPermissions.FindWrite(profile.Name) ?? [NotExportable(profile)])
        {
            await _authorizationService.CheckAsync(permission);
        }

        return profile;
    }

    /// <summary>A table that may be imported but has no permission entry is a build error; ErpEntityPermissions_Tests catches it.</summary>
    private static string NotExportable(ConfigTableProfile profile)
    {
        throw new BusinessException(ErpErrorCodes.RapidStart.TableNotImportable).WithData("entityName", profile.Name);
    }

    public async Task<List<ConfigTableInfoDto>> GetCatalogAsync()
    {
        var result = new List<ConfigTableInfoDto>();

        foreach (var profile in _tables.GetAll())
        {
            if (!await CanReadAsync(profile.Name))
            {
                continue;
            }

            result.Add(
                new ConfigTableInfoDto
                {
                    EntityName = profile.Name,
                    DisplayName = ErpEntityField.Humanize(profile.Name),
                    Area = profile.Area,
                    KeyFields = profile.KeyFields.ToList(),
                    ParentEntity = profile.ParentEntity,
                    RelatedTables = profile
                        .Relations.Select(r => r.TargetEntity)
                        .Where(t => !string.Equals(t, profile.Name, StringComparison.OrdinalIgnoreCase))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    CanWrite = await CanWriteAsync(profile.Name),
                }
            );
        }

        return result
            .OrderBy(t => IndexOfArea(t.Area))
            .ThenBy(t => _tables.SortByDependencies(result.Select(r => r.EntityName)).ToList().IndexOf(t.EntityName))
            .ToList();
    }

    public static List<ConfigFieldInfoDto> DescribeFields(ConfigTableProfile profile)
    {
        return profile
            .ImportableFields.Select(field =>
            {
                var described = ExportFieldMapper.ToDto(field, profile.Definition);
                return new ConfigFieldInfoDto
                {
                    Name = field.Name,
                    DisplayName = field.DisplayName,
                    DataType = described.DataType,
                    EnumValues = described.EnumValues,
                    IsKey = profile.IsKeyField(field.Name),
                    IsRequired = profile.IsRequired(field.Name),
                    MaxLength = field.ClrType == typeof(string) ? profile.MaxLength(field.Name) : null,
                    RelatedTable = profile.FindRelation(field.Name)?.TargetEntity,
                };
            })
            .ToList();
    }

    private static int IndexOfArea(string area)
    {
        var index = ConfigTableRegistry.Areas.ToList().IndexOf(area);
        return index < 0 ? int.MaxValue : index;
    }

    /// <summary>A file sent as base64; anything that does not decode is refused as not a valid file.</summary>
    public static byte[] Decode(UploadFileInput input)
    {
        // Four base64 characters carry three bytes: refuse an oversized file before decoding it.
        if ((input?.ContentBase64?.Length ?? 0) / 4L * 3 > ErpDomainConsts.MaxImportFileBytes)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.FileTooLarge)
                .WithData("maxSize", ErpDomainConsts.MaxImportFileBytes / (1024 * 1024) + " MB");
        }

        byte[] content;
        try
        {
            content = Convert.FromBase64String(input?.ContentBase64 ?? string.Empty);
        }
        catch (FormatException)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.FileNotValid);
        }

        ImportFileReader.EnsureSize(content);
        return content;
    }
}

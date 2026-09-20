using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Integration;

/// <summary>
/// Published Web Service. Mirrors Business Central table 7700 "Web Service".
/// <para>
/// Nothing is reachable through the integration API until it is published here, exactly as in BC,
/// where a page is invisible to OData until someone publishes it. Publishing is per company, so
/// one company can expose its customers without exposing another's.
/// </para>
/// <para>
/// Callers authenticate with the same OAuth tokens as the rest of the API — there is no separate
/// web service access key, which is also the direction Business Central has taken.
/// </para>
/// </summary>
public class PublishedWebService : CompanyAggregateRoot
{
    /// <summary>Name the endpoint is addressed by, e.g. "Customers" in /integration/data/Customers.</summary>
    public string ServiceName { get; private set; }

    /// <summary>Table from the entity registry this service exposes.</summary>
    public string EntityName { get; private set; }

    public WebServiceObjectType ObjectType { get; private set; }

    public bool Published { get; private set; }

    /// <summary>
    /// Fields withheld from callers, separated by commas. A published service is read-only, so
    /// this is the only control needed over what leaves the system.
    /// </summary>
    public string ExcludedFields { get; private set; }

    protected PublishedWebService() { }

    public PublishedWebService(
        Guid id,
        string serviceName,
        string entityName,
        WebServiceObjectType objectType = WebServiceObjectType.Page,
        bool published = false,
        string excludedFields = null
    )
        : base(id)
    {
        ServiceName = NormalizeServiceName(serviceName);
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxEntityNameLength);
        ObjectType = objectType;
        Published = published;
        SetExcludedFields(excludedFields);
    }

    public void Update(string serviceName, WebServiceObjectType objectType, string excludedFields)
    {
        ServiceName = NormalizeServiceName(serviceName);
        ObjectType = objectType;
        SetExcludedFields(excludedFields);
    }

    public void Publish() => Published = true;

    public void Unpublish() => Published = false;

    public bool IsFieldExcluded(string fieldName)
    {
        if (ExcludedFields.IsNullOrWhiteSpace())
        {
            return false;
        }

        foreach (var excluded in ExcludedFields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.Equals(excluded, fieldName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void SetExcludedFields(string excludedFields)
    {
        ExcludedFields = Check.Length(excludedFields, nameof(excludedFields), ErpDomainConsts.MaxTotalingLength);
    }

    /// <summary>
    /// The service name becomes part of a URL, so it is kept to letters, digits and dashes.
    /// </summary>
    private static string NormalizeServiceName(string serviceName)
    {
        var trimmed = Check.NotNullOrWhiteSpace(serviceName, nameof(serviceName), ErpDomainConsts.MaxNameLength).Trim();

        foreach (var character in trimmed)
        {
            if (!char.IsLetterOrDigit(character) && character is not ('-' or '_'))
            {
                throw new BusinessException(ErpErrorCodes.Integration.EndpointNotValid)
                    .WithData("serviceName", serviceName);
            }
        }

        return trimmed;
    }
}

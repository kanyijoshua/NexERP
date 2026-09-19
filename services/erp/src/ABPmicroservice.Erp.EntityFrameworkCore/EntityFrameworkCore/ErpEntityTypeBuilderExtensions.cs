using System.Linq;
using ABPmicroservice.Erp.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp;
using Volo.Abp.MultiTenancy;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

public static class ErpEntityTypeBuilderExtensions
{
    /// <summary>
    /// Unique per tenant and company, as Business Central keys are unique per company.
    /// Soft-deleted rows are excluded so a deleted code can be reused, and NULL tenant
    /// ids (host side) are treated as equal, which PostgreSQL does not do by default.
    /// </summary>
    public static IndexBuilder<TEntity> HasCompanyUniqueIndex<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        params string[] propertyNames
    )
        where TEntity : class, ICompanyScoped
    {
        var columns = new[] { nameof(IMultiTenant.TenantId), nameof(ICompanyScoped.CompanyId) }
            .Concat(propertyNames)
            .ToArray();

        var index = builder.HasIndex(columns).IsUnique().AreNullsDistinct(false);

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            index.HasFilter($"\"{nameof(ISoftDelete.IsDeleted)}\" = false");
        }

        return index;
    }
}

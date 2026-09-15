using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace ABPmicroservice.SaaS.EntityFrameworkCore;

[ConnectionStringName(ABPmicroserviceNames.SaaSDb)]
public interface ISaaSDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}

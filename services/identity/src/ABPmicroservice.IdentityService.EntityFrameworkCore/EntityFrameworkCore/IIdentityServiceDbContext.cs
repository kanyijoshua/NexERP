using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace ABPmicroservice.IdentityService.EntityFrameworkCore;

[ConnectionStringName(ABPmicroserviceNames.IdentityServiceDb)]
public interface IIdentityServiceDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}

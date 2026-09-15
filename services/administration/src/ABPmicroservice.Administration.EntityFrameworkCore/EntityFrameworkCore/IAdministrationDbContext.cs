using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace ABPmicroservice.Administration.EntityFrameworkCore;

[ConnectionStringName(ABPmicroserviceNames.AdministrationDb)]
public interface IAdministrationDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}

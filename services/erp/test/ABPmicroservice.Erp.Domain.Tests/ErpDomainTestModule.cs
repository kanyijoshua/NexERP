using ABPmicroservice.Erp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace ABPmicroservice.Erp;

/* Domain tests run against the EF Core provider (SQLite in-memory). */
[DependsOn(typeof(ErpEntityFrameworkCoreTestModule))]
public class ErpDomainTestModule : AbpModule { }

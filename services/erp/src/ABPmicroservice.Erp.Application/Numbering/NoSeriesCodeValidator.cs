using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Numbering;

/// <summary>Setup pages store series codes; this refuses codes that do not exist (BC: TableRelation).</summary>
public class NoSeriesCodeValidator : ITransientDependency
{
    private readonly IRepository<NoSeries, Guid> _repository;

    public NoSeriesCodeValidator(IRepository<NoSeries, Guid> repository)
    {
        _repository = repository;
    }

    public async Task EnsureExistAsync(params string[] codes)
    {
        var wanted = codes.Where(c => !c.IsNullOrWhiteSpace()).Select(c => c.Trim()).Distinct().ToList();
        if (wanted.Count == 0)
        {
            return;
        }

        var known = (await _repository.GetListAsync(s => wanted.Contains(s.Code))).Select(s => s.Code).ToHashSet();
        var missing = wanted.FirstOrDefault(c => !known.Contains(c));
        if (missing != null)
        {
            throw new BusinessException(ErpErrorCodes.NoSeries.NoSeriesNotFound).WithData("code", missing);
        }
    }
}

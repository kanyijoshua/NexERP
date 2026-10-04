using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp;

/// <summary>A list input that carries the filter pane's conditions (see <c>DynamicFilter</c>).</summary>
public interface IHasDynamicFilter
{
    /// <summary>
    /// JSON: <c>{"logic":"and","conditions":[{"field":"no","operator":"expression","value":"1000..2000"}]}</c>.
    /// Blank means no filter.
    /// </summary>
    string DynamicFilter { get; set; }
}

/// <summary>
/// The paged, sorted list input of every ERP list page. Besides paging and sorting it takes the
/// filter conditions the list's filter pane builds.
/// </summary>
public class ErpPagedListInput : PagedAndSortedResultRequestDto, IHasDynamicFilter
{
    public const int MaxDynamicFilterLength = 8000;

    [StringLength(MaxDynamicFilterLength)]
    public string DynamicFilter { get; set; }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Modules;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Home;

/// <summary>How a cue should read at a glance.</summary>
public enum ActivityCueTone
{
    Neutral = 0,
    Attention = 1,
    Overdue = 2,
}

/// <summary>
/// One figure on the home page, with somewhere to go when it is clicked.
/// Mirrors a cue on a Business Central Role Center.
/// </summary>
public class ActivityCueDto
{
    public string Key { get; set; }

    /// <summary>Already localized.</summary>
    public string DisplayName { get; set; }

    public decimal Value { get; set; }

    /// <summary>True when the value is money rather than a count, so it reads as an amount.</summary>
    public bool IsAmount { get; set; }

    public ActivityCueTone Tone { get; set; }

    public string Icon { get; set; }

    /// <summary>The list this figure was counted from.</summary>
    public string Route { get; set; }
}

/// <summary>Everything the home page draws itself from, in one call.</summary>
public class HomeSummaryDto
{
    public string CompanyName { get; set; }

    public string UserName { get; set; }

    public List<ActivityCueDto> Cues { get; set; } = [];

    /// <summary>The modules that are on, in the order they should be shown.</summary>
    public List<ErpModuleDto> Apps { get; set; } = [];
}

/// <summary>
/// The landing page. Mirrors a Business Central Role Center — activities first, with the apps you
/// can open laid out as tiles the way Odoo's home does it.
/// </summary>
public interface IHomeAppService : IApplicationService
{
    Task<HomeSummaryDto> GetSummaryAsync();
}

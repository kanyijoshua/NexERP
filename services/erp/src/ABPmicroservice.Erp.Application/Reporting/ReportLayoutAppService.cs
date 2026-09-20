using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Report layouts: what a report looks like when it is printed.
/// <para>
/// Mirrors Business Central's Report Layouts page — list the layouts of a report, add your own,
/// and pick which one this company prints through. A layout is held as text, so the client can
/// download one, edit it and upload it again exactly as BC does with Word and Excel layouts.
/// </para>
/// </summary>
[Authorize(ErpPermissions.ReportLayouts.Default)]
public class ReportLayoutAppService : ErpAppService, IReportLayoutAppService
{
    private readonly IRepository<CustomReportLayout, Guid> _layoutRepository;
    private readonly IRepository<ReportLayoutSelection, Guid> _selectionRepository;
    private readonly IRepository<AccountSchedule, Guid> _scheduleRepository;

    public ReportLayoutAppService(
        IRepository<CustomReportLayout, Guid> layoutRepository,
        IRepository<ReportLayoutSelection, Guid> selectionRepository,
        IRepository<AccountSchedule, Guid> scheduleRepository
    )
    {
        _layoutRepository = layoutRepository;
        _selectionRepository = selectionRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<ListResultDto<ReportLayoutDto>> GetListAsync(GetReportLayoutsInput input)
    {
        var layouts = input.ReportName.IsNullOrWhiteSpace()
            ? await _layoutRepository.GetListAsync()
            : await _layoutRepository.GetListAsync(l => l.ReportName == input.ReportName);

        return new ListResultDto<ReportLayoutDto>(
            ObjectMapper.Map<List<CustomReportLayout>, List<ReportLayoutDto>>(
                layouts
                    .OrderBy(l => l.ReportName, StringComparer.Ordinal)
                    .ThenByDescending(l => l.IsDefault)
                    .ThenBy(l => l.LayoutName, StringComparer.Ordinal)
                    .ToList()
            )
        );
    }

    public async Task<ReportLayoutDetailDto> GetAsync(Guid id)
    {
        var layout = await GetLayoutAsync(id);

        return ObjectMapper.Map<CustomReportLayout, ReportLayoutDetailDto>(layout);
    }

    /// <summary>
    /// The fixed reports plus one entry per account schedule, because each schedule prints as a
    /// report of its own and can carry its own layout.
    /// </summary>
    public async Task<ListResultDto<ReportNameDto>> GetReportNamesAsync()
    {
        var names = Enum.GetValues<ReportKind>()
            .Where(kind => kind != ReportKind.AccountSchedule)
            .Select(kind => new ReportNameDto
            {
                Name = ReportLayoutNames.For(kind),
                DisplayName = L[$"Erp::{kind}"],
            })
            .ToList();

        var schedules = await _scheduleRepository.GetListAsync();

        names.AddRange(
            schedules
                .OrderBy(s => s.Name, StringComparer.Ordinal)
                .Select(schedule => new ReportNameDto
                {
                    Name = ReportLayoutNames.For(ReportKind.AccountSchedule, schedule.Name),
                    DisplayName = schedule.Description.IsNullOrWhiteSpace() ? schedule.Name : schedule.Description,
                })
        );

        return new ListResultDto<ReportNameDto>(names);
    }

    public Task<string> GetBuiltInTemplateAsync()
    {
        return Task.FromResult(ReportLayoutTemplates.BuiltIn);
    }

    [Authorize(ErpPermissions.ReportLayouts.Manage)]
    public async Task<ReportLayoutDto> CreateAsync(CreateUpdateReportLayoutDto input)
    {
        await EnsureNameIsFreeAsync(input.ReportName, input.LayoutName);

        var layout = new CustomReportLayout(
            GuidGenerator.Create(),
            input.ReportName,
            input.LayoutName,
            input.LayoutType,
            input.TemplateContent,
            input.Description
        );

        await _layoutRepository.InsertAsync(layout, autoSave: true);

        return ObjectMapper.Map<CustomReportLayout, ReportLayoutDto>(layout);
    }

    [Authorize(ErpPermissions.ReportLayouts.Manage)]
    public async Task<ReportLayoutDto> UpdateAsync(Guid id, CreateUpdateReportLayoutDto input)
    {
        var layout = await GetLayoutAsync(id);

        // The report a layout belongs to is what the selection points at, so it is fixed here;
        // a layout for another report is a new layout.
        if (layout.ReportName != input.ReportName)
        {
            throw new BusinessException(ErpErrorCodes.Reports.LayoutBelongsToAnotherReport)
                .WithData("reportName", layout.ReportName);
        }

        if (layout.LayoutName != input.LayoutName)
        {
            await EnsureNameIsFreeAsync(input.ReportName, input.LayoutName);
        }

        layout.Update(input.TemplateContent, input.Description);
        await _layoutRepository.UpdateAsync(layout, autoSave: true);

        return ObjectMapper.Map<CustomReportLayout, ReportLayoutDto>(layout);
    }

    [Authorize(ErpPermissions.ReportLayouts.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var layout = await GetLayoutAsync(id);

        // A selection pointing at a layout that no longer exists would silently print the
        // built-in one, so the selection goes with it.
        var selection = await _selectionRepository.FirstOrDefaultAsync(s =>
            s.ReportName == layout.ReportName && s.SelectedLayoutId == id
        );

        if (selection != null)
        {
            await _selectionRepository.DeleteAsync(selection);
        }

        await _layoutRepository.DeleteAsync(layout, autoSave: true);
    }

    [Authorize(ErpPermissions.ReportLayouts.Manage)]
    public async Task SetDefaultAsync(SetDefaultReportLayoutInput input)
    {
        var layouts = await _layoutRepository.GetListAsync(l => l.ReportName == input.ReportName);

        var selected = layouts.FirstOrDefault(l => l.Id == input.LayoutId);
        if (selected == null)
        {
            throw new BusinessException(ErpErrorCodes.Reports.LayoutNotFound)
                .WithData("layoutId", input.LayoutId)
                .WithData("reportName", input.ReportName);
        }

        foreach (var layout in layouts.Where(l => l.IsDefault != (l.Id == input.LayoutId)))
        {
            layout.SetDefault(layout.Id == input.LayoutId);
            await _layoutRepository.UpdateAsync(layout);
        }

        var selection = await _selectionRepository.FirstOrDefaultAsync(s => s.ReportName == input.ReportName);
        if (selection == null)
        {
            await _selectionRepository.InsertAsync(
                new ReportLayoutSelection(GuidGenerator.Create(), input.ReportName, selected.Id, selected.LayoutType),
                autoSave: true
            );
        }
        else
        {
            selection.SetSelectedLayout(selected.Id, selected.LayoutType);
            await _selectionRepository.UpdateAsync(selection, autoSave: true);
        }
    }

    /// <summary>
    /// Renders the layout against made-up figures. A layout is checked against something before
    /// it is saved, without needing a company that has any postings yet.
    /// </summary>
    [Authorize(ErpPermissions.ReportLayouts.Manage)]
    public Task<string> RunPreviewAsync(PreviewReportLayoutInput input)
    {
        var template = ReportTemplate.Parse(input.TemplateContent);

        return Task.FromResult(
            template.Render(
                SampleResult(),
                new ReportRenderContext { CompanyName = CurrentCompany.Name, PrintedOn = Clock.Now }
            )
        );
    }

    private static ReportResult SampleResult()
    {
        var result = new ReportResult
        {
            Title = "Trial Balance",
            FromDate = new DateTime(DateTime.Now.Year, 1, 1),
            ToDate = new DateTime(DateTime.Now.Year, 12, 31),
        };

        result.Columns.Add(new ReportColumnDefinition("accountNo", "Account", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("name", "Name", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("debit", "Debit"));
        result.Columns.Add(new ReportColumnDefinition("credit", "Credit"));

        AddSampleRow(result, "1010", "Cash", 12500m, 0m, indentation: 1);
        AddSampleRow(result, "1200", "Accounts Receivable", 4300m, 0m, indentation: 1);
        AddSampleRow(result, "2100", "Accounts Payable", 0m, 6800m, indentation: 1);

        var total = AddSampleRow(result, string.Empty, "Total", 16800m, 6800m);
        total.Bold = true;

        return result;
    }

    private static ReportRow AddSampleRow(
        ReportResult result,
        string accountNo,
        string name,
        decimal debit,
        decimal credit,
        int indentation = 0
    )
    {
        var row = result.AddRow();
        row.Values["accountNo"] = accountNo;
        row.Values["name"] = name;
        row.Values["debit"] = debit;
        row.Values["credit"] = credit;
        row.Indentation = indentation;

        return row;
    }

    private async Task<CustomReportLayout> GetLayoutAsync(Guid id)
    {
        var layout = await _layoutRepository.FindAsync(id);
        if (layout == null)
        {
            throw new BusinessException(ErpErrorCodes.Reports.LayoutNotFound).WithData("layoutId", id);
        }

        return layout;
    }

    private async Task EnsureNameIsFreeAsync(string reportName, string layoutName)
    {
        if (await _layoutRepository.AnyAsync(l => l.ReportName == reportName && l.LayoutName == layoutName))
        {
            throw new BusinessException(ErpErrorCodes.Reports.LayoutNameAlreadyExists)
                .WithData("layoutName", layoutName)
                .WithData("reportName", reportName);
        }
    }
}

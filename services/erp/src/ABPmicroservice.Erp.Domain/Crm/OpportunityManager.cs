using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Crm;

/// <summary>One stage of a pipeline with what sits in it.</summary>
public sealed record PipelineStageSummary(Guid StageId, string Name, int Sequence, bool IsWon, bool IsLost, int Count, decimal ExpectedRevenue, decimal WeightedRevenue);

/// <summary>
/// The life of leads and opportunities: they enter the first stage of their pipeline, move from
/// stage to stage (taking the stage's probability), and close as won or lost. A lead is converted
/// to an opportunity, which ties it to a contact. Every step is logged on the opportunity.
/// </summary>
public class OpportunityManager : DomainService
{
    /// <summary>The pipelines, by the kind of what moves through them.</summary>
    public const string LeadPipeline = "Lead";
    public const string OpportunityPipeline = "Opportunity";

    private readonly IRepository<Opportunity, Guid> _opportunityRepository;
    private readonly IRepository<KanbanStage, Guid> _stageRepository;
    private readonly IRepository<OpportunityEntry, Guid> _entryRepository;
    private readonly IRepository<Contact, Guid> _contactRepository;

    private T Repo<T>() where T : class => LazyServiceProvider.LazyGetRequiredService<T>();

    public OpportunityManager(
        IRepository<Opportunity, Guid> opportunityRepository,
        IRepository<KanbanStage, Guid> stageRepository,
        IRepository<OpportunityEntry, Guid> entryRepository,
        IRepository<Contact, Guid> contactRepository
    )
    {
        _opportunityRepository = opportunityRepository;
        _stageRepository = stageRepository;
        _entryRepository = entryRepository;
        _contactRepository = contactRepository;
    }

    public static string PipelineOf(OpportunityKind kind) => kind == OpportunityKind.Lead ? LeadPipeline : OpportunityPipeline;

    public async Task<List<KanbanStage>> GetStagesAsync(string pipeline) =>
        (await _stageRepository.GetListAsync(s => s.PipelineType == pipeline)).OrderBy(s => s.Sequence).ToList();

    public async Task<Contact> GetContactAsync(string contactNo)
    {
        var no = contactNo?.Trim().ToUpperInvariant();
        return await _contactRepository.FirstOrDefaultAsync(c => c.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Crm.ContactNotFound).WithData("no", no ?? "");
    }

    /// <summary>A new lead or opportunity, in the first stage of its pipeline.</summary>
    public async Task<Opportunity> CreateAsync(string no, OpportunityKind kind, string description, string contactNo)
    {
        var setup = await SetupAsync();
        var resolved = await Repo<NoSeriesManager>().ResolveNoAsync(setup?.OpportunityNos, no, Clock.Now);
        if (await _opportunityRepository.AnyAsync(o => o.No == resolved.ToUpper()))
        {
            throw new BusinessException(ErpErrorCodes.Crm.OpportunityAlreadyExists).WithData("no", resolved);
        }

        var opportunity = new Opportunity(GuidGenerator.Create(), resolved, kind, description);
        if (!contactNo.IsNullOrWhiteSpace())
        {
            opportunity.SetContact(await GetContactAsync(contactNo));
        }

        var first = (await GetStagesAsync(PipelineOf(kind))).FirstOrDefault(s => !s.IsLostStage && !s.IsWonStage);
        if (first != null)
        {
            opportunity.MoveTo(first);
            opportunity.SetProbability(first.DefaultProbability);
        }

        await _opportunityRepository.InsertAsync(opportunity, autoSave: true);
        await LogAsync(opportunity, "Created");
        return opportunity;
    }

    /// <summary>
    /// Moves an open lead or opportunity to a stage of its pipeline. A won stage wins it; a lost
    /// stage loses it, which needs a close code.
    /// </summary>
    public async Task<Opportunity> MoveToStageAsync(Guid id, Guid stageId, string closeCode = null)
    {
        var opportunity = await _opportunityRepository.GetAsync(id);
        opportunity.EnsureOpen();
        var stage = await _stageRepository.GetAsync(stageId);
        if (stage.PipelineType != PipelineOf(opportunity.Kind))
        {
            throw new BusinessException(ErpErrorCodes.Crm.StageNotInPipeline).WithData("stage", stage.Name).WithData("no", opportunity.No);
        }

        if (stage.IsWonStage)
        {
            return await WinAsync(id, closeCode, stage);
        }
        if (stage.IsLostStage)
        {
            return await LoseAsync(id, closeCode, stage);
        }

        opportunity.MoveTo(stage);
        opportunity.SetProbability(stage.DefaultProbability);
        await _opportunityRepository.UpdateAsync(opportunity, autoSave: true);
        await LogAsync(opportunity, $"Moved to {stage.Name}");
        return opportunity;
    }

    /// <summary>Makes a lead an opportunity: it gets a contact (a new one from what the lead says, when none) and enters the opportunity pipeline.</summary>
    public async Task<Opportunity> ConvertLeadAsync(Guid id)
    {
        var opportunity = await _opportunityRepository.GetAsync(id);
        opportunity.EnsureOpen();
        if (opportunity.Kind != OpportunityKind.Lead)
        {
            throw new BusinessException(ErpErrorCodes.Crm.NotALead).WithData("no", opportunity.No);
        }

        if (opportunity.ContactNo == null)
        {
            var contact = await NewContactAsync(
                opportunity.ContactCompanyName.IsNullOrWhiteSpace() ? opportunity.Description : opportunity.ContactCompanyName,
                ContactType.Company,
                opportunity.Email,
                opportunity.PhoneNo,
                opportunity.SalespersonCode
            );
            opportunity.SetContact(contact);
        }

        opportunity.Convert();
        var first = (await GetStagesAsync(OpportunityPipeline)).FirstOrDefault(s => !s.IsLostStage && !s.IsWonStage);
        if (first != null)
        {
            opportunity.MoveTo(first);
            opportunity.SetProbability(Math.Max(opportunity.Probability, first.DefaultProbability));
        }

        await _opportunityRepository.UpdateAsync(opportunity, autoSave: true);
        await LogAsync(opportunity, "Converted to an opportunity");
        return opportunity;
    }

    public async Task<Opportunity> WinAsync(Guid id, string closeCode, KanbanStage wonStage = null)
    {
        var opportunity = await _opportunityRepository.GetAsync(id);
        opportunity.EnsureOpen();
        var code = await CloseCodeAsync(closeCode, CloseOpportunityType.Won, required: false);
        wonStage ??= (await GetStagesAsync(PipelineOf(opportunity.Kind))).FirstOrDefault(s => s.IsWonStage);
        if (wonStage != null)
        {
            opportunity.MoveTo(wonStage);
        }

        opportunity.Close(OpportunityStatus.Won, code, Clock.Now);
        await _opportunityRepository.UpdateAsync(opportunity, autoSave: true);
        await LogAsync(opportunity, "Won");
        return opportunity;
    }

    public async Task<Opportunity> LoseAsync(Guid id, string closeCode, KanbanStage lostStage = null)
    {
        var opportunity = await _opportunityRepository.GetAsync(id);
        opportunity.EnsureOpen();
        var code = await CloseCodeAsync(closeCode, CloseOpportunityType.Lost, required: true);
        lostStage ??= (await GetStagesAsync(PipelineOf(opportunity.Kind))).FirstOrDefault(s => s.IsLostStage);
        if (lostStage != null)
        {
            opportunity.MoveTo(lostStage);
        }

        opportunity.Close(OpportunityStatus.Lost, code, Clock.Now);
        await _opportunityRepository.UpdateAsync(opportunity, autoSave: true);
        await LogAsync(opportunity, $"Lost: {code}");
        return opportunity;
    }

    /// <summary>Opens a won or lost opportunity again, in the first stage of its pipeline.</summary>
    public async Task<Opportunity> ReopenAsync(Guid id)
    {
        var opportunity = await _opportunityRepository.GetAsync(id);
        if (!opportunity.IsClosed)
        {
            return opportunity;
        }

        opportunity.Reopen();
        var first = (await GetStagesAsync(PipelineOf(opportunity.Kind))).FirstOrDefault(s => !s.IsLostStage && !s.IsWonStage);
        if (first != null)
        {
            opportunity.MoveTo(first);
            opportunity.SetProbability(first.DefaultProbability);
        }

        await _opportunityRepository.UpdateAsync(opportunity, autoSave: true);
        await LogAsync(opportunity, "Reopened");
        return opportunity;
    }

    /// <summary>Records the sales quote made for the opportunity.</summary>
    public async Task LinkQuoteAsync(Guid id, string quoteNo)
    {
        var opportunity = await _opportunityRepository.GetAsync(id);
        opportunity.LinkQuote(quoteNo);
        await _opportunityRepository.UpdateAsync(opportunity, autoSave: true);
        await LogAsync(opportunity, $"Sales quote {quoteNo}");
    }

    /// <summary>The customer a contact is; made from the contact (with the setup's defaults) when it is none yet.</summary>
    public async Task<Customer> EnsureCustomerAsync(Contact contact)
    {
        var customers = Repo<IRepository<Customer, Guid>>();
        if (contact.CustomerNo != null)
        {
            var existing = await customers.FirstOrDefaultAsync(c => c.No == contact.CustomerNo);
            if (existing != null)
            {
                return existing;
            }
        }

        var setup = await SetupAsync();
        var salesSetup = await Repo<SalesReceivablesSetupManager>().GetAsync();
        var no = await Repo<NoSeriesManager>().ResolveNoAsync(salesSetup.CustomerNos, salesSetup.CustomerNos.IsNullOrWhiteSpace() ? contact.No : null, Clock.Now);
        var customer = await Repo<CustomerManager>().CreateAsync(no, contact.Name);
        customer.SetAddress(contact.Address, contact.City, null, contact.CountryRegionCode);
        customer.SetContact(contact.PhoneNo, contact.Email);
        customer.SetPostingGroups(setup?.CustomerPostingGroup, setup?.GenBusPostingGroup);
        customer.SetVatBusPostingGroup(setup?.VatBusPostingGroup);
        customer.SetPaymentTerms(setup?.PaymentTermsCode);
        await customers.InsertAsync(customer, autoSave: true);

        contact.LinkCustomer(customer.No);
        await _contactRepository.UpdateAsync(contact, autoSave: true);
        return customer;
    }

    public async Task<Contact> NewContactAsync(string name, ContactType type, string email, string phoneNo, string salespersonCode, string no = null)
    {
        var setup = await SetupAsync();
        var resolved = await Repo<NoSeriesManager>().ResolveNoAsync(setup?.ContactNos, no, Clock.Now);
        if (await _contactRepository.AnyAsync(c => c.No == resolved.ToUpper()))
        {
            throw new BusinessException(ErpErrorCodes.Crm.ContactAlreadyExists).WithData("no", resolved);
        }

        var contact = new Contact(GuidGenerator.Create(), resolved, type, name);
        contact.SetCommunication(null, email, phoneNo, null);
        contact.SetSalesperson(salespersonCode);
        await _contactRepository.InsertAsync(contact, autoSave: true);
        return contact;
    }

    /// <summary>Per stage of a pipeline: how many open cards sit there, their expected and weighted revenue.</summary>
    public async Task<List<PipelineStageSummary>> GetPipelineAsync(string pipeline)
    {
        var stages = await GetStagesAsync(pipeline);
        var kind = pipeline == LeadPipeline ? OpportunityKind.Lead : OpportunityKind.Opportunity;
        var cards = await _opportunityRepository.GetListAsync(o => o.Kind == kind);
        return stages
            .Select(s =>
            {
                var inStage = cards.Where(o => o.StageId == s.Id).ToList();
                return new PipelineStageSummary(s.Id, s.Name, s.Sequence, s.IsWonStage, s.IsLostStage, inStage.Count, inStage.Sum(o => o.ExpectedRevenue), inStage.Sum(o => o.WeightedRevenue));
            })
            .ToList();
    }

    private async Task<string> CloseCodeAsync(string closeCode, CloseOpportunityType type, bool required)
    {
        var code = Companies.CodeTableEntity.NormalizeCode(closeCode);
        if (code == null)
        {
            if (required)
            {
                throw new BusinessException(ErpErrorCodes.Crm.CloseCodeRequired);
            }
            return null;
        }

        var row = await Repo<IRepository<CloseOpportunityCode, Guid>>().FirstOrDefaultAsync(c => c.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.Crm.CloseCodeNotFound).WithData("code", code);
        if (row.Type != type)
        {
            throw new BusinessException(ErpErrorCodes.Crm.CloseCodeWrongType).WithData("code", code);
        }

        return code;
    }

    private async Task<MarketingSetup> SetupAsync() => await Repo<IRepository<MarketingSetup, Guid>>().FirstOrDefaultAsync();

    private Task LogAsync(Opportunity opportunity, string note) =>
        _entryRepository.InsertAsync(new OpportunityEntry(GuidGenerator.Create(), opportunity, Clock.Now, note), autoSave: true);
}

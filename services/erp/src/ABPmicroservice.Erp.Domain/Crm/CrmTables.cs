using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Crm;

/// <summary>Marketing Setup: the number series of contacts, opportunities and campaigns.</summary>
public class MarketingSetup : CompanyEntity
{
    public string ContactNos { get; private set; }
    public string OpportunityNos { get; private set; }
    public string CampaignNos { get; private set; }

    /// <summary>What a customer made from a contact starts with.</summary>
    public string CustomerPostingGroup { get; private set; }
    public string GenBusPostingGroup { get; private set; }
    public string VatBusPostingGroup { get; private set; }
    public string PaymentTermsCode { get; private set; }

    protected MarketingSetup() { }

    public MarketingSetup(Guid id)
        : base(id) { }

    public void SetNumbering(string contactNos, string opportunityNos, string campaignNos)
    {
        ContactNos = Code(contactNos, nameof(contactNos));
        OpportunityNos = Code(opportunityNos, nameof(opportunityNos));
        CampaignNos = Code(campaignNos, nameof(campaignNos));
    }

    public void SetCustomerDefaults(string customerPostingGroup, string genBusPostingGroup, string vatBusPostingGroup, string paymentTermsCode)
    {
        CustomerPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(customerPostingGroup, nameof(customerPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        GenBusPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(genBusPostingGroup, nameof(genBusPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        VatBusPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vatBusPostingGroup, nameof(vatBusPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        PaymentTermsCode = CodeTableEntity.NormalizeCode(Check.Length(paymentTermsCode, nameof(paymentTermsCode), ErpDomainConsts.MaxPaymentTermsCodeLength));
    }

    private static string Code(string value, string name) =>
        value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, ErpDomainConsts.MaxNoSeriesCodeLength);
}

/// <summary>A company or a person the business deals with, before or besides being a customer or vendor.</summary>
public class Contact : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public ContactType Type { get; private set; }
    public string Name { get; private set; }

    /// <summary>The company contact a person works for.</summary>
    public string CompanyNo { get; private set; }
    public string CompanyName { get; private set; }

    public string JobTitle { get; private set; }
    public string Email { get; private set; }
    public string PhoneNo { get; private set; }
    public string MobilePhoneNo { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public string CountryRegionCode { get; private set; }
    public string SalespersonCode { get; private set; }

    /// <summary>The business relations: the customer and the vendor the contact is.</summary>
    public string CustomerNo { get; private set; }
    public string VendorNo { get; private set; }

    protected Contact() { }

    public Contact(Guid id, string no, ContactType type, string name)
        : base(id)
    {
        SetNo(no);
        Type = type;
        SetName(name);
    }

    public void SetNo(string no) => No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();

    public void SetName(string name) => Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength).Trim();

    public void SetType(ContactType type) => Type = type;

    public void SetCompany(string companyNo, string companyName)
    {
        CompanyNo = CodeTableEntity.NormalizeCode(Check.Length(companyNo, nameof(companyNo), ErpDomainConsts.MaxNoLength));
        CompanyName = CompanyNo == null ? null : Check.Length(companyName, nameof(companyName), ErpDomainConsts.MaxNameLength);
    }

    public void SetCommunication(string jobTitle, string email, string phoneNo, string mobilePhoneNo)
    {
        JobTitle = Check.Length(jobTitle, nameof(jobTitle), 50);
        Email = Check.Length(email, nameof(email), 80);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), 30);
        MobilePhoneNo = Check.Length(mobilePhoneNo, nameof(mobilePhoneNo), 30);
    }

    public void SetAddress(string address, string city, string countryRegionCode)
    {
        Address = Check.Length(address, nameof(address), 100);
        City = Check.Length(city, nameof(city), 30);
        CountryRegionCode = CodeTableEntity.NormalizeCode(Check.Length(countryRegionCode, nameof(countryRegionCode), 10));
    }

    public void SetSalesperson(string salespersonCode) =>
        SalespersonCode = CodeTableEntity.NormalizeCode(Check.Length(salespersonCode, nameof(salespersonCode), ErpDomainConsts.MaxCodeLength));

    internal void LinkCustomer(string customerNo) => CustomerNo = customerNo;

    public void SetVendor(string vendorNo) => VendorNo = CodeTableEntity.NormalizeCode(Check.Length(vendorNo, nameof(vendorNo), ErpDomainConsts.MaxNoLength));
}

/// <summary>
/// A lead or an opportunity: a chance to sell, moving through the stages of its pipeline until it
/// is won or lost. Probability and expected revenue give the weighted revenue the pipeline sums.
/// </summary>
public class Opportunity : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public OpportunityKind Kind { get; private set; }
    public string Description { get; private set; }

    public string ContactNo { get; private set; }
    public string ContactName { get; private set; }

    /// <summary>On a lead not yet tied to a contact: who it is, as typed.</summary>
    public string ContactCompanyName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNo { get; private set; }

    public string SalespersonCode { get; private set; }
    public string CampaignNo { get; private set; }
    public OpportunityPriority Priority { get; private set; } = OpportunityPriority.Normal;

    public Guid? StageId { get; private set; }
    public string StageName { get; private set; }

    /// <summary>The chance of winning it, in percent.</summary>
    public decimal Probability { get; private set; }

    public decimal ExpectedRevenue { get; private set; }
    public DateTime? ExpectedCloseDate { get; private set; }

    public OpportunityStatus Status { get; private set; }
    public string CloseOpportunityCode { get; private set; }
    public DateTime? ClosedDate { get; private set; }

    /// <summary>The sales quote made from it.</summary>
    public string SalesQuoteNo { get; private set; }

    protected Opportunity() { }

    public Opportunity(Guid id, string no, OpportunityKind kind, string description)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        Kind = kind;
        SetDescription(description);
    }

    public decimal WeightedRevenue => Math.Round(ExpectedRevenue * Probability / 100m, 2);

    public bool IsClosed => Status != OpportunityStatus.Open;

    public void SetDescription(string description) => Description = Check.NotNullOrWhiteSpace(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);

    public void SetProspect(string contactCompanyName, string email, string phoneNo)
    {
        ContactCompanyName = Check.Length(contactCompanyName, nameof(contactCompanyName), ErpDomainConsts.MaxNameLength);
        Email = Check.Length(email, nameof(email), 80);
        PhoneNo = Check.Length(phoneNo, nameof(phoneNo), 30);
    }

    public void SetContact(Contact contact)
    {
        ContactNo = contact?.No;
        ContactName = contact?.Name;
    }

    public void SetPlanning(string salespersonCode, string campaignNo, OpportunityPriority priority, decimal expectedRevenue, DateTime? expectedCloseDate)
    {
        SalespersonCode = CodeTableEntity.NormalizeCode(Check.Length(salespersonCode, nameof(salespersonCode), ErpDomainConsts.MaxCodeLength));
        CampaignNo = CodeTableEntity.NormalizeCode(Check.Length(campaignNo, nameof(campaignNo), ErpDomainConsts.MaxNoLength));
        Priority = priority;
        ExpectedRevenue = Math.Max(0m, expectedRevenue);
        ExpectedCloseDate = expectedCloseDate?.Date;
    }

    public void SetProbability(decimal probability)
    {
        if (probability < 0m || probability > 100m)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", probability);
        }

        Probability = probability;
    }

    internal void EnsureOpen()
    {
        if (IsClosed)
        {
            throw new BusinessException(ErpErrorCodes.Crm.OpportunityClosed).WithData("no", No);
        }
    }

    internal void MoveTo(Kanban.KanbanStage stage)
    {
        StageId = stage.Id;
        StageName = stage.Name;
    }

    internal void Convert() => Kind = OpportunityKind.Opportunity;

    internal void Close(OpportunityStatus status, string closeCode, DateTime date)
    {
        Status = status;
        CloseOpportunityCode = closeCode;
        ClosedDate = date.Date;
        Probability = status == OpportunityStatus.Won ? 100m : 0m;
    }

    internal void Reopen()
    {
        Status = OpportunityStatus.Open;
        CloseOpportunityCode = null;
        ClosedDate = null;
    }

    internal void LinkQuote(string quoteNo) => SalesQuoteNo = quoteNo;
}

/// <summary>A step in the life of an opportunity: each stage it passed through and how it closed.</summary>
public class OpportunityEntry : CompanyEntity
{
    public Guid OpportunityId { get; private set; }
    public DateTime Date { get; private set; }
    public string StageName { get; private set; }
    public OpportunityStatus Status { get; private set; }
    public decimal Probability { get; private set; }
    public decimal ExpectedRevenue { get; private set; }
    public string Note { get; private set; }

    protected OpportunityEntry() { }

    internal OpportunityEntry(Guid id, Opportunity opportunity, DateTime date, string note)
        : base(id)
    {
        OpportunityId = opportunity.Id;
        Date = date;
        StageName = opportunity.StageName;
        Status = opportunity.Status;
        Probability = opportunity.Probability;
        ExpectedRevenue = opportunity.ExpectedRevenue;
        Note = Check.Length(note, nameof(note), ErpDomainConsts.MaxDescriptionLength);
    }
}

/// <summary>Why an opportunity was won or lost.</summary>
public class CloseOpportunityCode : CodeTableEntity
{
    public CloseOpportunityType Type { get; private set; }

    protected CloseOpportunityCode() { }

    public CloseOpportunityCode(Guid id, string code, string description, CloseOpportunityType type)
        : base(id, code, description)
    {
        Type = type;
    }

    public void SetType(CloseOpportunityType type) => Type = type;
}

/// <summary>A kind of interaction or activity: call, meeting, e-mail, demo.</summary>
public class ActivityType : CodeTableEntity
{
    public string Icon { get; private set; }

    protected ActivityType() { }

    public ActivityType(Guid id, string code, string description, string icon = null)
        : base(id, code, description)
    {
        SetIcon(icon);
    }

    public void SetIcon(string icon) => Icon = Check.Length(icon, nameof(icon), 50);
}

/// <summary>A marketing campaign and the contacts it targets.</summary>
public class Campaign : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }
    public DateTime? StartingDate { get; private set; }
    public DateTime? EndingDate { get; private set; }
    public string SalespersonCode { get; private set; }
    public decimal Budget { get; private set; }

    protected Campaign() { }

    public Campaign(Guid id, string no, string description)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        Set(description, null, null, null, 0m);
    }

    public void Set(string description, DateTime? startingDate, DateTime? endingDate, string salespersonCode, decimal budget)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        if (startingDate.HasValue && endingDate.HasValue && endingDate < startingDate)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidDateRange);
        }

        StartingDate = startingDate?.Date;
        EndingDate = endingDate?.Date;
        SalespersonCode = CodeTableEntity.NormalizeCode(Check.Length(salespersonCode, nameof(salespersonCode), ErpDomainConsts.MaxCodeLength));
        Budget = Math.Max(0m, budget);
    }
}

public class CampaignTarget : CompanyEntity
{
    public string CampaignNo { get; private set; }
    public string ContactNo { get; private set; }
    public string ContactName { get; private set; }

    protected CampaignTarget() { }

    public CampaignTarget(Guid id, string campaignNo, Contact contact)
        : base(id)
    {
        CampaignNo = Check.NotNullOrWhiteSpace(campaignNo, nameof(campaignNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        ContactNo = contact.No;
        ContactName = contact.Name;
    }
}

/// <summary>Interaction Log Entry: something that happened with a contact, such as a call made or a meeting held.</summary>
public class InteractionLogEntry : CompanyEntity
{
    public DateTime Date { get; private set; }
    public string ContactNo { get; private set; }
    public string ContactName { get; private set; }
    public string OpportunityNo { get; private set; }
    public string CampaignNo { get; private set; }
    public string ActivityTypeCode { get; private set; }
    public string Description { get; private set; }
    public int DurationMinutes { get; private set; }
    public decimal Cost { get; private set; }
    public string SalespersonCode { get; private set; }

    protected InteractionLogEntry() { }

    public InteractionLogEntry(Guid id, DateTime date, Contact contact, string activityTypeCode, string description)
        : base(id)
    {
        Date = date;
        ContactNo = contact.No;
        ContactName = contact.Name;
        ActivityTypeCode = CodeTableEntity.NormalizeCode(Check.Length(activityTypeCode, nameof(activityTypeCode), ErpDomainConsts.MaxCodeLength));
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetDetails(string opportunityNo, string campaignNo, int durationMinutes, decimal cost, string salespersonCode)
    {
        OpportunityNo = CodeTableEntity.NormalizeCode(Check.Length(opportunityNo, nameof(opportunityNo), ErpDomainConsts.MaxNoLength));
        CampaignNo = CodeTableEntity.NormalizeCode(Check.Length(campaignNo, nameof(campaignNo), ErpDomainConsts.MaxNoLength));
        DurationMinutes = Math.Max(0, durationMinutes);
        Cost = Math.Max(0m, cost);
        SalespersonCode = CodeTableEntity.NormalizeCode(Check.Length(salespersonCode, nameof(salespersonCode), ErpDomainConsts.MaxCodeLength));
    }
}

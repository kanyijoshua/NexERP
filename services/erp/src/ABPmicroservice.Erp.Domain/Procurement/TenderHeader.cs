using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;

namespace ABPmicroservice.Erp.Procurement;

/// <summary>
/// Tender / Procurement Request: manages the tendering lifecycle including tender creation,
/// committee appointments, mandatory requirements checklist, bidder submissions, scoring, and award.
/// </summary>
public class TenderHeader : CompanyAggregateRoot, IApprovalDocument
{
    public string No { get; private set; }
    public string Title { get; private set; }
    public TenderType TenderType { get; private set; }
    public string RequisitionNo { get; private set; }
    public string ProcurementPlanCode { get; private set; }
    public string ProcurementPlanItemNo { get; private set; }
    public string Category { get; private set; }
    public TenderStatus Status { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public string ClosingTime { get; private set; }
    public int ValidityDays { get; private set; }
    public decimal TenderSecurityAmount { get; private set; }
    public string AwardedVendorNo { get; private set; }
    public string AwardedVendorName { get; private set; }
    public decimal AwardedAmount { get; private set; }
    public DateTime? AwardDate { get; private set; }
    public string PurchaseOrderNo { get; private set; }
    public decimal TotalEstimatedAmount { get; private set; }

    public Collection<TenderLine> Lines { get; private set; }
    public Collection<TenderCommittee> Committees { get; private set; }
    public Collection<TenderMandatoryRequirement> MandatoryRequirements { get; private set; }
    public Collection<TenderBid> Bids { get; private set; }

    // IApprovalDocument implementation
    DocumentStatus IApprovalDocument.Status => Status switch
    {
        TenderStatus.New => DocumentStatus.Open,
        TenderStatus.PendingApproval => DocumentStatus.PendingApproval,
        TenderStatus.Released => DocumentStatus.Released,
        _ => DocumentStatus.Released,
    };
    bool IApprovalDocument.HasLines => Lines.Any();
    decimal IApprovalDocument.ApprovalAmount => TotalEstimatedAmount;
    void IApprovalDocument.SendForApproval() => Status = TenderStatus.PendingApproval;
    void IApprovalDocument.Release() => Release();
    void IApprovalDocument.Reopen() => Status = TenderStatus.New;

    protected TenderHeader()
    {
        Lines = new Collection<TenderLine>();
        Committees = new Collection<TenderCommittee>();
        MandatoryRequirements = new Collection<TenderMandatoryRequirement>();
        Bids = new Collection<TenderBid>();
    }

    public TenderHeader(
        Guid id,
        string no,
        string title,
        TenderType tenderType,
        DateTime issueDate,
        DateTime closingDate,
        string closingTime = "10:00 AM",
        string requisitionNo = null,
        string procurementPlanCode = null,
        string procurementPlanItemNo = null,
        string category = null,
        int validityDays = 90,
        decimal tenderSecurityAmount = 0m
    )
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), 30).Trim().ToUpperInvariant();
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 250).Trim();
        TenderType = tenderType;
        IssueDate = issueDate.Date;
        ClosingDate = closingDate.Date;
        ClosingTime = closingTime ?? "10:00 AM";
        RequisitionNo = requisitionNo?.Trim();
        ProcurementPlanCode = procurementPlanCode?.Trim();
        ProcurementPlanItemNo = procurementPlanItemNo?.Trim();
        Category = category?.Trim();
        ValidityDays = validityDays > 0 ? validityDays : 90;
        TenderSecurityAmount = tenderSecurityAmount;
        Status = TenderStatus.New;
        Lines = new Collection<TenderLine>();
        Committees = new Collection<TenderCommittee>();
        MandatoryRequirements = new Collection<TenderMandatoryRequirement>();
        Bids = new Collection<TenderBid>();
    }

    public void Update(
        string title,
        TenderType tenderType,
        DateTime closingDate,
        string closingTime,
        string category,
        int validityDays,
        decimal tenderSecurityAmount
    )
    {
        EnsureNotAwarded();
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 250).Trim();
        TenderType = tenderType;
        ClosingDate = closingDate.Date;
        ClosingTime = closingTime ?? "10:00 AM";
        Category = category?.Trim();
        ValidityDays = validityDays;
        TenderSecurityAmount = tenderSecurityAmount;
    }

    public void Release()
    {
        if (Status == TenderStatus.Awarded || Status == TenderStatus.Cancelled)
        {
            throw new BusinessException(ErpErrorCodes.Procurement.CannotReleaseTenderInCurrentState).WithData("status", Status);
        }
        Status = TenderStatus.Released;
    }

    public void MoveToOpening()
    {
        Status = TenderStatus.Opening;
    }

    public void MoveToEvaluation()
    {
        Status = TenderStatus.UnderEvaluation;
    }

    public void Award(string vendorNo, string vendorName, decimal amount, DateTime? awardDate, string poNo = null)
    {
        AwardedVendorNo = Check.NotNullOrWhiteSpace(vendorNo, nameof(vendorNo), 30).Trim().ToUpperInvariant();
        AwardedVendorName = Check.NotNullOrWhiteSpace(vendorName, nameof(vendorName), 100).Trim();
        AwardedAmount = amount;
        AwardDate = (awardDate ?? DateTime.Today).Date;
        if (!string.IsNullOrWhiteSpace(poNo))
        {
            PurchaseOrderNo = poNo.Trim().ToUpperInvariant();
        }
        Status = TenderStatus.Awarded;

        // Mark matching bid as awarded
        foreach (var bid in Bids)
        {
            if (bid.BidderNo.Equals(vendorNo, StringComparison.OrdinalIgnoreCase))
            {
                bid.MarkAwarded(true);
            }
            else
            {
                bid.MarkAwarded(false);
            }
        }
    }

    public void Cancel()
    {
        Status = TenderStatus.Cancelled;
    }

    public TenderLine AddLine(
        Guid id,
        int lineNo,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal estimatedUnitCost,
        string specifications = null,
        string unitOfMeasure = null
    )
    {
        EnsureNotAwarded();
        var line = new TenderLine(
            id,
            Id,
            lineNo,
            type,
            no,
            description,
            quantity,
            estimatedUnitCost,
            specifications,
            unitOfMeasure
        );
        Lines.Add(line);
        TotalEstimatedAmount = Lines.Sum(l => l.EstimatedCost);
        return line;
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureNotAwarded();
        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line != null)
        {
            Lines.Remove(line);
            TotalEstimatedAmount = Lines.Sum(l => l.EstimatedCost);
        }
    }

    public TenderCommittee AddCommitteeMember(
        Guid id,
        TenderCommitteeType committeeType,
        string staffNo,
        string staffName,
        TenderCommitteeRole role,
        DateTime? appointmentDate = null
    )
    {
        var committee = new TenderCommittee(id, Id, committeeType, staffNo, staffName, role, appointmentDate ?? DateTime.Today);
        Committees.Add(committee);
        return committee;
    }

    public void RemoveCommitteeMember(Guid memberId)
    {
        var member = Committees.FirstOrDefault(c => c.Id == memberId);
        if (member != null)
        {
            Committees.Remove(member);
        }
    }

    public TenderMandatoryRequirement AddMandatoryRequirement(
        Guid id,
        string requirementDescription,
        bool isMandatory = true
    )
    {
        var req = new TenderMandatoryRequirement(id, Id, requirementDescription, isMandatory);
        MandatoryRequirements.Add(req);
        return req;
    }

    public void RemoveMandatoryRequirement(Guid reqId)
    {
        var req = MandatoryRequirements.FirstOrDefault(m => m.Id == reqId);
        if (req != null)
        {
            MandatoryRequirements.Remove(req);
        }
    }

    public TenderBid AddBid(
        Guid id,
        string bidderNo,
        string bidderName,
        decimal bidAmount,
        string email = null,
        int deliveryPeriodDays = 30,
        DateTime? submissionDate = null,
        bool passedMandatory = true,
        decimal technicalScore = 0m,
        decimal financialScore = 0m,
        string remarks = null
    )
    {
        var bid = new TenderBid(
            id,
            Id,
            bidderNo,
            bidderName,
            bidAmount,
            email,
            deliveryPeriodDays,
            submissionDate ?? DateTime.Today,
            passedMandatory,
            technicalScore,
            financialScore,
            remarks
        );
        Bids.Add(bid);
        return bid;
    }

    public void RemoveBid(Guid bidId)
    {
        EnsureNotAwarded();
        var bid = Bids.FirstOrDefault(b => b.Id == bidId);
        if (bid != null)
        {
            Bids.Remove(bid);
        }
    }

    private void EnsureNotAwarded()
    {
        if (Status == TenderStatus.Awarded || Status == TenderStatus.Cancelled)
        {
            throw new BusinessException(ErpErrorCodes.Procurement.TenderLockedForEditing)
                .WithData("no", No)
                .WithData("status", Status);
        }
    }
}

public class TenderLine : CompanyEntity
{
    public Guid TenderId { get; private set; }
    public int LineNo { get; private set; }
    public DocumentLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public string Specifications { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal EstimatedUnitCost { get; private set; }
    public decimal EstimatedCost { get; private set; }
    public string UnitOfMeasure { get; private set; }

    protected TenderLine() { }

    public TenderLine(
        Guid id,
        Guid tenderId,
        int lineNo,
        DocumentLineType type,
        string no,
        string description,
        decimal quantity,
        decimal estimatedUnitCost,
        string specifications = null,
        string unitOfMeasure = null
    )
        : base(id)
    {
        TenderId = tenderId;
        LineNo = lineNo;
        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), 30).Trim().ToUpperInvariant();
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 200).Trim();
        Quantity = quantity;
        EstimatedUnitCost = estimatedUnitCost;
        EstimatedCost = Math.Round(quantity * estimatedUnitCost, 2, MidpointRounding.AwayFromZero);
        Specifications = specifications?.Trim();
        UnitOfMeasure = unitOfMeasure?.Trim();
    }
}

public class TenderCommittee : CompanyEntity
{
    public Guid TenderId { get; private set; }
    public TenderCommitteeType CommitteeType { get; private set; }
    public string StaffNo { get; private set; }
    public string StaffName { get; private set; }
    public TenderCommitteeRole Role { get; private set; }
    public DateTime AppointmentDate { get; private set; }

    protected TenderCommittee() { }

    public TenderCommittee(
        Guid id,
        Guid tenderId,
        TenderCommitteeType committeeType,
        string staffNo,
        string staffName,
        TenderCommitteeRole role,
        DateTime appointmentDate
    )
        : base(id)
    {
        TenderId = tenderId;
        CommitteeType = committeeType;
        StaffNo = Check.NotNullOrWhiteSpace(staffNo, nameof(staffNo), 30).Trim().ToUpperInvariant();
        StaffName = Check.NotNullOrWhiteSpace(staffName, nameof(staffName), 100).Trim();
        Role = role;
        AppointmentDate = appointmentDate.Date;
    }
}

public class TenderMandatoryRequirement : CompanyEntity
{
    public Guid TenderId { get; private set; }
    public string RequirementDescription { get; private set; }
    public bool IsMandatory { get; private set; }

    protected TenderMandatoryRequirement() { }

    public TenderMandatoryRequirement(
        Guid id,
        Guid tenderId,
        string requirementDescription,
        bool isMandatory = true
    )
        : base(id)
    {
        TenderId = tenderId;
        RequirementDescription = Check.NotNullOrWhiteSpace(requirementDescription, nameof(requirementDescription), 250).Trim();
        IsMandatory = isMandatory;
    }
}

public class TenderBid : CompanyEntity
{
    public Guid TenderId { get; private set; }
    public string BidderNo { get; private set; }
    public string BidderName { get; private set; }
    public string Email { get; private set; }
    public decimal BidAmount { get; private set; }
    public int DeliveryPeriodDays { get; private set; }
    public DateTime SubmissionDate { get; private set; }
    public bool PassedMandatory { get; private set; }
    public decimal TechnicalScore { get; private set; }
    public decimal FinancialScore { get; private set; }
    public decimal TotalScore { get; private set; }
    public int Rank { get; private set; }
    public bool Awarded { get; private set; }
    public string Remarks { get; private set; }

    protected TenderBid() { }

    public TenderBid(
        Guid id,
        Guid tenderId,
        string bidderNo,
        string bidderName,
        decimal bidAmount,
        string email = null,
        int deliveryPeriodDays = 30,
        DateTime? submissionDate = null,
        bool passedMandatory = true,
        decimal technicalScore = 0m,
        decimal financialScore = 0m,
        string remarks = null
    )
        : base(id)
    {
        TenderId = tenderId;
        BidderNo = Check.NotNullOrWhiteSpace(bidderNo, nameof(bidderNo), 30).Trim().ToUpperInvariant();
        BidderName = Check.NotNullOrWhiteSpace(bidderName, nameof(bidderName), 100).Trim();
        BidAmount = bidAmount;
        Email = email?.Trim();
        DeliveryPeriodDays = deliveryPeriodDays;
        SubmissionDate = (submissionDate ?? DateTime.Today).Date;
        PassedMandatory = passedMandatory;
        TechnicalScore = technicalScore;
        FinancialScore = financialScore;
        TotalScore = technicalScore + financialScore;
        Remarks = remarks?.Trim();
        Awarded = false;
    }

    public void UpdateScores(bool passedMandatory, decimal technicalScore, decimal financialScore, int rank, string remarks)
    {
        PassedMandatory = passedMandatory;
        TechnicalScore = technicalScore;
        FinancialScore = financialScore;
        TotalScore = technicalScore + financialScore;
        Rank = rank;
        Remarks = remarks?.Trim();
    }

    public void MarkAwarded(bool awarded)
    {
        Awarded = awarded;
    }
}

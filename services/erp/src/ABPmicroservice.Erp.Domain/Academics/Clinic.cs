using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Inventory;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Academics;

/// <summary>
/// A visit to the infirmary: the complaint, what was found and done, the drugs prescribed, and any
/// sick sheet (days off duty). Completing it issues the drugs from stock and, for a student,
/// bills the visit's charge to the student's account.
/// </summary>
public class ClinicVisit : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public PatientType PatientType { get; private set; }

    /// <summary>The student or employee number; blank for anyone else.</summary>
    public string PatientNo { get; private set; }

    public string PatientName { get; private set; }
    public DateTime VisitDate { get; private set; }
    public TreatmentType TreatmentType { get; private set; }

    public string Complaint { get; private set; }
    public string Diagnosis { get; private set; }
    public string Treatment { get; private set; }

    /// <summary>The clinician (an employee) who saw the patient.</summary>
    public string AttendedBy { get; private set; }

    /// <summary>Where the patient was sent on to; a visit with a referral completes as Referred.</summary>
    public string ReferredTo { get; private set; }

    /// <summary>The sick sheet: the days the patient is excused, and days on light duty after.</summary>
    public DateTime? OffDutyFrom { get; private set; }

    public DateTime? OffDutyTo { get; private set; }
    public int OffDutyDays { get; private set; }
    public int LightDutyDays { get; private set; }
    public string OffDutyComments { get; private set; }

    /// <summary>What a student is billed for the visit; nothing is billed to anyone else.</summary>
    public decimal Charge { get; private set; }

    public ClinicVisitStatus Status { get; private set; }
    public string BillNo { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string CompletedBy { get; private set; }

    protected ClinicVisit() { }

    public ClinicVisit(Guid id, string no, PatientType patientType, string patientNo, string patientName, DateTime visitDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        SetPatient(patientType, patientNo, patientName, visitDate);
    }

    public bool IsOpen => Status == ClinicVisitStatus.Open;

    public void SetPatient(PatientType patientType, string patientNo, string patientName, DateTime visitDate)
    {
        EnsureOpen();
        PatientType = patientType;
        PatientNo = patientNo.IsNullOrWhiteSpace() ? null : Check.Length(patientNo.Trim(), nameof(patientNo), ErpDomainConsts.MaxNoLength);
        PatientName = Check.NotNullOrWhiteSpace(patientName, nameof(patientName), ErpDomainConsts.MaxNameLength * 2).Trim();
        VisitDate = visitDate.Date;
    }

    public void SetClinical(TreatmentType treatmentType, string complaint, string diagnosis, string treatment, string attendedBy, string referredTo, decimal charge)
    {
        EnsureOpen();
        if (charge < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Charge");
        }

        TreatmentType = treatmentType;
        Complaint = Check.Length(complaint, nameof(complaint), ErpDomainConsts.MaxDescriptionLength);
        Diagnosis = Check.Length(diagnosis, nameof(diagnosis), ErpDomainConsts.MaxDescriptionLength);
        Treatment = Check.Length(treatment, nameof(treatment), ErpDomainConsts.MaxDescriptionLength);
        AttendedBy = attendedBy.IsNullOrWhiteSpace() ? null : Check.Length(attendedBy.Trim(), nameof(attendedBy), ErpDomainConsts.MaxNoLength);
        ReferredTo = Check.Length(referredTo, nameof(referredTo), ErpDomainConsts.MaxNameLength);
        Charge = Math.Round(charge, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>The sick sheet; the days off are counted from the first to the last day, both included.</summary>
    public void SetOffDuty(DateTime? offDutyFrom, DateTime? offDutyTo, int lightDutyDays, string offDutyComments)
    {
        EnsureOpen();
        if (offDutyFrom.HasValue != offDutyTo.HasValue || (offDutyFrom.HasValue && offDutyTo.Value.Date < offDutyFrom.Value.Date))
        {
            throw new BusinessException(ErpErrorCodes.Academics.PeriodReversed).WithData("code", No);
        }

        if (lightDutyDays < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Light Duty Days");
        }

        OffDutyFrom = offDutyFrom?.Date;
        OffDutyTo = offDutyTo?.Date;
        OffDutyDays = offDutyFrom.HasValue ? (OffDutyTo.Value - OffDutyFrom.Value).Days + 1 : 0;
        LightDutyDays = lightDutyDays;
        OffDutyComments = Check.Length(offDutyComments, nameof(offDutyComments), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void MarkCompleted(string billNo, DateTime when, string by)
    {
        Status = ReferredTo.IsNullOrWhiteSpace() ? ClinicVisitStatus.Completed : ClinicVisitStatus.Referred;
        BillNo = billNo;
        CompletedDate = when;
        CompletedBy = by;
    }

    public void EnsureOpen()
    {
        if (Status != ClinicVisitStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Academics.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>A drug prescribed on a visit, issued from stock when the visit is completed.</summary>
public class ClinicPrescription : CompanyEntity
{
    public string DocumentNo { get; private set; }
    public int LineNo { get; private set; }

    /// <summary>The stock item dispensed; blank for a drug prescribed but not dispensed here.</summary>
    public string ItemNo { get; private set; }

    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public string Dosage { get; private set; }

    /// <summary>The pharmacy store it is issued from.</summary>
    public string LocationCode { get; private set; }

    public bool Issued { get; internal set; }

    protected ClinicPrescription() { }

    public ClinicPrescription(Guid id, string documentNo, int lineNo)
        : base(id)
    {
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        LineNo = lineNo;
    }

    public void Set(string itemNo, string description, decimal quantity, string dosage, string locationCode)
    {
        if (quantity < 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NegativeAmount).WithData("field", "Quantity");
        }

        ItemNo = itemNo.IsNullOrWhiteSpace() ? null : Check.Length(itemNo.Trim(), nameof(itemNo), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Quantity = quantity;
        Dosage = Check.Length(dosage, nameof(dosage), ErpDomainConsts.MaxDescriptionLength);
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxCodeLength));
    }
}

/// <summary>Completes infirmary visits: issues the drugs prescribed and bills students.</summary>
public class ClinicManager : DomainService
{
    public const string SourceType = "ClinicVisit";

    private readonly IRepository<ClinicVisit, Guid> _visits;
    private readonly IRepository<ClinicPrescription, Guid> _prescriptions;
    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<Student, Guid> _students;
    private readonly ItemJnlPostLine _itemJnlPostLine;
    private readonly StudentBillingEngine _billingEngine;
    private readonly AcademicSetupManager _setupManager;
    private readonly ICurrentUser _currentUser;

    public ClinicManager(
        IRepository<ClinicVisit, Guid> visits,
        IRepository<ClinicPrescription, Guid> prescriptions,
        IRepository<Item, Guid> items,
        IRepository<Student, Guid> students,
        ItemJnlPostLine itemJnlPostLine,
        StudentBillingEngine billingEngine,
        AcademicSetupManager setupManager,
        ICurrentUser currentUser
    )
    {
        _visits = visits;
        _prescriptions = prescriptions;
        _items = items;
        _students = students;
        _itemJnlPostLine = itemJnlPostLine;
        _billingEngine = billingEngine;
        _setupManager = setupManager;
        _currentUser = currentUser;
    }

    public async Task CompleteAsync(ClinicVisit visit)
    {
        visit.EnsureOpen();

        var lines = (await _prescriptions.GetListAsync(p => p.DocumentNo == visit.No && !p.Issued && p.ItemNo != null && p.Quantity > 0))
            .OrderBy(p => p.LineNo)
            .ToList();

        // Checked for every drug before any is issued: a visit is completed whole or not at all.
        var items = new System.Collections.Generic.Dictionary<string, Item>(StringComparer.Ordinal);
        foreach (var group in lines.GroupBy(l => l.ItemNo))
        {
            var item = await _items.FirstOrDefaultAsync(i => i.No == group.Key)
                ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Item").WithData("code", group.Key);

            var wanted = group.Sum(l => l.Quantity);
            if (item.Type == ItemType.Inventory && item.Inventory < wanted)
            {
                throw new BusinessException(ErpErrorCodes.Academics.ItemNotInStock)
                    .WithData("itemNo", item.No)
                    .WithData("quantity", wanted)
                    .WithData("inventory", item.Inventory);
            }

            items[item.No] = item;
        }

        Student student = null;
        if (visit.PatientType == PatientType.Student && visit.Charge > 0m)
        {
            student = await _students.FirstOrDefaultAsync(s => s.No == visit.PatientNo)
                ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", visit.PatientNo ?? string.Empty);
        }

        foreach (var line in lines)
        {
            var item = items[line.ItemNo];
            await _itemJnlPostLine.PostItemEntryAsync(
                item.Id,
                item.No,
                visit.VisitDate,
                ItemLedgerEntryType.NegativeAdjmt,
                visit.No,
                line.Description.IsNullOrWhiteSpace() ? $"Issued to {visit.PatientName}" : line.Description,
                line.Quantity,
                item.UnitCost,
                locationCode: line.LocationCode
            );

            line.Issued = true;
            await _prescriptions.UpdateAsync(line, autoSave: true);
        }

        string billNo = null;
        if (student != null)
        {
            var setup = await _setupManager.GetAsync();
            if (setup.MedicalFeeItemCode.IsNullOrWhiteSpace())
            {
                throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing).WithData("field", "Medical Fee Item Code").WithData("setup", "Academic Setup");
            }

            var description = $"Infirmary visit {visit.No}";
            var bill = await _billingEngine.ChargeAsync(student, visit.VisitDate, null, description, [new StudentCharge(setup.MedicalFeeItemCode, description, visit.Charge)]);
            billNo = bill.No;
        }

        visit.MarkCompleted(billNo, Clock.Now, _currentUser.UserName);
        await _visits.UpdateAsync(visit, autoSave: true);
    }
}

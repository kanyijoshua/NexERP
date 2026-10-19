namespace ABPmicroservice.Erp.Procurement;

public enum ProcurementType
{
    Goods = 0,
    Service = 1,
    Works = 2,
    Consultancy = 3
}

public enum ProcurementMethod
{
    OpenTender = 0,
    RestrictedTender = 1,
    DirectProcurement = 2,
    LowValueProcurement = 3,
    FrameworkAgreement = 4,
    RFQ = 5,
    RFP = 6
}

public enum ProcurementPlanStatus
{
    Open = 0,
    PendingApproval = 1,
    Approved = 2,
    Closed = 3
}

public enum RequisitionType
{
    Purchase = 0,
    Store = 1,
    Capex = 2
}

public enum RequisitionStatus
{
    Open = 0,
    PendingApproval = 1,
    Released = 2,
    Rejected = 3,
    Closed = 4
}

public enum TenderType
{
    Open = 0,
    Restricted = 1,
    Direct = 2,
    LowValue = 3,
    Framework = 4,
    RFQ = 5,
    RFP = 6
}

public enum TenderStatus
{
    New = 0,
    PendingApproval = 1,
    Released = 2,
    Opening = 3,
    UnderEvaluation = 4,
    Awarded = 5,
    Cancelled = 6
}

public enum TenderCommitteeType
{
    Opening = 0,
    Evaluation = 1,
    Negotiation = 2,
    Inspection = 3
}

public enum TenderCommitteeRole
{
    Member = 0,
    Chairperson = 1,
    Secretary = 2
}

public enum PurchaseOrderStage
{
    Draft = 0,
    PendingApproval = 1,
    Released = 2,
    PartiallyReceived = 3,
    Received = 4,
    Invoiced = 5,
    Closed = 6,
    Cancelled = 7
}

namespace ABPmicroservice.Erp.HumanResources
{
    public enum JobVacancyStatus
    {
        Draft = 0,
        Open = 1,
        Closed = 2,
    }

    /// <summary>Where an applicant is in the hiring pipeline; Hired and Rejected are final.</summary>
    public enum ApplicantStage
    {
        New = 0,
        Screening = 1,
        Interview = 2,
        Offer = 3,
        Hired = 4,
        Rejected = 5,
    }

    /// <summary>An appraisal goes from the employee's own review to the manager's, then is completed.</summary>
    public enum AppraisalStatus
    {
        Draft = 0,
        SelfReview = 1,
        ManagerReview = 2,
        Completed = 3,
    }
}

namespace ABPmicroservice.Erp.Subscriptions
{
    public enum SubscriptionStatus
    {
        Draft = 0,
        Active = 1,
        Paused = 2,
        Cancelled = 3,
        Expired = 4,
    }

    /// <summary>What a subscription plan invoices: a G/L account or an item.</summary>
    public enum SubscriptionLineType
    {
        GLAccount = 0,
        Item = 1,
    }
}

namespace ABPmicroservice.Erp.PointOfSale
{
    public enum PosSessionStatus
    {
        Open = 0,
        Closed = 1,
    }

    public enum PosOrderStatus
    {
        Open = 0,
        Paid = 1,
        Cancelled = 2,
    }

    public enum PosPaymentMethod
    {
        Cash = 0,
        Card = 1,
    }
}

namespace ABPmicroservice.Erp.MaintenanceManagement
{
    public enum EquipmentStatus
    {
        Active = 0,
        Scrapped = 1,
    }

    public enum MaintenanceType
    {
        Corrective = 0,
        Preventive = 1,
    }

    public enum MaintenancePriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
    }

    /// <summary>Repaired and Scrapped are final.</summary>
    public enum MaintenanceStage
    {
        New = 0,
        InProgress = 1,
        Repaired = 2,
        Scrapped = 3,
    }
}

namespace ABPmicroservice.Erp.Quality
{
    /// <summary>Where a control point checks goods.</summary>
    public enum QualityOperation
    {
        Receipt = 0,
        Production = 1,
    }

    public enum QualityTestType
    {
        PassFail = 0,
        Measure = 1,
    }

    public enum QualityCheckResult
    {
        Pending = 0,
        Pass = 1,
        Fail = 2,
    }
}

namespace ABPmicroservice.Erp.Fleet
{
    public enum VehicleStatus
    {
        Active = 0,
        InShop = 1,
        Retired = 2,
    }

    public enum VehicleLogType
    {
        Odometer = 0,
        Fuel = 1,
        Service = 2,
    }

    public enum VehicleContractType
    {
        Leasing = 0,
        Insurance = 1,
        Service = 2,
    }

    public enum ContractCostFrequency
    {
        Monthly = 0,
        Yearly = 1,
    }
}

namespace ABPmicroservice.Erp.Crm
{
    public enum MailingStatus
    {
        Sent = 0,
        Failed = 1,
        NoEmail = 2,
    }
}

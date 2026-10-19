using System;
using System.Collections.Generic;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Manufacturing;

/// <summary>Manufacturing Setup: production order numbers and the accounts production posts to.</summary>
public class ManufacturingSetup : CompanyEntity
{
    public string ProductionOrderNos { get; private set; }

    /// <summary>Work in progress: debited with consumption and capacity, credited with output.</summary>
    public string WipAccountNo { get; private set; }

    /// <summary>Credited with the cost of the capacity used.</summary>
    public string CapacityAppliedAccountNo { get; private set; }

    /// <summary>What finishing an order leaves in WIP: the difference between its actual and its output cost.</summary>
    public string VarianceAccountNo { get; private set; }

    protected ManufacturingSetup() { }

    public ManufacturingSetup(Guid id)
        : base(id) { }

    public void Set(string productionOrderNos, string wipAccountNo, string capacityAppliedAccountNo, string varianceAccountNo)
    {
        ProductionOrderNos = productionOrderNos.IsNullOrWhiteSpace() ? null : Check.Length(productionOrderNos.Trim(), nameof(productionOrderNos), ErpDomainConsts.MaxNoSeriesCodeLength);
        WipAccountNo = Account(wipAccountNo, nameof(wipAccountNo));
        CapacityAppliedAccountNo = Account(capacityAppliedAccountNo, nameof(capacityAppliedAccountNo));
        VarianceAccountNo = Account(varianceAccountNo, nameof(varianceAccountNo));
    }

    private static string Account(string no, string name) =>
        no.IsNullOrWhiteSpace() ? null : Check.Length(no.Trim(), name, ErpDomainConsts.MaxNoLength);
}

/// <summary>Work Center: a group of machines or people that does operations, at a cost per hour.</summary>
public class WorkCenter : CodeTableEntity
{
    public decimal UnitCostPerHour { get; private set; }
    public decimal CapacityHoursPerDay { get; private set; } = 8m;

    protected WorkCenter() { }

    public WorkCenter(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(decimal unitCostPerHour, decimal capacityHoursPerDay)
    {
        UnitCostPerHour = Math.Max(0m, unitCostPerHour);
        CapacityHoursPerDay = Math.Max(0m, capacityHoursPerDay);
    }
}

/// <summary>Machine Center: one machine of a work center, with a cost of its own.</summary>
public class MachineCenter : CodeTableEntity
{
    public string WorkCenterCode { get; private set; }
    public decimal UnitCostPerHour { get; private set; }

    protected MachineCenter() { }

    public MachineCenter(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(string workCenterCode, decimal unitCostPerHour)
    {
        WorkCenterCode = NormalizeCode(Check.Length(workCenterCode, nameof(workCenterCode), ErpDomainConsts.MaxCodeLength));
        UnitCostPerHour = Math.Max(0m, unitCostPerHour);
    }
}

/// <summary>Production BOM: what one unit of an item is made of.</summary>
public class ProductionBom : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }
    public ProductionBomStatus Status { get; private set; }

    public List<ProductionBomLine> Lines { get; private set; } = new();

    protected ProductionBom() { }

    public ProductionBom(Guid id, string no, string description)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        SetDescription(description);
    }

    public void SetDescription(string description) => Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);

    public void SetStatus(ProductionBomStatus status)
    {
        if (status == ProductionBomStatus.Certified && Lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.BomEmpty).WithData("no", No);
        }

        Status = status;
    }

    public void EnsureEditable()
    {
        if (Status == ProductionBomStatus.Certified)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.BomCertified).WithData("no", No);
        }
    }
}

public class ProductionBomLine : Entity<Guid>
{
    public Guid ProductionBomId { get; private set; }
    public int LineNo { get; private set; }
    public ProductionBomLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal QuantityPer { get; private set; }
    public decimal ScrapPct { get; private set; }

    protected ProductionBomLine() { }

    public ProductionBomLine(Guid id, Guid bomId, int lineNo)
        : base(id)
    {
        ProductionBomId = bomId;
        LineNo = lineNo;
    }

    public void Set(ProductionBomLineType type, string no, string description, decimal quantityPer, decimal scrapPct)
    {
        if (quantityPer <= 0m || scrapPct < 0m || scrapPct >= 100m)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.InvalidQuantityPer).WithData("no", no ?? "");
        }

        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        QuantityPer = quantityPer;
        ScrapPct = scrapPct;
    }
}

/// <summary>Routing: the operations an item goes through, with their setup and run times.</summary>
public class Routing : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }
    public ProductionBomStatus Status { get; private set; }

    public List<RoutingLine> Lines { get; private set; } = new();

    protected Routing() { }

    public Routing(Guid id, string no, string description)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        SetDescription(description);
    }

    public void SetDescription(string description) => Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);

    public void SetStatus(ProductionBomStatus status) => Status = status;
}

public class RoutingLine : Entity<Guid>
{
    public Guid RoutingId { get; private set; }
    public string OperationNo { get; private set; }
    public RoutingLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }

    /// <summary>Minutes to set the operation up, once per order.</summary>
    public decimal SetupTime { get; private set; }

    /// <summary>Minutes per unit.</summary>
    public decimal RunTime { get; private set; }

    protected RoutingLine() { }

    public RoutingLine(Guid id, Guid routingId)
        : base(id)
    {
        RoutingId = routingId;
    }

    public void Set(string operationNo, RoutingLineType type, string no, string description, decimal setupTime, decimal runTime)
    {
        OperationNo = Check.NotNullOrWhiteSpace(operationNo, nameof(operationNo), 10).Trim();
        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        SetupTime = Math.Max(0m, setupTime);
        RunTime = Math.Max(0m, runTime);
    }
}

/// <summary>
/// Production Order: making a quantity of an item. Refreshing explodes its BOM into components and
/// its routing into operations; once released, components are consumed, capacity is used and output
/// is posted; finishing it closes the WIP, leaving any difference as variance.
/// </summary>
public class ProductionOrder : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public ProductionOrderStatus Status { get; private set; }
    public string SourceItemNo { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTime DueDate { get; private set; }
    public string LocationCode { get; private set; }
    public decimal FinishedQuantity { get; private set; }
    public DateTime? FinishedDate { get; private set; }

    /// <summary>What has gone into the order and come out of it, in LCY: its WIP is the difference.</summary>
    public decimal MaterialCost { get; private set; }
    public decimal CapacityCost { get; private set; }
    public decimal OutputCost { get; private set; }
    public decimal VarianceCost { get; private set; }

    public List<ProdOrderComponent> Components { get; private set; } = new();
    public List<ProdOrderRoutingLine> RoutingLines { get; private set; } = new();

    protected ProductionOrder() { }

    public ProductionOrder(Guid id, string no, ProductionOrderStatus status, string sourceItemNo, string description, decimal quantity, DateTime dueDate, string locationCode)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        Status = status;
        SourceItemNo = Check.NotNullOrWhiteSpace(sourceItemNo, nameof(sourceItemNo), ErpDomainConsts.MaxNoLength).Trim();
        Set(description, quantity, dueDate, locationCode);
    }

    public decimal Wip => MaterialCost + CapacityCost - OutputCost - VarianceCost;

    public void Set(string description, decimal quantity, DateTime dueDate, string locationCode)
    {
        if (quantity <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.InvalidQuantityPer).WithData("no", No);
        }

        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Quantity = quantity;
        DueDate = dueDate.Date;
        LocationCode = CodeTableEntity.NormalizeCode(Check.Length(locationCode, nameof(locationCode), ErpDomainConsts.MaxLocationCodeLength));
    }

    public void EnsureStatus(ProductionOrderStatus status)
    {
        if (Status != status)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.WrongOrderStatus).WithData("no", No).WithData("status", Status.ToString()).WithData("expected", status.ToString());
        }
    }

    internal void ChangeStatus(ProductionOrderStatus status)
    {
        if (status < Status || status == ProductionOrderStatus.Finished)
        {
            throw new BusinessException(ErpErrorCodes.Manufacturing.WrongOrderStatus).WithData("no", No).WithData("status", Status.ToString()).WithData("expected", status.ToString());
        }

        Status = status;
    }

    internal void AddMaterial(decimal cost) => MaterialCost += cost;
    internal void AddCapacity(decimal cost) => CapacityCost += cost;

    internal void AddOutput(decimal quantity, decimal cost)
    {
        FinishedQuantity += quantity;
        OutputCost += cost;
    }

    internal void Finish(DateTime date, decimal variance)
    {
        Status = ProductionOrderStatus.Finished;
        FinishedDate = date.Date;
        VarianceCost += variance;
    }
}

public class ProdOrderComponent : Entity<Guid>
{
    public Guid ProductionOrderId { get; private set; }
    public int LineNo { get; private set; }
    public string ItemNo { get; private set; }
    public string Description { get; private set; }
    public decimal QuantityPer { get; private set; }
    public decimal ScrapPct { get; private set; }
    public decimal ExpectedQuantity { get; private set; }
    public decimal ActConsumption { get; private set; }
    public decimal CostAmount { get; private set; }

    protected ProdOrderComponent() { }

    public ProdOrderComponent(Guid id, Guid orderId, int lineNo, string itemNo, string description, decimal quantityPer, decimal scrapPct, decimal orderQuantity)
        : base(id)
    {
        ProductionOrderId = orderId;
        LineNo = lineNo;
        ItemNo = itemNo;
        Description = description;
        QuantityPer = quantityPer;
        ScrapPct = scrapPct;
        ExpectedQuantity = Math.Round(quantityPer * (1m + scrapPct / 100m) * orderQuantity, 5);
    }

    public decimal RemainingQuantity => Math.Max(0m, ExpectedQuantity - ActConsumption);

    internal void Consume(decimal quantity, decimal cost)
    {
        ActConsumption += quantity;
        CostAmount += cost;
    }
}

public class ProdOrderRoutingLine : Entity<Guid>
{
    public Guid ProductionOrderId { get; private set; }
    public string OperationNo { get; private set; }
    public RoutingLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal SetupTime { get; private set; }
    public decimal RunTime { get; private set; }
    public decimal UnitCostPerHour { get; private set; }
    public decimal ExpectedMinutes { get; private set; }
    public decimal ActualMinutes { get; private set; }
    public decimal CostAmount { get; private set; }

    protected ProdOrderRoutingLine() { }

    public ProdOrderRoutingLine(Guid id, Guid orderId, RoutingLine line, decimal unitCostPerHour, decimal orderQuantity)
        : base(id)
    {
        ProductionOrderId = orderId;
        OperationNo = line.OperationNo;
        Type = line.Type;
        No = line.No;
        Description = line.Description;
        SetupTime = line.SetupTime;
        RunTime = line.RunTime;
        UnitCostPerHour = unitCostPerHour;
        ExpectedMinutes = line.SetupTime + line.RunTime * orderQuantity;
    }

    internal void Use(decimal minutes, decimal cost)
    {
        ActualMinutes += minutes;
        CostAmount += cost;
    }
}

/// <summary>Capacity Ledger Entry: time used on an operation of a production order, at its cost.</summary>
public class CapacityLedgerEntry : LedgerEntryBase
{
    public string ProductionOrderNo { get; internal set; }
    public string OperationNo { get; internal set; }
    public RoutingLineType Type { get; internal set; }
    public string No { get; internal set; }
    public string ItemNo { get; internal set; }
    public DateTime PostingDate { get; internal set; }
    public string DocumentNo { get; internal set; }
    public decimal OutputQuantity { get; internal set; }
    public decimal Minutes { get; internal set; }
    public decimal CostAmount { get; internal set; }

    protected CapacityLedgerEntry() { }

    public CapacityLedgerEntry(Guid id)
        : base(id) { }
}

/// <summary>A line of the planning worksheet: something to buy or make, by when.</summary>
public class PlanningLine : CompanyEntity
{
    public string ItemNo { get; private set; }
    public string Description { get; private set; }
    public PlanningActionMessage ActionMessage { get; private set; }
    public ReplenishmentSystem ReplenishmentSystem { get; private set; }
    public string VendorNo { get; private set; }
    public string LocationCode { get; private set; }
    public decimal Quantity { get; private set; }
    public DateTime DueDate { get; private set; }

    /// <summary>The level of the item in the bills of materials: 0 is sold, deeper levels are components.</summary>
    public int LowLevelCode { get; private set; }

    public bool AcceptActionMessage { get; private set; } = true;

    /// <summary>The order Carry Out made of the line.</summary>
    public string CarriedOutNo { get; private set; }

    protected PlanningLine() { }

    public PlanningLine(Guid id, string itemNo, string description, ReplenishmentSystem replenishment, string vendorNo, string locationCode, decimal quantity, DateTime dueDate, int lowLevelCode)
        : base(id)
    {
        ItemNo = itemNo;
        Description = description;
        ReplenishmentSystem = replenishment;
        VendorNo = vendorNo;
        LocationCode = locationCode;
        LowLevelCode = lowLevelCode;
        Set(quantity, dueDate, true);
    }

    public void Set(decimal quantity, DateTime dueDate, bool accept)
    {
        Quantity = Math.Max(0m, quantity);
        DueDate = dueDate.Date;
        AcceptActionMessage = accept;
    }

    public void SetVendor(string vendorNo) => VendorNo = CodeTableEntity.NormalizeCode(Check.Length(vendorNo, nameof(vendorNo), ErpDomainConsts.MaxNoLength));

    internal void CarriedOut(string orderNo) => CarriedOutNo = orderNo;
}

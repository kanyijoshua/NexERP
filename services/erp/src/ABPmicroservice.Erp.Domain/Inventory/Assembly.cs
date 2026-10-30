using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Assembly BOM component: one item that goes into an assembled item, and how many of it per
/// unit. Kitting without production: no routing, no work centres, no versions.
/// </summary>
public class AssemblyBomComponent : CompanyEntity
{
    public string ParentItemNo { get; private set; }
    public int LineNo { get; private set; }
    public string ComponentItemNo { get; private set; }
    public string Description { get; private set; }

    /// <summary>Base units of the component per base unit of the parent.</summary>
    public decimal QuantityPer { get; private set; }

    protected AssemblyBomComponent() { }

    public AssemblyBomComponent(Guid id, string parentItemNo, int lineNo)
        : base(id)
    {
        ParentItemNo = Check.NotNullOrWhiteSpace(parentItemNo, nameof(parentItemNo), ErpDomainConsts.MaxNoLength).Trim();
        LineNo = lineNo;
    }

    public void Set(string componentItemNo, string description, decimal quantityPer)
    {
        var component = Check.NotNullOrWhiteSpace(componentItemNo, nameof(componentItemNo), ErpDomainConsts.MaxNoLength).Trim();
        if (string.Equals(component, ParentItemNo, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(ErpErrorCodes.Inventory.AssemblyBomRecursive).WithData("itemNo", ParentItemNo);
        }

        if (quantityPer <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.AssemblyQuantityTooLarge)
                .WithData("no", ParentItemNo)
                .WithData("quantity", quantityPer)
                .WithData("remaining", 0);
        }

        ComponentItemNo = component;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        QuantityPer = quantityPer;
    }
}

/// <summary>
/// Assembly Order: puts together a quantity of an item from the components of its assembly BOM.
/// Posting consumes the components and outputs the assembled item at what the components cost; an
/// order can be posted in parts, as its quantity to assemble says.
/// </summary>
public class AssemblyHeader : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string Description { get; private set; }
    public string VariantCode { get; private set; }
    public string LocationCode { get; private set; }
    public string BinCode { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal QuantityToAssemble { get; private set; }
    public decimal AssembledQuantity { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime PostingDate { get; private set; }
    public AssemblyOrderStatus Status { get; private set; }

    /// <summary>The cost of one assembled unit, from the last posting.</summary>
    public decimal UnitCost { get; private set; }

    public Collection<AssemblyLine> Lines { get; private set; } = new();

    public decimal RemainingQuantity => Quantity - AssembledQuantity;

    protected AssemblyHeader() { }

    public AssemblyHeader(Guid id, string no, Item item)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        ItemId = item.Id;
        ItemNo = item.No;
        Description = item.Description;
        Status = AssemblyOrderStatus.Open;
    }

    public void Set(string description, string variantCode, string locationCode, string binCode, DateTime dueDate, DateTime postingDate)
    {
        EnsureNotFinished();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength) ?? Description;
        if (AssembledQuantity == 0m)
        {
            VariantCode = CodeTableEntity.NormalizeCode(variantCode);
            LocationCode = CodeTableEntity.NormalizeCode(locationCode);
            foreach (var line in Lines.Where(l => l.ConsumedQuantity == 0m))
            {
                line.SetLocation(LocationCode);
            }
        }

        BinCode = Bin.NormalizeCode(binCode);
        DueDate = dueDate.Date;
        PostingDate = postingDate.Date;
    }

    /// <summary>A new quantity scales the component lines; it cannot go below what is assembled.</summary>
    public void SetQuantity(decimal quantity, decimal? quantityToAssemble)
    {
        EnsureNotFinished();
        if (quantity <= 0m || quantity < AssembledQuantity)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.AssemblyQuantityTooLarge)
                .WithData("no", No)
                .WithData("quantity", quantity)
                .WithData("remaining", AssembledQuantity);
        }

        Quantity = quantity;
        foreach (var line in Lines)
        {
            line.Scale(quantity);
        }

        SetQuantityToAssemble(quantityToAssemble ?? RemainingQuantity);
    }

    public void SetQuantityToAssemble(decimal quantityToAssemble)
    {
        if (quantityToAssemble < 0m || quantityToAssemble > RemainingQuantity)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.AssemblyQuantityTooLarge)
                .WithData("no", No)
                .WithData("quantity", quantityToAssemble)
                .WithData("remaining", RemainingQuantity);
        }

        QuantityToAssemble = quantityToAssemble;
    }

    /// <summary>Replaces the component lines with the BOM; only before anything is consumed.</summary>
    public void SetComponents(IEnumerable<(Guid Id, Item Item, decimal QuantityPer, string Description)> components)
    {
        EnsureNotFinished();
        if (Lines.Any(l => l.ConsumedQuantity != 0m))
        {
            return;
        }

        Lines.Clear();
        var lineNo = 0;
        foreach (var (id, item, quantityPer, description) in components)
        {
            lineNo += 10000;
            Lines.Add(new AssemblyLine(id, Id, lineNo, item, description, quantityPer, Quantity, LocationCode));
        }
    }

    public void Release()
    {
        EnsureNotFinished();
        Status = AssemblyOrderStatus.Released;
    }

    public void Reopen()
    {
        EnsureNotFinished();
        Status = AssemblyOrderStatus.Open;
    }

    public void EnsureNotFinished()
    {
        if (Status == AssemblyOrderStatus.Finished)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.AssemblyOrderFinished).WithData("no", No);
        }
    }

    internal void RecordAssembled(decimal quantity, decimal unitCost)
    {
        AssembledQuantity += quantity;
        UnitCost = unitCost;
        QuantityToAssemble = RemainingQuantity;
        if (RemainingQuantity == 0m)
        {
            Status = AssemblyOrderStatus.Finished;
        }
    }
}

public class AssemblyLine : Entity<Guid>
{
    public Guid AssemblyHeaderId { get; private set; }
    public int LineNo { get; private set; }
    public Guid ItemId { get; private set; }
    public string ItemNo { get; private set; }
    public string Description { get; private set; }
    public string VariantCode { get; private set; }
    public string LocationCode { get; private set; }
    public decimal QuantityPer { get; private set; }

    /// <summary>What the whole order needs, in base units.</summary>
    public decimal Quantity { get; private set; }

    public decimal ConsumedQuantity { get; private set; }

    public decimal RemainingQuantity => Quantity - ConsumedQuantity;

    protected AssemblyLine() { }

    public AssemblyLine(Guid id, Guid headerId, int lineNo, Item item, string description, decimal quantityPer, decimal headerQuantity, string locationCode)
        : base(id)
    {
        AssemblyHeaderId = headerId;
        LineNo = lineNo;
        ItemId = item.Id;
        ItemNo = item.No;
        Description = Check.Length(description ?? item.Description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        QuantityPer = quantityPer;
        LocationCode = locationCode;
        Scale(headerQuantity);
    }

    internal void Scale(decimal headerQuantity) => Quantity = Math.Round(QuantityPer * headerQuantity, 5, MidpointRounding.AwayFromZero);

    internal void SetLocation(string locationCode) => LocationCode = locationCode;

    public void SetVariant(string variantCode) => VariantCode = CodeTableEntity.NormalizeCode(variantCode);

    internal void RecordConsumed(decimal quantity) => ConsumedQuantity += quantity;
}

/// <summary>Builds assembly orders from BOMs, reserves their components and posts them.</summary>
public class AssemblyManager : DomainService
{
    private const string SourceCode = "ASSEMBLY";

    private readonly IRepository<AssemblyHeader, Guid> _orderRepository;
    private readonly IRepository<AssemblyBomComponent, Guid> _bomRepository;
    private readonly IRepository<Item, Guid> _itemRepository;

    private ItemJnlPostLine PostLine => LazyServiceProvider.LazyGetRequiredService<ItemJnlPostLine>();
    private InventoryCostPoster CostPoster => LazyServiceProvider.LazyGetRequiredService<InventoryCostPoster>();
    private GLRegisterManager RegisterManager => LazyServiceProvider.LazyGetRequiredService<GLRegisterManager>();
    private GeneralLedgerSetupManager GLSetupManager => LazyServiceProvider.LazyGetRequiredService<GeneralLedgerSetupManager>();
    private ItemTrackingManager TrackingManager => LazyServiceProvider.LazyGetRequiredService<ItemTrackingManager>();
    private ReservationManager ReservationManager => LazyServiceProvider.LazyGetRequiredService<ReservationManager>();
    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public AssemblyManager(IRepository<AssemblyHeader, Guid> orderRepository, IRepository<AssemblyBomComponent, Guid> bomRepository, IRepository<Item, Guid> itemRepository)
    {
        _orderRepository = orderRepository;
        _bomRepository = bomRepository;
        _itemRepository = itemRepository;
    }

    public async Task<string> NextNoAsync() => $"AO{await EntryNos.NextAsync(ErpSequenceNames.AssemblyOrderNo):D5}";

    public async Task<Item> GetItemAsync(string itemNo)
    {
        var no = itemNo?.Trim();
        return await _itemRepository.FirstOrDefaultAsync(i => i.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", no ?? "");
    }

    /// <summary>Fills the order's component lines from the item's assembly BOM.</summary>
    public async Task ExplodeBomAsync(AssemblyHeader order)
    {
        var parent = order.ItemNo;
        var bom = (await _bomRepository.GetListAsync(c => c.ParentItemNo == parent)).OrderBy(c => c.LineNo).ToList();
        if (bom.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.AssemblyBomMissing).WithData("itemNo", parent);
        }

        var components = new List<(Guid, Item, decimal, string)>();
        foreach (var component in bom)
        {
            components.Add((GuidGenerator.Create(), await GetItemAsync(component.ComponentItemNo), component.QuantityPer, component.Description));
        }

        order.SetComponents(components);
    }

    /// <summary>Reserves what each component line still needs at the order's location; returns the lines it reserved for.</summary>
    public async Task<int> ReserveComponentsAsync(Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId, includeDetails: true);
        order.EnsureNotFinished();
        var reserved = 0;
        foreach (var line in order.Lines)
        {
            var wanted = line.RemainingQuantity - await ReservationManager.GetReservedForSourceAsync(line.Id);
            if (wanted <= 0m)
            {
                continue;
            }

            var item = await _itemRepository.GetAsync(line.ItemId);
            if (await ReservationManager.ReserveAsync(item, line.LocationCode, line.VariantCode, wanted, ReservationSourceType.AssemblyLine, line.Id, order.No, line.LineNo, order.DueDate) > 0m)
            {
                reserved++;
            }
        }

        if (reserved == 0)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.NothingToReserve).WithData("itemNo", order.ItemNo).WithData("locationCode", order.LocationCode ?? "");
        }

        return reserved;
    }

    /// <summary>
    /// Consumes the components for the quantity to assemble and outputs that quantity of the item at
    /// their cost. The cost moves between the components' and the item's inventory accounts through
    /// the inventory adjustment account, which nets to nothing.
    /// </summary>
    public async Task<AssemblyHeader> PostAsync(Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId, includeDetails: true);
        order.EnsureNotFinished();
        var quantity = order.QuantityToAssemble;
        if (quantity <= 0m || quantity > order.RemainingQuantity)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.AssemblyQuantityTooLarge)
                .WithData("no", order.No)
                .WithData("quantity", quantity)
                .WithData("remaining", order.RemainingQuantity);
        }

        await GLSetupManager.CheckPostingDateAsync(order.PostingDate);
        if (order.Status == AssemblyOrderStatus.Open)
        {
            order.Release();
        }

        var register = await RegisterManager.OpenAsync(order.PostingDate, SourceCode, order.No);
        var context = new GLPostingContext(register, SourceCode);
        var last = quantity == order.RemainingQuantity;
        var totalCost = 0m;

        foreach (var line in order.Lines.OrderBy(l => l.LineNo))
        {
            // The last part takes what the line has left, so rounding never leaves a remainder.
            var consume = last ? line.RemainingQuantity : Math.Round(line.QuantityPer * quantity, 5, MidpointRounding.AwayFromZero);
            if (consume <= 0m)
            {
                continue;
            }

            var component = await _itemRepository.GetAsync(line.ItemId);
            var consumed = await PostLine.PostAllAsync(new ItemPostingRequest
            {
                ItemId = component.Id,
                ItemNo = component.No,
                PostingDate = order.PostingDate,
                EntryType = ItemLedgerEntryType.AssemblyConsumption,
                DocumentNo = order.No,
                Description = line.Description,
                Quantity = consume,
                LocationCode = line.LocationCode,
                VariantCode = line.VariantCode,
                Tracking = await TrackingManager.TakeAsync(component, ItemTrackingSourceType.AssemblyLine, line.Id, consume),
                ReservationSourceId = line.Id,
            });

            var cost = -consumed.Sum(e => Math.Round(e.Quantity * e.UnitCost, 2, MidpointRounding.AwayFromZero));
            await CostPoster.PostAdjustmentAsync(component, line.LocationCode, -cost, order.PostingDate, order.No, line.Description, context);
            totalCost += cost;
            line.RecordConsumed(consume);
        }

        var item = await _itemRepository.GetAsync(order.ItemId);
        var unitCost = Math.Round(totalCost / quantity, 5, MidpointRounding.AwayFromZero);
        await PostLine.PostAllAsync(new ItemPostingRequest
        {
            ItemId = item.Id,
            ItemNo = item.No,
            PostingDate = order.PostingDate,
            EntryType = ItemLedgerEntryType.AssemblyOutput,
            DocumentNo = order.No,
            Description = order.Description,
            Quantity = quantity,
            UnitCost = unitCost,
            LocationCode = order.LocationCode,
            VariantCode = order.VariantCode,
            BinCode = order.BinCode,
            Tracking = await TrackingManager.TakeAsync(item, ItemTrackingSourceType.AssemblyOrder, order.Id, quantity),
        });
        await CostPoster.PostAdjustmentAsync(item, order.LocationCode, totalCost, order.PostingDate, order.No, order.Description, context);
        await RegisterManager.CloseAsync(register);

        order.RecordAssembled(quantity, unitCost);
        if (order.Status == AssemblyOrderStatus.Finished)
        {
            await ReservationManager.CancelAsync(order.Lines.Select(l => l.Id));
        }

        return await _orderRepository.UpdateAsync(order, autoSave: true);
    }
}

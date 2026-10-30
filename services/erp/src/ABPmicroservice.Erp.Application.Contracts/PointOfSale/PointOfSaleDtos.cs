using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.PointOfSale;

public class PosTerminalDto : CodeTableDto
{
    public string LocationCode { get; set; }
    public string WalkInCustomerNo { get; set; }
    public string CashBankAccountNo { get; set; }
    public string CardBankAccountNo { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdatePosTerminalDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string WalkInCustomerNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CashBankAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CardBankAccountNo { get; set; }

    public bool Blocked { get; set; }
}

public interface IPosTerminalAppService : ICrudAppService<PosTerminalDto, Guid, GetCodeTableListInput, CreateUpdatePosTerminalDto> { }

public class PosSessionDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string TerminalCode { get; set; }
    public DateTime OpenedAt { get; set; }
    public string OpenedBy { get; set; }
    public decimal OpeningFloat { get; set; }
    public PosSessionStatus Status { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal CashSales { get; set; }
    public decimal CardSales { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal CountedCash { get; set; }
    public decimal Difference { get; set; }
}

public class GetPosSessionListInput : ErpPagedListInput
{
    public string TerminalCode { get; set; }
    public PosSessionStatus? Status { get; set; }
}

public class OpenPosSessionInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string TerminalCode { get; set; }

    public decimal OpeningFloat { get; set; }
}

public class ClosePosSessionInput
{
    public decimal CountedCash { get; set; }
}

public interface IPosSessionAppService : IReadOnlyAppService<PosSessionDto, Guid, GetPosSessionListInput>
{
    Task<PosSessionDto> OpenAsync(OpenPosSessionInput input);

    Task<PosSessionDto> CloseAsync(Guid id, ClosePosSessionInput input);
}

public class PosOrderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public Guid SessionId { get; set; }
    public string TerminalCode { get; set; }
    public string CustomerNo { get; set; }
    public DateTime OrderDate { get; set; }
    public PosOrderStatus Status { get; set; }
    public PosPaymentMethod PaymentMethod { get; set; }
    public decimal Subtotal { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal Change { get; set; }
    public decimal TotalPaid { get; set; }
    public string PostedInvoiceNo { get; set; }
}

public class GetPosOrderListInput : ErpPagedListInput
{
    public Guid? SessionId { get; set; }
    public PosOrderStatus? Status { get; set; }
}

public class StartPosOrderInput
{
    public Guid SessionId { get; set; }

    /// <summary>Blank sells to the terminal's walk-in customer.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }
}

public class RingUpInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ItemNo { get; set; }

    public decimal Quantity { get; set; } = 1m;

    /// <summary>Blank takes the item's price.</summary>
    public decimal? UnitPrice { get; set; }

    public decimal DiscountPct { get; set; }
}

public class PayPosOrderInput
{
    public PosPaymentMethod Method { get; set; }
    public decimal AmountTendered { get; set; }
}

public interface IPosOrderAppService : IReadOnlyAppService<PosOrderDto, Guid, GetPosOrderListInput>
{
    Task<PosOrderDto> StartAsync(StartPosOrderInput input);

    Task<PosOrderDto> RingUpAsync(Guid id, RingUpInput input);

    Task<PosOrderDto> PayAsync(Guid id, PayPosOrderInput input);

    Task<PosOrderDto> CancelAsync(Guid id);
}

public class PosOrderLineDto : EntityDto<Guid>
{
    public Guid PosOrderId { get; set; }
    public int LineNo { get; set; }
    public string ItemNo { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPct { get; set; }
    public decimal LineAmount { get; set; }
}

public class GetPosOrderLineListInput : ErpPagedListInput
{
    public Guid? PosOrderId { get; set; }
}

public interface IPosOrderLineAppService : IReadOnlyAppService<PosOrderLineDto, Guid, GetPosOrderLineListInput>
{
    Task DeleteAsync(Guid id);
}

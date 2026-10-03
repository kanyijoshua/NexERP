import type { PurchaseDocumentType } from './purchase-document-type.enum';
import type { PurchasingDiscountPosting } from './purchasing-discount-posting.enum';
import type { VatCalculationType } from '../finance/vat-calculation-type.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { DocumentStatus } from '../documents/document-status.enum';
import type { DocumentLineType } from '../documents/document-line-type.enum';

export interface CreateUpdatePurchaseHeaderDto {
  documentType: PurchaseDocumentType;
  no?: string;
  vendorId?: string;
  postingDate?: string;
  dueDate?: string;
  expectedReceiptDate?: string;
  currencyCode?: string;
  paymentTermsCode?: string;
  paymentMethodCode?: string;
  shipmentMethodCode?: string;
  locationCode?: string;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  vendorPostingGroup?: string;
  genBusPostingGroup?: string;
  vatBusPostingGroup?: string;
  pricesIncludingVat?: boolean;
  onHold?: string;
  appliesToDocType?: PurchaseDocumentType;
  appliesToDocNo?: string;
  appliesToId?: string;
  taxAreaCode?: string;
  taxLiable?: boolean;
  prepaymentPct?: number;
  yourReference?: string;
  vendorInvoiceNo?: string;
  payToVendorNo?: string;
  payToName?: string;
  payToAddress?: string;
  payToCity?: string;
  payToPostCode?: string;
  payToCountryRegionCode?: string;
  payToContact?: string;
  shipToCode?: string;
  shipToName?: string;
  shipToAddress?: string;
  shipToCity?: string;
  shipToPostCode?: string;
  shipToCountryRegionCode?: string;
  shipToContact?: string;
  lines: PurchaseLineInputDto[];
}

export interface CreateUpdateVendorDto {
  no?: string;
  name?: string;
  searchName?: string;
  name2?: string;
  address?: string;
  address2?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  mobilePhoneNo?: string;
  email?: string;
  homePage?: string;
  contact?: string;
  ourAccountNo?: string;
  paymentTermsCode?: string;
  paymentMethodCode?: string;
  shipmentMethodCode?: string;
  shippingAgentCode?: string;
  locationCode?: string;
  vendorPostingGroup?: string;
  genBusPostingGroup?: string;
  vatBusPostingGroup?: string;
  invoiceDiscCode?: string;
  pricesIncludingVAT?: boolean;
  vatRegistrationNo?: string;
  taxAreaCode?: string;
  taxLiable?: boolean;
  blockPaymentTolerance?: boolean;
  prepaymentPct?: number;
  allowMultiplePostingGroups?: boolean;
  leadTimeCalculation?: string;
  purchaserCode?: string;
  currencyCode?: string;
}

export interface GetPurchaseDocumentListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  documentType?: PurchaseDocumentType;
  status?: DocumentStatus;
  vendorId?: string;
}

export interface GetVendorListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  blocked?: boolean;
}

export interface PurchaseHeaderDto extends FullAuditedEntityDto<string> {
  documentType: PurchaseDocumentType;
  no?: string;
  vendorId?: string;
  buyFromVendorNo?: string;
  buyFromVendorName?: string;
  payToVendorNo?: string;
  payToName?: string;
  payToAddress?: string;
  payToCity?: string;
  payToPostCode?: string;
  payToCountryRegionCode?: string;
  payToContact?: string;
  shipToCode?: string;
  shipToName?: string;
  shipToAddress?: string;
  shipToCity?: string;
  shipToPostCode?: string;
  shipToCountryRegionCode?: string;
  shipToContact?: string;
  postingDate?: string;
  orderDate?: string;
  dueDate?: string;
  expectedReceiptDate?: string;
  status: DocumentStatus;
  currencyCode?: string;
  paymentTermsCode?: string;
  paymentMethodCode?: string;
  shipmentMethodCode?: string;
  locationCode?: string;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  vendorPostingGroup?: string;
  genBusPostingGroup?: string;
  vatBusPostingGroup?: string;
  pricesIncludingVat: boolean;
  onHold?: string;
  appliesToDocType?: PurchaseDocumentType;
  appliesToDocNo?: string;
  appliesToId?: string;
  taxAreaCode?: string;
  taxLiable: boolean;
  prepaymentPct: number;
  yourReference?: string;
  vendorInvoiceNo?: string;
  totalAmount: number;
  totalAmountIncludingVat: number;
  posted: boolean;
  postedDocumentNo?: string;
  lines: PurchaseLineDto[];
}

export interface PurchaseLineDto extends EntityDto<string> {
  purchaseHeaderId?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  directUnitCost: number;
  lineDiscountPercent: number;
  lineAmount: number;
  lineAmountIncludingVat: number;
  unitOfMeasureCode?: string;
  locationCode?: string;
  expectedReceiptDate?: string;
  itemCategoryCode?: string;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  qtyToReceive: number;
  quantityReceived: number;
  qtyToInvoice: number;
  quantityInvoiced: number;
  deferralCode?: string;
  taxAreaCode?: string;
  taxLiable: boolean;
  taxGroupCode?: string;
  genBusPostingGroup?: string;
  genProdPostingGroup?: string;
  vatBusPostingGroup?: string;
  vatProdPostingGroup?: string;
  vatCalculationType: VatCalculationType;
  vatIdentifier?: string;
  vatPercent: number;
  vatBaseAmount: number;
  vatAmount: number;
}

export interface PurchaseLineInputDto {
  id?: string;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  directUnitCost: number;
  lineDiscountPercent: number;
  unitOfMeasureCode?: string;
  locationCode?: string;
  expectedReceiptDate?: string;
  itemCategoryCode?: string;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  qtyToReceive?: number;
  quantityReceived?: number;
  qtyToInvoice?: number;
  quantityInvoiced?: number;
  deferralCode?: string;
  taxAreaCode?: string;
  taxLiable?: boolean;
  taxGroupCode?: string;
  genBusPostingGroup?: string;
  genProdPostingGroup?: string;
}

export interface PurchasesPayablesSetupDto {
  vendorNos?: string;
  quoteNos?: string;
  orderNos?: string;
  invoiceNos?: string;
  creditMemoNos?: string;
  postedInvoiceNos?: string;
  postedCreditMemoNos?: string;
  blanketOrderNos?: string;
  postedReceiptNos?: string;
  postedReturnShptNos?: string;
  returnOrderNos?: string;
  discountPosting: PurchasingDiscountPosting;
  receiptOnInvoice: boolean;
  invoiceRounding: boolean;
  extDocNoMandatory: boolean;
  calcInvDiscount: boolean;
  allowVATDifference: boolean;
  calcInvDiscPerVATID: boolean;
  exactCostReversingMandatory: boolean;
  postWithJobQueue: boolean;
  jobQueueCategoryCode?: string;
  notifyOnSuccess: boolean;
  copyCommentsBlanketToOrder: boolean;
  copyCommentsOrderToInvoice: boolean;
  copyCommentsOrderToReceipt: boolean;
  copyCommentsRetOrderToCrMemo: boolean;
  copyCommentsRetOrderToRetShpt: boolean;
  returnShipmentOnCreditMemo: boolean;
  copyVendorNameToEntries: boolean;
  copyLineDescrToGLEntry: boolean;
  allowMultiplePostingGroups: boolean;
}

export interface VendorDto extends FullAuditedEntityDto<string> {
  no?: string;
  name?: string;
  searchName?: string;
  name2?: string;
  address?: string;
  address2?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  mobilePhoneNo?: string;
  email?: string;
  homePage?: string;
  contact?: string;
  ourAccountNo?: string;
  balance: number;
  paymentTermsCode?: string;
  paymentMethodCode?: string;
  shipmentMethodCode?: string;
  shippingAgentCode?: string;
  locationCode?: string;
  vendorPostingGroup?: string;
  genBusPostingGroup?: string;
  vatBusPostingGroup?: string;
  invoiceDiscCode?: string;
  pricesIncludingVAT: boolean;
  vatRegistrationNo?: string;
  taxAreaCode?: string;
  taxLiable: boolean;
  blockPaymentTolerance: boolean;
  prepaymentPct: number;
  allowMultiplePostingGroups: boolean;
  leadTimeCalculation?: string;
  purchaserCode?: string;
  currencyCode?: string;
  blocked: boolean;
}

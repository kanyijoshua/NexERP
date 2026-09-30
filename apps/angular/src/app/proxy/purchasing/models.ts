import type { PurchaseDocumentType } from './purchase-document-type.enum';
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
  locationCode?: string;
  vendorInvoiceNo?: string;
  lines: PurchaseLineInputDto[];
}

export interface CreateUpdateVendorDto {
  no?: string;
  name?: string;
  address?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  email?: string;
  paymentTermsCode?: string;
  vendorPostingGroup?: string;
  genBusPostingGroup?: string;
  vatBusPostingGroup?: string;
  purchaserCode?: string;
  paymentMethodCode?: string;
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
  postingDate?: string;
  orderDate?: string;
  dueDate?: string;
  expectedReceiptDate?: string;
  status: DocumentStatus;
  currencyCode?: string;
  paymentTermsCode?: string;
  locationCode?: string;
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
  vatBusPostingGroup?: string;
  vatProdPostingGroup?: string;
  vatCalculationType: VatCalculationType;
  vatIdentifier?: string;
  vatPercent: number;
  vatBaseAmount: number;
  vatAmount: number;
  unitOfMeasureCode?: string;
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
}

export interface PurchasesPayablesSetupDto {
  vendorNos?: string;
  quoteNos?: string;
  orderNos?: string;
  invoiceNos?: string;
  creditMemoNos?: string;
  postedInvoiceNos?: string;
  postedCreditMemoNos?: string;
  extDocNoMandatory: boolean;
}

export interface VendorDto extends FullAuditedEntityDto<string> {
  no?: string;
  name?: string;
  address?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  email?: string;
  balance: number;
  paymentTermsCode?: string;
  vendorPostingGroup?: string;
  genBusPostingGroup?: string;
  vatBusPostingGroup?: string;
  purchaserCode?: string;
  paymentMethodCode?: string;
  currencyCode?: string;
  blocked: boolean;
}

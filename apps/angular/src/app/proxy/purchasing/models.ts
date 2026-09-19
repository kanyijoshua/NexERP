import type { PurchaseDocumentType } from './purchase-document-type.enum';
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
  currencyCode?: string;
}

export interface GetPurchaseDocumentListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  documentType?: PurchaseDocumentType;
  status?: DocumentStatus;
  vendorId?: string;
}

export interface GetVendorListInput extends PagedAndSortedResultRequestDto {
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
  currencyCode?: string;
  blocked: boolean;
}

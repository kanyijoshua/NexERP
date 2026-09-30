import type { SalesDocumentType } from './sales-document-type.enum';
import type { CreditWarnings } from './credit-warnings.enum';
import type { VatCalculationType } from '../finance/vat-calculation-type.enum';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { DocumentStatus } from '../documents/document-status.enum';
import type { DocumentLineType } from '../documents/document-line-type.enum';

export interface CreateUpdateCustomerDto {
  no?: string;
  name?: string;
  address?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  email?: string;
  creditLimit: number;
  paymentTermsCode?: string;
  customerPostingGroup?: string;
  genBusPostingGroup?: string;
  vatBusPostingGroup?: string;
  salespersonCode?: string;
  paymentMethodCode?: string;
  currencyCode?: string;
}

export interface CreateUpdateSalesHeaderDto {
  documentType: SalesDocumentType;
  no?: string;
  customerId?: string;
  postingDate?: string;
  dueDate?: string;
  currencyCode?: string;
  paymentTermsCode?: string;
  locationCode?: string;
  externalDocumentNo?: string;
  lines: SalesLineInputDto[];
}

export interface CustomerDto extends FullAuditedEntityDto<string> {
  no?: string;
  name?: string;
  address?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  email?: string;
  creditLimit: number;
  balance: number;
  paymentTermsCode?: string;
  customerPostingGroup?: string;
  genBusPostingGroup?: string;
  vatBusPostingGroup?: string;
  salespersonCode?: string;
  paymentMethodCode?: string;
  currencyCode?: string;
  blocked: boolean;
}

export interface GetCustomerListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  blocked?: boolean;
}

export interface GetSalesDocumentListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  documentType?: SalesDocumentType;
  status?: DocumentStatus;
  customerId?: string;
}

export interface SalesHeaderDto extends FullAuditedEntityDto<string> {
  documentType: SalesDocumentType;
  no?: string;
  customerId?: string;
  sellToCustomerNo?: string;
  sellToCustomerName?: string;
  billToCustomerNo?: string;
  postingDate?: string;
  orderDate?: string;
  dueDate?: string;
  status: DocumentStatus;
  currencyCode?: string;
  paymentTermsCode?: string;
  locationCode?: string;
  externalDocumentNo?: string;
  totalAmount: number;
  totalAmountIncludingVat: number;
  posted: boolean;
  postedDocumentNo?: string;
  lines: SalesLineDto[];
}

export interface SalesLineDto extends EntityDto<string> {
  salesHeaderId?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
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

export interface SalesLineInputDto {
  id?: string;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  lineDiscountPercent: number;
  unitOfMeasureCode?: string;
}

export interface SalesReceivablesSetupDto {
  customerNos?: string;
  quoteNos?: string;
  orderNos?: string;
  invoiceNos?: string;
  creditMemoNos?: string;
  postedInvoiceNos?: string;
  postedCreditMemoNos?: string;
  creditWarnings: CreditWarnings;
  extDocNoMandatory: boolean;
}

export interface SalespersonPurchaserDto extends CodeTableDto {
  email?: string;
  phoneNo?: string;
  jobTitle?: string;
  commissionPercent: number;
  blocked: boolean;
}

export interface CreateUpdateSalespersonPurchaserDto extends CreateUpdateCodeTableDto {
  email?: string;
  phoneNo?: string;
  jobTitle?: string;
  commissionPercent: number;
  blocked: boolean;
}

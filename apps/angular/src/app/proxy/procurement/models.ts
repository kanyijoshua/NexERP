import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { DocumentLineType } from '../documents/document-line-type.enum';
import type { PurchaseHeaderDto } from '../purchasing/models';
import type { ApprovalRequestResultDto } from '../workflows/models';
import type {
  ProcurementMethod,
  ProcurementPlanStatus,
  ProcurementType,
  RequisitionStatus,
  RequisitionType,
  TenderCommitteeRole,
  TenderCommitteeType,
  TenderStatus,
  TenderType,
} from './procurement-enums';

// ---------------------- Procurement Plan ----------------------
export interface ProcurementPlanDto extends FullAuditedEntityDto<string> {
  code?: string;
  description?: string;
  budgetName?: string;
  status: ProcurementPlanStatus;
  startDate?: string;
  endDate?: string;
  totalEstimatedAmount: number;
  lines: ProcurementPlanLineDto[];
}

export interface ProcurementPlanLineDto extends FullAuditedEntityDto<string> {
  planId?: string;
  planItemNo?: string;
  description?: string;
  procurementType: ProcurementType;
  procurementMethod: ProcurementMethod;
  unitOfMeasure?: string;
  quantity: number;
  unitPrice: number;
  estimatedCost: number;
  sourceOfFunds?: string;
  departmentCode?: string;
  quarter?: string;
}

export interface CreateUpdateProcurementPlanDto {
  code: string;
  description: string;
  budgetName?: string;
  startDate?: string;
  endDate?: string;
  lines?: CreateUpdateProcurementPlanLineDto[];
}

export interface CreateUpdateProcurementPlanLineDto {
  id?: string;
  planItemNo: string;
  description: string;
  procurementType: ProcurementType;
  procurementMethod: ProcurementMethod;
  unitOfMeasure?: string;
  quantity: number;
  unitPrice: number;
  sourceOfFunds?: string;
  departmentCode?: string;
  quarter?: string;
}

export interface GetProcurementPlanListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  status?: ProcurementPlanStatus;
}

// ---------------------- Purchase Requisition ----------------------
export interface PurchaseRequisitionDto extends FullAuditedEntityDto<string> {
  no?: string;
  requestType: RequisitionType;
  requestedBy?: string;
  departmentCode?: string;
  procurementPlanCode?: string;
  requestDate: string;
  requiredDate?: string;
  status: RequisitionStatus;
  description?: string;
  tenderNo?: string;
  purchaseOrderNo?: string;
  totalEstimatedCost: number;
  lines: PurchaseRequisitionLineDto[];
}

export interface PurchaseRequisitionLineDto extends FullAuditedEntityDto<string> {
  requisitionId?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  specifications?: string;
  procurementPlanItemNo?: string;
  quantity: number;
  estimatedUnitCost: number;
  estimatedTotalCost: number;
  unitOfMeasure?: string;
  orderedQuantity: number;
}

export interface CreateUpdatePurchaseRequisitionDto {
  no?: string;
  requestType: RequisitionType;
  requestedBy?: string;
  departmentCode?: string;
  procurementPlanCode?: string;
  requestDate?: string;
  requiredDate?: string;
  description?: string;
  lines?: CreateUpdatePurchaseRequisitionLineDto[];
}

export interface CreateUpdatePurchaseRequisitionLineDto {
  id?: string;
  type: DocumentLineType;
  no: string;
  description: string;
  specifications?: string;
  procurementPlanItemNo?: string;
  quantity: number;
  estimatedUnitCost: number;
  unitOfMeasure?: string;
}

export interface ConvertRequisitionToOrderInput {
  vendorId: string;
}

export interface ConvertRequisitionToTenderInput {
  tenderType: TenderType;
  title: string;
  closingDate: string;
  closingTime?: string;
  validityDays?: number;
  tenderSecurityAmount?: number;
}

export interface GetPurchaseRequisitionListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  requestType?: RequisitionType;
  status?: RequisitionStatus;
  departmentCode?: string;
}

// ---------------------- Tenders / RFQ ----------------------
export interface TenderHeaderDto extends FullAuditedEntityDto<string> {
  no?: string;
  title?: string;
  tenderType: TenderType;
  requisitionNo?: string;
  procurementPlanCode?: string;
  procurementPlanItemNo?: string;
  category?: string;
  status: TenderStatus;
  issueDate: string;
  closingDate: string;
  closingTime?: string;
  validityDays: number;
  tenderSecurityAmount: number;
  awardedVendorNo?: string;
  awardedVendorName?: string;
  awardedAmount: number;
  awardDate?: string;
  purchaseOrderNo?: string;
  totalEstimatedAmount: number;
  lines: TenderLineDto[];
  committees: TenderCommitteeDto[];
  mandatoryRequirements: TenderMandatoryRequirementDto[];
  bids: TenderBidDto[];
}

export interface TenderLineDto extends FullAuditedEntityDto<string> {
  tenderId?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  specifications?: string;
  quantity: number;
  estimatedUnitCost: number;
  estimatedCost: number;
  unitOfMeasure?: string;
}

export interface TenderCommitteeDto extends FullAuditedEntityDto<string> {
  tenderId?: string;
  committeeType: TenderCommitteeType;
  staffNo?: string;
  staffName?: string;
  role: TenderCommitteeRole;
  appointmentDate: string;
}

export interface TenderMandatoryRequirementDto extends FullAuditedEntityDto<string> {
  tenderId?: string;
  requirementDescription?: string;
  isMandatory: boolean;
}

export interface TenderBidDto extends FullAuditedEntityDto<string> {
  tenderId?: string;
  bidderNo?: string;
  bidderName?: string;
  email?: string;
  bidAmount: number;
  deliveryPeriodDays: number;
  submissionDate: string;
  passedMandatory: boolean;
  technicalScore: number;
  financialScore: number;
  totalScore: number;
  rank: number;
  awarded: boolean;
  remarks?: string;
}

export interface CreateUpdateTenderDto {
  no?: string;
  title: string;
  tenderType: TenderType;
  requisitionNo?: string;
  procurementPlanCode?: string;
  procurementPlanItemNo?: string;
  category?: string;
  issueDate?: string;
  closingDate: string;
  closingTime?: string;
  validityDays?: number;
  tenderSecurityAmount?: number;
  lines?: CreateUpdateTenderLineDto[];
  committees?: CreateUpdateTenderCommitteeDto[];
  mandatoryRequirements?: CreateUpdateTenderMandatoryRequirementDto[];
}

export interface CreateUpdateTenderLineDto {
  id?: string;
  type: DocumentLineType;
  no: string;
  description: string;
  specifications?: string;
  quantity: number;
  estimatedUnitCost: number;
  unitOfMeasure?: string;
}

export interface CreateUpdateTenderCommitteeDto {
  id?: string;
  committeeType: TenderCommitteeType;
  staffNo: string;
  staffName: string;
  role: TenderCommitteeRole;
  appointmentDate?: string;
}

export interface CreateUpdateTenderMandatoryRequirementDto {
  id?: string;
  requirementDescription: string;
  isMandatory: boolean;
}

export interface SubmitTenderBidInput {
  bidderNo: string;
  bidderName: string;
  email?: string;
  bidAmount: number;
  deliveryPeriodDays?: number;
  passedMandatory?: boolean;
  remarks?: string;
}

export interface EvaluateTenderBidInput {
  bidId: string;
  passedMandatory: boolean;
  technicalScore: number;
  financialScore: number;
  rank: number;
  remarks?: string;
}

export interface AwardTenderInput {
  vendorNo: string;
  vendorName: string;
  awardAmount: number;
  awardDate?: string;
  generatePurchaseOrder?: boolean;
}

export interface GetTenderListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  tenderType?: TenderType;
  status?: TenderStatus;
}

import { Injectable, inject } from '@angular/core';
import {
  CreateUpdateProcurementPlanDto,
  CreateUpdatePurchaseRequisitionDto,
  CreateUpdateTenderDto,
  ProcurementPlanDto,
  ProcurementPlanService,
  ProcurementPlanStatus,
  PurchaseRequisitionDto,
  PurchaseRequisitionService,
  RequisitionStatus,
  RequisitionType,
  requisitionTypeOptions,
  TenderHeaderDto,
  TenderService,
  TenderStatus,
  TenderType,
  tenderTypeOptions,
} from '@proxy/procurement';
import { RecordEntity, RecordField } from '../erp-shared';
import { enumOptions } from './entity-helpers';

const PLAN_PERMISSION = 'Erp.ProcurementPlans';
const REQ_PERMISSION = 'Erp.PurchaseRequisitions';
const TENDER_PERMISSION = 'Erp.Tenders';

const today = () => new Date().toISOString().substring(0, 10);

function figure(field: string, labelKey: string, section: string): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

@Injectable({ providedIn: 'root' })
export class ProcurementEntities {
  private readonly plans = inject(ProcurementPlanService);
  private readonly requisitions = inject(PurchaseRequisitionService);
  private readonly tenders = inject(TenderService);

  readonly procurementPlan: RecordEntity<ProcurementPlanDto, CreateUpdateProcurementPlanDto> = {
    key: 'procurementPlan',
    titleKey: 'Erp::ProcurementPlan',
    pluralKey: 'Erp::ProcurementPlans',
    icon: 'fas fa-clipboard-list',
    permission: PLAN_PERMISSION,
    listRoute: ['/erp/procurement-plans'],
    attachmentEntityType: 'ProcurementPlan',
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 140 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'budgetName', labelKey: 'Erp::BudgetName', width: 140 },
      {
        field: 'status',
        labelKey: 'Erp::Status',
        type: 'badge',
        format: val =>
          val === ProcurementPlanStatus.Approved
            ? 'Approved'
            : val === ProcurementPlanStatus.Closed
            ? 'Closed'
            : val === ProcurementPlanStatus.PendingApproval
            ? 'Pending Approval'
            : 'Open',
      },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
      { field: 'totalEstimatedAmount', labelKey: 'Erp::TotalEstimatedAmount', type: 'currency' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'schedule', labelKey: 'Erp::Schedule' },
    ],
    fields: [
      { field: 'code', labelKey: 'Erp::Code', type: 'text', maxLength: 30, required: true, createOnly: true },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 200, required: true, wide: true },
      { field: 'budgetName', labelKey: 'Erp::BudgetName', type: 'text', maxLength: 50 },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', section: 'schedule' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date', section: 'schedule' },
      figure('totalEstimatedAmount', 'Erp::TotalEstimatedAmount', 'general'),
    ],
    getList: query => this.plans.getList(query),
    get: id => this.plans.get(id),
    create: input => this.plans.create(input),
    update: (id, input) => this.plans.update(id, input),
    delete: id => this.plans.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ code: '', description: '', startDate: today() }),
    facts: dto => [
      {
        labelKey: 'Erp::Status',
        value:
          dto.status === ProcurementPlanStatus.Approved
            ? 'Approved'
            : dto.status === ProcurementPlanStatus.Closed
            ? 'Closed'
            : dto.status === ProcurementPlanStatus.PendingApproval
            ? 'Pending Approval'
            : 'Open',
      },
      { labelKey: 'Erp::TotalEstimatedAmount', value: dto.totalEstimatedAmount, type: 'currency' },
      { labelKey: 'Erp::BudgetName', value: dto.budgetName },
    ],
    actions: [
      {
        key: 'approve',
        labelKey: 'Erp::Approve',
        icon: 'fas fa-check',
        permission: `${PLAN_PERMISSION}.Approve`,
        visible: dto => dto.status === ProcurementPlanStatus.Open,
        run: dto => this.plans.approve(dto.id!),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${PLAN_PERMISSION}.Update`,
        visible: dto => dto.status === ProcurementPlanStatus.Approved,
        run: dto => this.plans.reopen(dto.id!),
      },
      {
        key: 'close',
        labelKey: 'Erp::Close',
        icon: 'fas fa-archive',
        permission: `${PLAN_PERMISSION}.Update`,
        visible: dto => dto.status === ProcurementPlanStatus.Approved,
        run: dto => this.plans.close(dto.id!),
      },
    ],
  };

  readonly purchaseRequisition: RecordEntity<PurchaseRequisitionDto, CreateUpdatePurchaseRequisitionDto> = {
    key: 'purchaseRequisition',
    titleKey: 'Erp::PurchaseRequisition',
    pluralKey: 'Erp::PurchaseRequisitions',
    icon: 'fas fa-file-lines',
    permission: REQ_PERMISSION,
    listRoute: ['/erp/purchase-requisitions'],
    attachmentEntityType: 'PurchaseRequisition',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 140 },
      {
        field: 'requestType',
        labelKey: 'Erp::RequestType',
        type: 'select',
        options: enumOptions(requisitionTypeOptions, 'RequisitionType'),
      },
      { field: 'requestedBy', labelKey: 'Erp::RequestedBy' },
      { field: 'departmentCode', labelKey: 'Erp::Department' },
      { field: 'requestDate', labelKey: 'Erp::RequestDate', type: 'date' },
      {
        field: 'status',
        labelKey: 'Erp::Status',
        type: 'badge',
        format: val =>
          val === RequisitionStatus.Approved
            ? 'Approved'
            : val === RequisitionStatus.Ordered
            ? 'Ordered'
            : val === RequisitionStatus.Closed
            ? 'Closed'
            : val === RequisitionStatus.Rejected
            ? 'Rejected'
            : val === RequisitionStatus.PendingApproval
            ? 'Pending Approval'
            : 'Open',
      },
      { field: 'totalEstimatedCost', labelKey: 'Erp::TotalEstimatedCost', type: 'currency' },
      { field: 'purchaseOrderNo', labelKey: 'Erp::PurchaseOrderNo' },
      { field: 'tenderNo', labelKey: 'Erp::TenderNo' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'sourcing', labelKey: 'Erp::Sourcing' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 30, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      {
        field: 'requestType',
        labelKey: 'Erp::RequestType',
        type: 'select',
        options: enumOptions(requisitionTypeOptions, 'RequisitionType'),
        required: true,
      },
      { field: 'requestedBy', labelKey: 'Erp::RequestedBy', type: 'text', maxLength: 50 },
      { field: 'departmentCode', labelKey: 'Erp::Department', type: 'text', maxLength: 30 },
      { field: 'procurementPlanCode', labelKey: 'Erp::ProcurementPlanCode', type: 'text', maxLength: 30 },
      { field: 'requestDate', labelKey: 'Erp::RequestDate', type: 'date', required: true },
      { field: 'requiredDate', labelKey: 'Erp::RequiredDate', type: 'date' },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      figure('purchaseOrderNo', 'Erp::PurchaseOrderNo', 'sourcing'),
      figure('tenderNo', 'Erp::TenderNo', 'sourcing'),
      figure('totalEstimatedCost', 'Erp::TotalEstimatedCost', 'general'),
    ],
    getList: query => this.requisitions.getList(query),
    get: id => this.requisitions.get(id),
    create: input => this.requisitions.create(input),
    update: (id, input) => this.requisitions.update(id, input),
    delete: id => this.requisitions.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ requestType: RequisitionType.InternalRequest, requestDate: today() }),
    facts: dto => [
      {
        labelKey: 'Erp::Status',
        value:
          dto.status === RequisitionStatus.Approved
            ? 'Approved'
            : dto.status === RequisitionStatus.Ordered
            ? 'Ordered'
            : dto.status === RequisitionStatus.Rejected
            ? 'Rejected'
            : dto.status === RequisitionStatus.PendingApproval
            ? 'Pending Approval'
            : 'Open',
      },
      { labelKey: 'Erp::TotalEstimatedCost', value: dto.totalEstimatedCost, type: 'currency' },
      { labelKey: 'Erp::PurchaseOrderNo', value: dto.purchaseOrderNo },
      { labelKey: 'Erp::TenderNo', value: dto.tenderNo },
    ],
    actions: [
      {
        key: 'sendApproval',
        labelKey: 'Erp::SendApprovalRequest',
        icon: 'fas fa-share',
        permission: `${REQ_PERMISSION}.Update`,
        visible: dto => dto.status === RequisitionStatus.Open,
        run: dto => this.requisitions.sendApprovalRequest(dto.id!),
      },
      {
        key: 'cancelApproval',
        labelKey: 'Erp::CancelApprovalRequest',
        icon: 'fas fa-rotate-left',
        permission: `${REQ_PERMISSION}.Update`,
        visible: dto => dto.status === RequisitionStatus.PendingApproval,
        run: dto => this.requisitions.cancelApprovalRequest(dto.id!),
      },
      {
        key: 'release',
        labelKey: 'Erp::Release',
        icon: 'fas fa-lock',
        permission: `${REQ_PERMISSION}.Release`,
        visible: dto => dto.status === RequisitionStatus.Open,
        run: dto => this.requisitions.release(dto.id!),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${REQ_PERMISSION}.Update`,
        visible: dto => dto.status === RequisitionStatus.Approved,
        run: dto => this.requisitions.reopen(dto.id!),
      },
      {
        key: 'close',
        labelKey: 'Erp::Close',
        icon: 'fas fa-archive',
        permission: `${REQ_PERMISSION}.Update`,
        visible: dto => dto.status === RequisitionStatus.Approved || dto.status === RequisitionStatus.Ordered,
        run: dto => this.requisitions.close(dto.id!),
      },
    ],
  };

  readonly tender: RecordEntity<TenderHeaderDto, CreateUpdateTenderDto> = {
    key: 'tender',
    titleKey: 'Erp::Tender',
    pluralKey: 'Erp::Tenders',
    icon: 'fas fa-gavel',
    permission: TENDER_PERMISSION,
    listRoute: ['/erp/tenders'],
    attachmentEntityType: 'TenderHeader',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 140 },
      { field: 'title', labelKey: 'Erp::Title' },
      {
        field: 'tenderType',
        labelKey: 'Erp::TenderType',
        type: 'select',
        options: enumOptions(tenderTypeOptions, 'TenderType'),
      },
      {
        field: 'status',
        labelKey: 'Erp::Status',
        type: 'badge',
        format: val =>
          val === TenderStatus.Published
            ? 'Published'
            : val === TenderStatus.Opening
            ? 'Opening'
            : val === TenderStatus.UnderEvaluation
            ? 'Under Evaluation'
            : val === TenderStatus.Awarded
            ? 'Awarded'
            : val === TenderStatus.Cancelled
            ? 'Cancelled'
            : 'Draft',
      },
      { field: 'issueDate', labelKey: 'Erp::IssueDate', type: 'date' },
      { field: 'closingDate', labelKey: 'Erp::ClosingDate', type: 'date' },
      { field: 'awardedVendorName', labelKey: 'Erp::AwardedVendorName' },
      { field: 'awardedAmount', labelKey: 'Erp::AwardedAmount', type: 'currency' },
      { field: 'purchaseOrderNo', labelKey: 'Erp::PurchaseOrderNo' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'schedule', labelKey: 'Erp::Schedule' },
      { key: 'award', labelKey: 'Erp::Award' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 30, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      { field: 'title', labelKey: 'Erp::Title', type: 'text', maxLength: 250, required: true, wide: true },
      {
        field: 'tenderType',
        labelKey: 'Erp::TenderType',
        type: 'select',
        options: enumOptions(tenderTypeOptions, 'TenderType'),
        required: true,
      },
      { field: 'category', labelKey: 'Erp::Category', type: 'text', maxLength: 50 },
      { field: 'requisitionNo', labelKey: 'Erp::RequisitionNo', type: 'text', maxLength: 30 },
      { field: 'procurementPlanCode', labelKey: 'Erp::ProcurementPlanCode', type: 'text', maxLength: 30 },
      { field: 'issueDate', labelKey: 'Erp::IssueDate', type: 'date', section: 'schedule' },
      { field: 'closingDate', labelKey: 'Erp::ClosingDate', type: 'date', section: 'schedule', required: true },
      { field: 'closingTime', labelKey: 'Erp::ClosingTime', type: 'text', maxLength: 20, section: 'schedule' },
      { field: 'validityDays', labelKey: 'Erp::ValidityDays', type: 'number', section: 'schedule' },
      { field: 'tenderSecurityAmount', labelKey: 'Erp::TenderSecurityAmount', type: 'number', section: 'schedule' },
      figure('awardedVendorNo', 'Erp::AwardedVendorNo', 'award'),
      figure('awardedVendorName', 'Erp::AwardedVendorName', 'award'),
      figure('awardedAmount', 'Erp::AwardedAmount', 'award'),
      figure('purchaseOrderNo', 'Erp::PurchaseOrderNo', 'award'),
      figure('totalEstimatedAmount', 'Erp::TotalEstimatedAmount', 'general'),
    ],
    getList: query => this.tenders.getList(query),
    get: id => this.tenders.get(id),
    create: input => this.tenders.create(input),
    update: (id, input) => this.tenders.update(id, input),
    delete: id => this.tenders.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.title ?? undefined }),
    newRecord: () => ({
      title: '',
      tenderType: TenderType.Open,
      issueDate: today(),
      closingDate: today(),
      closingTime: '10:00 AM',
      validityDays: 90,
    }),
    facts: dto => [
      {
        labelKey: 'Erp::Status',
        value:
          dto.status === TenderStatus.Published
            ? 'Published'
            : dto.status === TenderStatus.Opening
            ? 'Opening'
            : dto.status === TenderStatus.UnderEvaluation
            ? 'Under Evaluation'
            : dto.status === TenderStatus.Awarded
            ? 'Awarded'
            : dto.status === TenderStatus.Cancelled
            ? 'Cancelled'
            : 'Draft',
      },
      { labelKey: 'Erp::TotalEstimatedAmount', value: dto.totalEstimatedAmount, type: 'currency' },
      { labelKey: 'Erp::AwardedVendorName', value: dto.awardedVendorName },
      { labelKey: 'Erp::AwardedAmount', value: dto.awardedAmount, type: 'currency' },
      { labelKey: 'Erp::PurchaseOrderNo', value: dto.purchaseOrderNo },
    ],
    actions: [
      {
        key: 'publish',
        labelKey: 'Erp::Publish',
        icon: 'fas fa-bullhorn',
        permission: `${TENDER_PERMISSION}.Release`,
        visible: dto => dto.status === TenderStatus.Draft,
        run: dto => this.tenders.publish(dto.id!),
      },
      {
        key: 'opening',
        labelKey: 'Erp::MoveToOpening',
        icon: 'fas fa-door-open',
        permission: `${TENDER_PERMISSION}.Update`,
        visible: dto => dto.status === TenderStatus.Published,
        run: dto => this.tenders.moveToOpening(dto.id!),
      },
      {
        key: 'evaluation',
        labelKey: 'Erp::MoveToEvaluation',
        icon: 'fas fa-calculator',
        permission: `${TENDER_PERMISSION}.Evaluate`,
        visible: dto => dto.status === TenderStatus.Opening,
        run: dto => this.tenders.moveToEvaluation(dto.id!),
      },
      {
        key: 'cancel',
        labelKey: 'Erp::Cancel',
        icon: 'fas fa-ban',
        permission: `${TENDER_PERMISSION}.Update`,
        visible: dto => dto.status !== TenderStatus.Awarded && dto.status !== TenderStatus.Cancelled,
        confirmKey: 'Erp::CancelTenderConfirmation',
        run: dto => this.tenders.cancelTender(dto.id!),
      },
    ],
  };

  get all(): RecordEntity[] {
    return [this.procurementPlan, this.purchaseRequisition, this.tender];
  }
}

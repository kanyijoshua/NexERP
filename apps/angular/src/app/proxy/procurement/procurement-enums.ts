export enum ProcurementType {
  Goods = 1,
  Works = 2,
  Services = 3,
  Consulting = 4,
}

export const procurementTypeOptions = [
  { value: ProcurementType.Goods, key: 'Erp::ProcurementType:Goods' },
  { value: ProcurementType.Works, key: 'Erp::ProcurementType:Works' },
  { value: ProcurementType.Services, key: 'Erp::ProcurementType:Services' },
  { value: ProcurementType.Consulting, key: 'Erp::ProcurementType:Consulting' },
];

export enum ProcurementMethod {
  OpenTender = 1,
  RestrictedTender = 2,
  DirectProcurement = 3,
  RequestForQuotations = 4,
  LowValueProcurement = 5,
  FrameworkAgreement = 6,
}

export const procurementMethodOptions = [
  { value: ProcurementMethod.OpenTender, key: 'Erp::ProcurementMethod:OpenTender' },
  { value: ProcurementMethod.RestrictedTender, key: 'Erp::ProcurementMethod:RestrictedTender' },
  { value: ProcurementMethod.DirectProcurement, key: 'Erp::ProcurementMethod:DirectProcurement' },
  { value: ProcurementMethod.RequestForQuotations, key: 'Erp::ProcurementMethod:RequestForQuotations' },
  { value: ProcurementMethod.LowValueProcurement, key: 'Erp::ProcurementMethod:LowValueProcurement' },
  { value: ProcurementMethod.FrameworkAgreement, key: 'Erp::ProcurementMethod:FrameworkAgreement' },
];

export enum ProcurementPlanStatus {
  Open = 0,
  PendingApproval = 1,
  Approved = 2,
  Closed = 3,
}

export enum RequisitionType {
  InternalRequest = 1,
  ProjectRequisition = 2,
  ServiceOrder = 3,
  DirectPurchase = 4,
}

export const requisitionTypeOptions = [
  { value: RequisitionType.InternalRequest, key: 'Erp::RequisitionType:InternalRequest' },
  { value: RequisitionType.ProjectRequisition, key: 'Erp::RequisitionType:ProjectRequisition' },
  { value: RequisitionType.ServiceOrder, key: 'Erp::RequisitionType:ServiceOrder' },
  { value: RequisitionType.DirectPurchase, key: 'Erp::RequisitionType:DirectPurchase' },
];

export enum RequisitionStatus {
  Open = 0,
  PendingApproval = 1,
  Approved = 2,
  Rejected = 3,
  Ordered = 4,
  Closed = 5,
}

export enum TenderType {
  Open = 1,
  Restricted = 2,
  Rfq = 3,
  Eoi = 4,
}

export const tenderTypeOptions = [
  { value: TenderType.Open, key: 'Erp::TenderType:Open' },
  { value: TenderType.Restricted, key: 'Erp::TenderType:Restricted' },
  { value: TenderType.Rfq, key: 'Erp::TenderType:Rfq' },
  { value: TenderType.Eoi, key: 'Erp::TenderType:Eoi' },
];

export enum TenderStatus {
  Draft = 0,
  Published = 1,
  Opening = 2,
  UnderEvaluation = 3,
  Awarded = 4,
  Cancelled = 5,
}

export enum TenderCommitteeType {
  Opening = 1,
  Evaluation = 2,
  Negotiation = 3,
  InspectionAndAcceptance = 4,
}

export enum TenderCommitteeRole {
  Chairperson = 1,
  Secretary = 2,
  Member = 3,
}

export enum PurchaseOrderStage {
  Open = 0,
  PendingApproval = 1,
  Released = 2,
  PartiallyReceived = 3,
  Received = 4,
  Invoiced = 5,
  Closed = 6,
  Cancelled = 7,
}

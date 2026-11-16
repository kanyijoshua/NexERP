/**
 * The standard reports a record's card and list offer under "Report", by record entity key. Each
 * report here filters on the record's number ("No. Filter"), so the card opens it for that record
 * alone. Which of them a user actually sees is decided by the server's report list (permissions
 * and enabled modules).
 */
export const RECORD_REPORTS: Readonly<Record<string, readonly string[]>> = {
  customer: ['CustomerDetailTrialBalance', 'CustomerTrialBalance', 'CustomerBalanceToDate', 'CustomerList'],
  vendor: [
    'VendorDetailTrialBalance',
    'VendorTrialBalance',
    'VendorBalanceToDate',
    'VendorOrderSummary',
    'PurchaseInvoiceRegister',
    'VendorItemCatalog',
    'VendorList',
  ],
  item: ['InventoryValuation', 'ItemAvailabilityByLocation', 'ItemTracing'],
  glAccount: ['TrialBalanceByAccount', 'TrialBalanceBudget', 'Budget'],
  bankAccount: ['BankAccountStatement', 'BankAccountDetailTrialBalance', 'BankAccountCheckDetails', 'CashBook'],
  fixedAsset: [
    'FixedAssetDetails',
    'FixedAssetBookValue',
    'FixedAssetProjectedValue',
    'FixedAssetsRegister',
    'MaintenanceRegister',
    'FixedAssetList',
  ],
  employee: [
    'Payslips',
    'PayrollSummary',
    'PayrollRegister',
    'PayrollDeductionSchedule',
    'PayrollNetPaySchedule',
    'EmployeeStaffAbsences',
    'EmployeeAbsencesByCauses',
    'EmployeeAddresses',
    'EmployeePhoneNos',
    'EmployeeBirthdays',
    'EmployeeContracts',
    'EmployeeQualifications',
    'EmployeeRelatives',
    'EmployeeUnions',
    'EmployeeMiscArticleInfo',
    'EmployeeConfidentialInfo',
    'EmployeeList',
  ],
  student: ['StudentTranscript', 'StudentFeeBalances', 'StudentList'],
  pensionMember: [
    'MemberStatement',
    'MemberContributionsStatement',
    'MemberBalances',
    'Beneficiaries',
    'ExitsWorksheet',
    'MembershipMovement',
    'MemberListing',
  ],
  pensioner: ['PensionerPayslip', 'PensionersMasterRoll'],
};

/** Reports of a record entity: its own list when it names one, else the shared map above. */
export function recordReportCodes(entity: { key: string; reports?: readonly string[] }): readonly string[] {
  return entity.reports ?? RECORD_REPORTS[entity.key] ?? [];
}

/** The query string that opens a standard report, run for one record number when given. */
export function standardReportQuery(code: string, no?: string | null): Record<string, string> {
  return no ? { report: code, no } : { report: code };
}

import { ABP, RoutesService, eLayoutType } from '@abp/ng.core';
import { APP_INITIALIZER } from '@angular/core';

/**
 * The module each ERP menu entry belongs to. Entries of a module this company has switched off are
 * left out of the menu; see `ErpModuleRoutesService`. Anything not named here belongs to the core
 * and is always shown.
 */
export const ERP_ROUTE_MODULE: Record<string, string> = {
  'Erp::Menu:ChartOfAccounts': 'Finance',
  'Erp::Menu:Journals': 'Finance',
  'Erp::Menu:SalesInvoices': 'Sales',
  'Erp::Menu:PurchaseInvoices': 'Purchasing',
  'Erp::Menu:Reports': 'Reporting',
  'Erp::Menu:RequestsToApprove': 'Approvals',
  'Erp::Menu:Workflows': 'Approvals',
  'Erp::Menu:ApprovalUserSetup': 'Approvals',
  'Erp::Menu:WebServices': 'Integration',
  'Erp::Menu:Webhooks': 'Integration',
  'Erp::Menu:DataExport': 'DataExport',
};

/** The ERP menu, defined once so it can be rebuilt when a module is switched on or off. */
export const ERP_ROUTES: ABP.Route[] = [
  {
    path: '/',
    name: '::Menu:Home',
    iconClass: 'fas fa-home',
    order: 1,
    layout: eLayoutType.application,
  },
  {
    path: '/erp',
    name: 'Erp::Menu:Erp',
    iconClass: 'fas fa-calculator',
    order: 2,
    layout: eLayoutType.application,
  },
  {
    path: '/erp/chart-of-accounts',
    name: 'Erp::Menu:ChartOfAccounts',
    parentName: 'Erp::Menu:Erp',
    iconClass: 'fas fa-list-ol',
    order: 2,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.GLAccounts',
  },
  {
    path: '/erp/sales-invoices',
    name: 'Erp::Menu:SalesInvoices',
    parentName: 'Erp::Menu:Erp',
    iconClass: 'fas fa-file-invoice-dollar',
    order: 3,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.SalesDocuments',
  },
  {
    path: '/erp/purchase-invoices',
    name: 'Erp::Menu:PurchaseInvoices',
    parentName: 'Erp::Menu:Erp',
    iconClass: 'fas fa-shopping-cart',
    order: 4,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.PurchaseDocuments',
  },
  {
    // A group only: it has no page of its own.
    name: 'Erp::Menu:Journals',
    parentName: 'Erp::Menu:Erp',
    iconClass: 'fas fa-book',
    order: 5,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.Journals || Erp.GLRegisters',
  },
  {
    path: '/erp/finance/general-journal',
    name: 'Erp::Menu:GeneralJournal',
    parentName: 'Erp::Menu:Journals',
    iconClass: 'fas fa-pen-to-square',
    order: 1,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.Journals',
  },
  {
    path: '/erp/finance/journal-templates',
    name: 'Erp::Menu:JournalTemplates',
    parentName: 'Erp::Menu:Journals',
    iconClass: 'fas fa-layer-group',
    order: 2,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.Journals',
  },
  {
    path: '/erp/finance/registers',
    name: 'Erp::Menu:GLRegisters',
    parentName: 'Erp::Menu:Journals',
    iconClass: 'fas fa-clock-rotate-left',
    order: 3,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.GLRegisters',
  },
  {
    name: 'Erp::Menu:Reports',
    parentName: 'Erp::Menu:Erp',
    iconClass: 'fas fa-chart-line',
    order: 6,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.Reports || Erp.AccountSchedules',
  },
  {
    path: '/erp/reports/financial',
    name: 'Erp::Menu:FinancialReports',
    parentName: 'Erp::Menu:Reports',
    iconClass: 'fas fa-file-invoice',
    order: 1,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.Reports',
  },
  {
    path: '/erp/reports/account-schedules',
    name: 'Erp::Menu:AccountSchedules',
    parentName: 'Erp::Menu:Reports',
    iconClass: 'fas fa-table',
    order: 2,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.AccountSchedules',
  },
  {
    path: '/erp/reports/column-layouts',
    name: 'Erp::Menu:ColumnLayouts',
    parentName: 'Erp::Menu:Reports',
    iconClass: 'fas fa-table-columns',
    order: 3,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.AccountSchedules',
  },
  {
    path: '/erp/reports/report-layouts',
    name: 'Erp::Menu:ReportLayouts',
    parentName: 'Erp::Menu:Reports',
    iconClass: 'fas fa-file-invoice',
    order: 4,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.ReportLayouts',
  },
  {
    path: '/erp/approvals',
    name: 'Erp::Menu:RequestsToApprove',
    parentName: 'Erp::Menu:Erp',
    iconClass: 'fas fa-clipboard-check',
    order: 7,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.Workflows',
  },
  {
    // A group only: it has no page of its own.
    name: 'Erp::Menu:Setup',
    parentName: 'Erp::Menu:Erp',
    iconClass: 'fas fa-cog',
    order: 90,
    layout: eLayoutType.application,
    requiredPolicy:
      'Erp.Modules || Erp.NoSeries || Erp.SalesSetup || Erp.PurchaseSetup || Erp.Workflows || Erp.ApprovalUserSetup || Erp.WebServices || Erp.Webhooks || Erp.DataExport',
  },
  {
    path: '/erp/setup/modules',
    name: 'Erp::Menu:Modules',
    parentName: 'Erp::Menu:Setup',
    iconClass: 'fas fa-sliders',
    order: 1,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.Modules',
  },
  {
    path: '/erp/setup/no-series',
    name: 'Erp::Menu:NoSeries',
    parentName: 'Erp::Menu:Setup',
    iconClass: 'fas fa-list-ol',
    order: 2,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.NoSeries',
  },
  {
    path: '/erp/setup/document-numbering',
    name: 'Erp::DocumentNumbering',
    parentName: 'Erp::Menu:Setup',
    iconClass: 'fas fa-hashtag',
    order: 3,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.NoSeries && (Erp.SalesSetup || Erp.PurchaseSetup)',
  },
  {
    path: '/erp/setup/workflows',
    name: 'Erp::Menu:Workflows',
    parentName: 'Erp::Menu:Setup',
    iconClass: 'fas fa-project-diagram',
    order: 4,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.Workflows.Manage',
  },
  {
    path: '/erp/setup/approval-users',
    name: 'Erp::Menu:ApprovalUserSetup',
    parentName: 'Erp::Menu:Setup',
    iconClass: 'fas fa-user-check',
    order: 5,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.ApprovalUserSetup',
  },
  {
    path: '/erp/setup/web-services',
    name: 'Erp::Menu:WebServices',
    parentName: 'Erp::Menu:Setup',
    iconClass: 'fas fa-plug',
    order: 6,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.WebServices && Erp.DataExport',
  },
  {
    path: '/erp/setup/webhooks',
    name: 'Erp::Menu:Webhooks',
    parentName: 'Erp::Menu:Setup',
    iconClass: 'fas fa-satellite-dish',
    order: 7,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.Webhooks && Erp.DataExport',
  },
  {
    path: '/erp/setup/data-export',
    name: 'Erp::Menu:DataExport',
    parentName: 'Erp::Menu:Setup',
    iconClass: 'fas fa-file-export',
    order: 8,
    layout: eLayoutType.application,
    requiredPolicy: 'Erp.DataExport',
  },
];

export const APP_ROUTE_PROVIDER = [
  { provide: APP_INITIALIZER, useFactory: configureRoutes, deps: [RoutesService], multi: true },
];

function configureRoutes(routesService: RoutesService) {
  return () => {
    routesService.add(ERP_ROUTES);
  };
}

import { RoutesService, eLayoutType } from '@abp/ng.core';
import { APP_INITIALIZER } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  { provide: APP_INITIALIZER, useFactory: configureRoutes, deps: [RoutesService], multi: true },
];

function configureRoutes(routesService: RoutesService) {
  return () => {
    routesService.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/erp',
        name: 'Business Central ERP',
        iconClass: 'fas fa-calculator',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: '/erp/dashboard',
        name: 'Role Center Dashboard',
        parentName: 'Business Central ERP',
        iconClass: 'fas fa-tachometer-alt',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/erp/chart-of-accounts',
        name: 'Chart of Accounts',
        parentName: 'Business Central ERP',
        iconClass: 'fas fa-list-ol',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: '/erp/sales-invoices',
        name: 'Sales Invoices',
        parentName: 'Business Central ERP',
        iconClass: 'fas fa-file-invoice-dollar',
        order: 3,
        layout: eLayoutType.application,
      },
      {
        path: '/erp/purchase-invoices',
        name: 'Purchase Invoices',
        parentName: 'Business Central ERP',
        iconClass: 'fas fa-shopping-cart',
        order: 4,
        layout: eLayoutType.application,
      },
      {
        path: '/erp/financial-reports',
        name: 'Financial Reports',
        parentName: 'Business Central ERP',
        iconClass: 'fas fa-chart-line',
        order: 5,
        layout: eLayoutType.application,
      },
      {
        path: '/erp/approvals',
        name: 'Erp::Menu:RequestsToApprove',
        parentName: 'Business Central ERP',
        iconClass: 'fas fa-clipboard-check',
        order: 6,
        layout: eLayoutType.application,
        requiredPolicy: 'Erp.Workflows',
      },
      {
        // A group only: it has no page of its own.
        name: 'Erp::Menu:Setup',
        parentName: 'Business Central ERP',
        iconClass: 'fas fa-cog',
        order: 90,
        layout: eLayoutType.application,
        requiredPolicy: 'Erp.NoSeries || Erp.SalesSetup || Erp.PurchaseSetup || Erp.Workflows || Erp.ApprovalUserSetup',
      },
      {
        path: '/erp/setup/no-series',
        name: 'Erp::Menu:NoSeries',
        parentName: 'Erp::Menu:Setup',
        iconClass: 'fas fa-list-ol',
        order: 1,
        layout: eLayoutType.application,
        requiredPolicy: 'Erp.NoSeries',
      },
      {
        path: '/erp/setup/document-numbering',
        name: 'Erp::DocumentNumbering',
        parentName: 'Erp::Menu:Setup',
        iconClass: 'fas fa-hashtag',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'Erp.NoSeries && (Erp.SalesSetup || Erp.PurchaseSetup)',
      },
      {
        path: '/erp/setup/workflows',
        name: 'Erp::Menu:Workflows',
        parentName: 'Erp::Menu:Setup',
        iconClass: 'fas fa-project-diagram',
        order: 3,
        layout: eLayoutType.application,
        requiredPolicy: 'Erp.Workflows.Manage',
      },
      {
        path: '/erp/setup/approval-users',
        name: 'Erp::Menu:ApprovalUserSetup',
        parentName: 'Erp::Menu:Setup',
        iconClass: 'fas fa-user-check',
        order: 4,
        layout: eLayoutType.application,
        requiredPolicy: 'Erp.ApprovalUserSetup',
      },
    ]);
  };
}

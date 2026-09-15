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
    ]);
  };
}

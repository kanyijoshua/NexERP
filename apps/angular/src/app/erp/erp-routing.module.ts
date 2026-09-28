import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { Route, RouterModule, Routes } from '@angular/router';
import { SalesInvoicesComponent } from './pages/sales-invoices/sales-invoices.component';
import { PurchaseInvoicesComponent } from './pages/purchase-invoices/purchase-invoices.component';
import { moduleGuard } from './services/module.guard';

function masterData(path: string, entity: string, requiredPolicy: string, module: string): Route {
  return {
    path,
    loadChildren: () => import('./master-data/master-data.module').then(m => m.MasterDataModule),
    canActivate: [permissionGuard, moduleGuard],
    // Component-less, so the list and card pages below inherit `entity`.
    data: { entity, requiredPolicy, module },
  };
}

const routes: Routes = [
  // The role centre is the application's landing page now, so /erp has nothing of its own.
  { path: '', redirectTo: '/', pathMatch: 'full' },
  { path: 'dashboard', redirectTo: '/', pathMatch: 'full' },
  // Master data: one lazy module serves the list and card pages of every table, see `MasterDataEntities`.
  masterData('chart-of-accounts', 'glAccount', 'Erp.GLAccounts', 'Finance'),
  masterData('customers', 'customer', 'Erp.Customers', 'Sales'),
  masterData('vendors', 'vendor', 'Erp.Vendors', 'Purchasing'),
  masterData('items', 'item', 'Erp.Items', 'Inventory'),
  masterData('units-of-measure', 'unitOfMeasure', 'Erp.UnitsOfMeasure', 'Inventory'),
  masterData('item-categories', 'itemCategory', 'Erp.ItemCategories', 'Inventory'),
  {
    path: 'sales-invoices',
    component: SalesInvoicesComponent,
    canActivate: [permissionGuard, moduleGuard],
    data: { requiredPolicy: 'Erp.SalesDocuments', module: 'Sales' },
  },
  {
    path: 'purchase-invoices',
    component: PurchaseInvoicesComponent,
    canActivate: [permissionGuard, moduleGuard],
    data: { requiredPolicy: 'Erp.PurchaseDocuments', module: 'Purchasing' },
  },
  // The old single-report page has been replaced by the report viewer under /reports.
  { path: 'financial-reports', redirectTo: 'reports/financial', pathMatch: 'full' },
  {
    path: 'approvals',
    loadChildren: () => import('./approvals/approvals.module').then(m => m.ApprovalsModule),
    canActivate: [permissionGuard, moduleGuard],
    data: { requiredPolicy: 'Erp.Workflows', module: 'Approvals' },
  },
  {
    path: 'finance',
    loadChildren: () => import('./finance/finance.module').then(m => m.FinanceModule),
  },
  {
    path: 'reports',
    loadChildren: () => import('./reports/reports.module').then(m => m.ReportsModule),
    canActivate: [moduleGuard],
    data: { module: 'Reporting' },
  },
  { path: 'setup', loadChildren: () => import('./setup/setup.module').then(m => m.SetupModule) },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ErpRoutingModule {}

import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ChartOfAccountsComponent } from './pages/chart-of-accounts/chart-of-accounts.component';
import { SalesInvoicesComponent } from './pages/sales-invoices/sales-invoices.component';
import { PurchaseInvoicesComponent } from './pages/purchase-invoices/purchase-invoices.component';
import { moduleGuard } from './services/module.guard';

const routes: Routes = [
  // The role centre is the application's landing page now, so /erp has nothing of its own.
  { path: '', redirectTo: '/', pathMatch: 'full' },
  { path: 'dashboard', redirectTo: '/', pathMatch: 'full' },
  {
    path: 'chart-of-accounts',
    component: ChartOfAccountsComponent,
    canActivate: [permissionGuard, moduleGuard],
    data: { requiredPolicy: 'Erp.GLAccounts', module: 'Finance' },
  },
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

import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ChartOfAccountsComponent } from './pages/chart-of-accounts/chart-of-accounts.component';
import { SalesInvoicesComponent } from './pages/sales-invoices/sales-invoices.component';
import { PurchaseInvoicesComponent } from './pages/purchase-invoices/purchase-invoices.component';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'chart-of-accounts', component: ChartOfAccountsComponent },
  { path: 'sales-invoices', component: SalesInvoicesComponent },
  { path: 'purchase-invoices', component: PurchaseInvoicesComponent },
  // The old single-report page has been replaced by the report viewer under /reports.
  { path: 'financial-reports', redirectTo: 'reports/financial', pathMatch: 'full' },
  {
    path: 'approvals',
    loadChildren: () => import('./approvals/approvals.module').then(m => m.ApprovalsModule),
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Workflows' },
  },
  {
    path: 'finance',
    loadChildren: () => import('./finance/finance.module').then(m => m.FinanceModule),
  },
  {
    path: 'reports',
    loadChildren: () => import('./reports/reports.module').then(m => m.ReportsModule),
  },
  { path: 'setup', loadChildren: () => import('./setup/setup.module').then(m => m.SetupModule) },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ErpRoutingModule {}

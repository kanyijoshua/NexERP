import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ChartOfAccountsComponent } from './pages/chart-of-accounts/chart-of-accounts.component';
import { SalesInvoicesComponent } from './pages/sales-invoices/sales-invoices.component';
import { PurchaseInvoicesComponent } from './pages/purchase-invoices/purchase-invoices.component';
import { FinancialReportsComponent } from './pages/financial-reports/financial-reports.component';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'chart-of-accounts', component: ChartOfAccountsComponent },
  { path: 'sales-invoices', component: SalesInvoicesComponent },
  { path: 'purchase-invoices', component: PurchaseInvoicesComponent },
  { path: 'financial-reports', component: FinancialReportsComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ErpRoutingModule {}

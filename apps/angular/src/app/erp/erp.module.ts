import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { SharedModule } from '../shared/shared.module';

import { ErpRoutingModule } from './erp-routing.module';
import { CompanyInterceptor } from './services/company.interceptor';

import { CompanySwitcherComponent } from './components/company-switcher/company-switcher.component';
import { ChatterWidgetComponent } from './components/chatter-widget/chatter-widget.component';

import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ChartOfAccountsComponent } from './pages/chart-of-accounts/chart-of-accounts.component';
import { SalesInvoicesComponent } from './pages/sales-invoices/sales-invoices.component';
import { PurchaseInvoicesComponent } from './pages/purchase-invoices/purchase-invoices.component';
import { FinancialReportsComponent } from './pages/financial-reports/financial-reports.component';

@NgModule({
  declarations: [
    CompanySwitcherComponent,
    ChatterWidgetComponent,
    DashboardComponent,
    ChartOfAccountsComponent,
    SalesInvoicesComponent,
    PurchaseInvoicesComponent,
    FinancialReportsComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    ErpRoutingModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CompanyInterceptor,
      multi: true,
    },
  ],
})
export class ErpModule {}

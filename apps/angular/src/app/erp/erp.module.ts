import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';

import { ErpRoutingModule } from './erp-routing.module';
import { ErpSharedModule } from './erp-shared/erp-shared.module';

import { ChartOfAccountsComponent } from './pages/chart-of-accounts/chart-of-accounts.component';
import { SalesInvoicesComponent } from './pages/sales-invoices/sales-invoices.component';
import { PurchaseInvoicesComponent } from './pages/purchase-invoices/purchase-invoices.component';

@NgModule({
  declarations: [
    ChartOfAccountsComponent,
    SalesInvoicesComponent,
    PurchaseInvoicesComponent,
  ],
  imports: [CommonModule, FormsModule, SharedModule, ErpSharedModule, ErpRoutingModule],
})
export class ErpModule {}

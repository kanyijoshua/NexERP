import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { GeneralJournalComponent } from './general-journal/general-journal.component';
import { GLRegistersComponent } from './gl-registers/gl-registers.component';
import { JournalTemplatesComponent } from './journal-templates/journal-templates.component';
import { AccountingPeriodsComponent } from './accounting-periods/accounting-periods.component';
import { BankLedgerEntriesComponent } from './bank-ledger-entries/bank-ledger-entries.component';
import { VatEntriesComponent } from './vat-entries/vat-entries.component';
import { PartyLedgerEntriesComponent } from './party-ledger-entries/party-ledger-entries.component';
import { ExchRateAdjustmentComponent } from './exch-rate-adjustment/exch-rate-adjustment.component';
import { VatReturnComponent } from './vat-return/vat-return.component';
import { VatSettlementComponent } from './vat-settlement/vat-settlement.component';

const routes: Routes = [
  { path: '', redirectTo: 'general-journal', pathMatch: 'full' },
  {
    path: 'general-journal',
    component: GeneralJournalComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Journals' },
  },
  {
    path: 'journal-templates',
    component: JournalTemplatesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Journals' },
  },
  {
    path: 'registers',
    component: GLRegistersComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.GLRegisters' },
  },
  {
    path: 'accounting-periods',
    component: AccountingPeriodsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.FinanceSetup' },
  },
  {
    path: 'vat-entries',
    component: VatEntriesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.VatEntries' },
  },
  {
    path: 'bank-ledger-entries',
    component: BankLedgerEntriesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.BankAccounts' },
  },
  {
    path: 'customer-ledger-entries',
    component: PartyLedgerEntriesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Customers', kind: 'customer' },
  },
  {
    path: 'vendor-ledger-entries',
    component: PartyLedgerEntriesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Vendors', kind: 'vendor' },
  },
  {
    path: 'employee-ledger-entries',
    component: PartyLedgerEntriesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Employees', kind: 'employee' },
  },
  {
    path: 'exch-rate-adjustment',
    component: ExchRateAdjustmentComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.PeriodicActivities' },
  },
  {
    path: 'vat-return',
    component: VatReturnComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.VatEntries' },
  },
  {
    path: 'vat-settlement',
    component: VatSettlementComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.PeriodicActivities.SettleVat' },
  },
];

@NgModule({
  declarations: [
    GeneralJournalComponent,
    JournalTemplatesComponent,
    GLRegistersComponent,
    AccountingPeriodsComponent,
    VatEntriesComponent,
    BankLedgerEntriesComponent,
    PartyLedgerEntriesComponent,
    ExchRateAdjustmentComponent,
    VatReturnComponent,
    VatSettlementComponent,
  ],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class FinanceModule {}

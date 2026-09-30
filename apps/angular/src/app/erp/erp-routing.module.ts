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
  // Posting groups and the setups that turn them into G/L accounts.
  masterData('gen-bus-posting-groups', 'genBusPostingGroup', 'Erp.PostingSetup', 'Finance'),
  masterData('gen-prod-posting-groups', 'genProdPostingGroup', 'Erp.PostingSetup', 'Finance'),
  masterData('customer-posting-groups', 'customerPostingGroup', 'Erp.PostingSetup', 'Sales'),
  masterData('vendor-posting-groups', 'vendorPostingGroup', 'Erp.PostingSetup', 'Purchasing'),
  masterData('inventory-posting-groups', 'inventoryPostingGroup', 'Erp.PostingSetup', 'Inventory'),
  masterData('general-posting-setup', 'generalPostingSetup', 'Erp.PostingSetup', 'Finance'),
  masterData('inventory-posting-setup', 'inventoryPostingSetup', 'Erp.PostingSetup', 'Inventory'),
  // Tax, finance, cash management, inventory, sales and human resources setup.
  masterData('vat-bus-posting-groups', 'vatBusPostingGroup', 'Erp.PostingSetup', 'Finance'),
  masterData('vat-prod-posting-groups', 'vatProdPostingGroup', 'Erp.PostingSetup', 'Finance'),
  masterData('vat-posting-setup', 'vatPostingSetup', 'Erp.PostingSetup', 'Finance'),
  masterData('payment-terms', 'paymentTerms', 'Erp.FinanceSetup', 'Finance'),
  masterData('currencies', 'currency', 'Erp.FinanceSetup', 'Finance'),
  masterData('currency-exchange-rates', 'currencyExchangeRate', 'Erp.FinanceSetup', 'Finance'),
  masterData('payment-methods', 'paymentMethod', 'Erp.FinanceSetup', 'Finance'),
  masterData('bank-account-posting-groups', 'bankAccountPostingGroup', 'Erp.PostingSetup', 'CashManagement'),
  masterData('bank-accounts', 'bankAccount', 'Erp.BankAccounts', 'CashManagement'),
  masterData('locations', 'location', 'Erp.Locations', 'Inventory'),
  masterData('salespeople-purchasers', 'salespersonPurchaser', 'Erp.SalespeoplePurchasers', 'Sales'),
  masterData('employees', 'employee', 'Erp.Employees', 'HumanResources'),
  masterData('employee-absences', 'employeeAbsence', 'Erp.Employees', 'HumanResources'),
  masterData('causes-of-absence', 'causeOfAbsence', 'Erp.HumanResourcesSetup', 'HumanResources'),
  masterData('qualifications', 'qualification', 'Erp.HumanResourcesSetup', 'HumanResources'),
  masterData('unions', 'union', 'Erp.HumanResourcesSetup', 'HumanResources'),
  masterData('employment-contracts', 'employmentContract', 'Erp.HumanResourcesSetup', 'HumanResources'),
  masterData('grounds-for-termination', 'groundsForTermination', 'Erp.HumanResourcesSetup', 'HumanResources'),
  masterData('hr-units-of-measure', 'hrUnitOfMeasure', 'Erp.HumanResourcesSetup', 'HumanResources'),
  masterData('employee-posting-groups', 'employeePostingGroup', 'Erp.HumanResourcesSetup', 'HumanResources'),
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
  {
    path: 'rapid-start',
    loadChildren: () => import('./rapid-start/rapid-start.module').then(m => m.RapidStartModule),
    canActivate: [moduleGuard],
    // Unknown to the module list until the backend registers it, which the guard treats as on.
    data: { module: 'RapidStart' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ErpRoutingModule {}

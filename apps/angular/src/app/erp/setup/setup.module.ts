import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { ApprovalUserSetupComponent } from './approval-user-setup/approval-user-setup.component';
import { DocumentSetupComponent } from './document-setup/document-setup.component';
import { NoSeriesComponent } from './no-series/no-series.component';
import { WorkflowsComponent } from './workflows/workflows.component';

const routes: Routes = [
  { path: '', redirectTo: 'no-series', pathMatch: 'full' },
  {
    path: 'no-series',
    component: NoSeriesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.NoSeries' },
  },
  {
    path: 'document-numbering',
    component: DocumentSetupComponent,
    canActivate: [permissionGuard],
    // The page lists the series to choose from, so it needs to read them too.
    data: { requiredPolicy: 'Erp.NoSeries && (Erp.SalesSetup || Erp.PurchaseSetup)' },
  },
  {
    path: 'workflows',
    component: WorkflowsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Workflows' },
  },
  {
    path: 'approval-users',
    component: ApprovalUserSetupComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.ApprovalUserSetup' },
  },
];

@NgModule({
  declarations: [NoSeriesComponent, DocumentSetupComponent, WorkflowsComponent, ApprovalUserSetupComponent],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class SetupModule {}

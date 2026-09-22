import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { ApprovalUserSetupComponent } from './approval-user-setup/approval-user-setup.component';
import { DataExportComponent } from './data-export/data-export.component';
import { DocumentSetupComponent } from './document-setup/document-setup.component';
import { ModulesComponent } from './modules/modules.component';
import { NoSeriesComponent } from './no-series/no-series.component';
import { WebServicesComponent } from './web-services/web-services.component';
import { WebhooksComponent } from './webhooks/webhooks.component';
import { WorkflowsComponent } from './workflows/workflows.component';

const routes: Routes = [
  { path: '', redirectTo: 'modules', pathMatch: 'full' },
  {
    path: 'modules',
    component: ModulesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Modules' },
  },
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
  {
    path: 'web-services',
    component: WebServicesComponent,
    canActivate: [permissionGuard],
    // The page lists the tables that may be published, which comes from the export service.
    data: { requiredPolicy: 'Erp.WebServices && Erp.DataExport' },
  },
  {
    path: 'webhooks',
    component: WebhooksComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Webhooks && Erp.DataExport' },
  },
  {
    path: 'data-export',
    component: DataExportComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.DataExport' },
  },
];

@NgModule({
  declarations: [
    ModulesComponent,
    NoSeriesComponent,
    DocumentSetupComponent,
    WorkflowsComponent,
    ApprovalUserSetupComponent,
    WebServicesComponent,
    WebhooksComponent,
    DataExportComponent,
  ],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class SetupModule {}

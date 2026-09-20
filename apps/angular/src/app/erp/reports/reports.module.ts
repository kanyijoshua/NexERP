import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { AccountSchedulesComponent } from './account-schedules/account-schedules.component';
import { ColumnLayoutsComponent } from './column-layouts/column-layouts.component';
import { ReportLayoutsComponent } from './report-layouts/report-layouts.component';
import { ReportViewerComponent } from './report-viewer/report-viewer.component';

const routes: Routes = [
  { path: '', redirectTo: 'financial', pathMatch: 'full' },
  {
    path: 'financial',
    component: ReportViewerComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Reports' },
  },
  {
    path: 'account-schedules',
    component: AccountSchedulesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.AccountSchedules' },
  },
  {
    path: 'column-layouts',
    component: ColumnLayoutsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.AccountSchedules' },
  },
  {
    path: 'report-layouts',
    component: ReportLayoutsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.ReportLayouts' },
  },
];

@NgModule({
  declarations: [
    ReportViewerComponent,
    AccountSchedulesComponent,
    ColumnLayoutsComponent,
    ReportLayoutsComponent,
  ],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class ReportsModule {}

import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { MyActivitiesComponent } from './my-activities/my-activities.component';
import { CrmPipelineComponent } from './pipeline/crm-pipeline.component';

const routes: Routes = [
  { path: '', redirectTo: 'pipeline', pathMatch: 'full' },
  { path: 'pipeline', component: CrmPipelineComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'Erp.Crm' } },
  { path: 'my-activities', component: MyActivitiesComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'Erp.Chatter' } },
];

/** The pipeline boards and the activities to do. */
@NgModule({
  declarations: [CrmPipelineComponent, MyActivitiesComponent],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class CrmModule {}

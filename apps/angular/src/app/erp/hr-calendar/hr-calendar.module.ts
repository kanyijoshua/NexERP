import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { WorkingCalendarComponent } from './working-calendar.component';

const routes: Routes = [
  { path: '', component: WorkingCalendarComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'Erp.HumanResourcesSetup' } },
];

/** The working calendar: a month of a base calendar, its nonworking days, and leave dates. */
@NgModule({
  declarations: [WorkingCalendarComponent],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class HrCalendarModule {}

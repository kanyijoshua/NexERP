import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { ProjectStatisticsComponent } from './project-statistics/project-statistics.component';
import { TimeSheetComponent } from './time-sheet/time-sheet.component';

const routes: Routes = [
  { path: 'time-sheet/:id', component: TimeSheetComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'Erp.TimeSheets' } },
  { path: 'statistics/:id', component: ProjectStatisticsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'Erp.Projects' } },
];

/** The time sheet week and the budget against actual of a project. */
@NgModule({
  declarations: [TimeSheetComponent, ProjectStatisticsComponent],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class ProjectWorkModule {}

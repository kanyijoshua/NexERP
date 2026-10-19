import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { ServiceBoardComponent } from './service-board/service-board.component';

const routes: Routes = [
  { path: '', redirectTo: 'board', pathMatch: 'full' },
  { path: 'board', component: ServiceBoardComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'Erp.Service' } },
];

/** The service order board. */
@NgModule({
  declarations: [ServiceBoardComponent],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class ServiceModule {}

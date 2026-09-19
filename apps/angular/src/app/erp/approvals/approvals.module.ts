import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { RequestsToApproveComponent } from './requests-to-approve.component';

const routes: Routes = [{ path: '', component: RequestsToApproveComponent }];

@NgModule({
  declarations: [RequestsToApproveComponent],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class ApprovalsModule {}

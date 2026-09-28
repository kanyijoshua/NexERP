import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { RecordCardComponent } from './record-card/record-card.component';
import { RecordListComponent } from './record-list/record-list.component';

/**
 * List and card pages of the master-data tables. Loaded once per table by `ErpRoutingModule`, whose
 * route supplies `data.entity`, the key of the table's `RecordEntity`.
 */
const routes: Routes = [
  { path: '', component: RecordListComponent },
  { path: ':id', component: RecordCardComponent },
];

@NgModule({
  declarations: [RecordListComponent, RecordCardComponent],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class MasterDataModule {}

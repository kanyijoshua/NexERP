import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';

import { ErpRoutingModule } from './erp-routing.module';
import { ErpSharedModule } from './erp-shared/erp-shared.module';
import { RecordEntityRegistry } from './erp-shared/record/record-entity';
import { MasterDataEntities } from './master-data/master-data-entities';

import { SalesDocumentsComponent } from './pages/sales-documents/sales-documents.component';
import { PurchaseDocumentsComponent } from './pages/purchase-documents/purchase-documents.component';

@NgModule({
  declarations: [
    SalesDocumentsComponent,
    PurchaseDocumentsComponent,
  ],
  imports: [CommonModule, FormsModule, SharedModule, ErpSharedModule, ErpRoutingModule],
})

export class ErpModule {
  // Every ERP page is below this module, so registering here makes the tables known to any lookup.
  constructor(registry: RecordEntityRegistry, entities: MasterDataEntities) {
    registry.register(...entities.all);
  }
}

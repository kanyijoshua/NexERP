import { PageModule } from '@abp/ng.components/page';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbDropdownModule, NgbNavModule, NgbTypeaheadModule } from '@ng-bootstrap/ng-bootstrap';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { SharedModule } from '../../shared/shared.module';

import { AttachmentsWidgetComponent } from '../components/attachments-widget/attachments-widget.component';
import { SpreadsheetDialogComponent } from './spreadsheet/spreadsheet-dialog.component';
import { ChatterWidgetComponent } from '../components/chatter-widget/chatter-widget.component';
import { DocumentPageComponent } from './document-page/document-page.component';
import { JournalPageComponent } from './journal-page/journal-page.component';
import { KanbanBoardComponent } from './kanban-board/kanban-board.component';
import { LineGridComponent } from './line-grid/line-grid.component';
import { LookupComponent } from './lookup/lookup.component';
import { PageToolbarComponent } from './page-toolbar/page-toolbar.component';
import { RecordCardDialogComponent } from './record/record-card-dialog.component';
import { RecordFormComponent } from './record/record-form.component';
import { RecordListDialogComponent } from './record/record-list-dialog.component';
import { RecordPartComponent } from './record/record-part.component';
import { ErpAmountPipe } from './pipes/erp-amount.pipe';
import { ReportPageComponent } from './report-page/report-page.component';
import { SmartButtonsComponent } from './smart-buttons/smart-buttons.component';

import { ErpCardComponent } from './card/erp-card.component';
import { ErpFactBoxComponent } from './card/erp-factbox.component';
import { ErpFastTabComponent } from './card/erp-fasttab.component';
import { ErpTableActionsDirective, ErpTableCardDirective, ErpTableColDirective } from './table/erp-table-col.directive';
import { ErpTableComponent } from './table/erp-table.component';
import { ErpDropdownComponent } from './dropdown/erp-dropdown.component';
import { ErpActionDropdownComponent } from './dropdown/erp-action-dropdown.component';
import { ErpButtonComponent } from './button/erp-button.component';
import { ErpSplitButtonComponent } from './button/erp-split-button.component';
import { ErpButtonGroupComponent } from './button/erp-button-group.component';
import { ErpDynamicTableComponent } from './dynamic-table/erp-dynamic-table.component';
import { ErpBadgeComponent } from './badge/erp-badge.component';

const DECLARATIONS = [
  // On every ERP page, so every lazy ERP module needs it.
  ChatterWidgetComponent,
  AttachmentsWidgetComponent,
  SpreadsheetDialogComponent,
  PageToolbarComponent,
  LookupComponent,
  LineGridComponent,
  DocumentPageComponent,
  JournalPageComponent,
  KanbanBoardComponent,
  ReportPageComponent,
  SmartButtonsComponent,
  RecordFormComponent,
  RecordCardDialogComponent,
  RecordListDialogComponent,
  RecordPartComponent,
  ErpTableComponent,
  ErpTableColDirective,
  ErpTableActionsDirective,
  ErpTableCardDirective,
  ErpCardComponent,
  ErpFastTabComponent,
  ErpFactBoxComponent,
  ErpAmountPipe,
  // Dynamic Reusable Components
  ErpDropdownComponent,
  ErpActionDropdownComponent,
  ErpButtonComponent,
  ErpSplitButtonComponent,
  ErpButtonGroupComponent,
  ErpDynamicTableComponent,
  ErpBadgeComponent,
];

const MODULES = [
  SharedModule,
  CommonModule,
  FormsModule,
  ReactiveFormsModule,
  NgxDatatableModule,
  PageModule,
  NgbNavModule,
  NgbTypeaheadModule,
  NgbDropdownModule,
  DragDropModule,
];

/**
 * Generic building blocks of the ERP front end, free of generated proxies. Import it in every lazy ERP
 * feature module. `CrudListBase` and the pure helpers are exported from `./index`.
 */
@NgModule({
  declarations: DECLARATIONS,
  imports: MODULES,
  exports: [...MODULES, ...DECLARATIONS],
})
export class ErpSharedModule {}

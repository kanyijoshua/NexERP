import { PageModule } from '@abp/ng.components/page';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbDropdownModule, NgbNavModule, NgbTypeaheadModule } from '@ng-bootstrap/ng-bootstrap';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { SharedModule } from '../../shared/shared.module';

import { ChatterWidgetComponent } from '../components/chatter-widget/chatter-widget.component';
import { CompanySwitcherComponent } from '../components/company-switcher/company-switcher.component';
import { DocumentPageComponent } from './document-page/document-page.component';
import { JournalPageComponent } from './journal-page/journal-page.component';
import { KanbanBoardComponent } from './kanban-board/kanban-board.component';
import { LineGridComponent } from './line-grid/line-grid.component';
import { LookupComponent } from './lookup/lookup.component';
import { PageToolbarComponent } from './page-toolbar/page-toolbar.component';
import { ErpAmountPipe } from './pipes/erp-amount.pipe';
import { ReportPageComponent } from './report-page/report-page.component';
import { SmartButtonsComponent } from './smart-buttons/smart-buttons.component';

const DECLARATIONS = [
  // On every ERP page, so every lazy ERP module needs it.
  CompanySwitcherComponent,
  ChatterWidgetComponent,
  PageToolbarComponent,
  LookupComponent,
  LineGridComponent,
  DocumentPageComponent,
  JournalPageComponent,
  KanbanBoardComponent,
  ReportPageComponent,
  SmartButtonsComponent,
  ErpAmountPipe,
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

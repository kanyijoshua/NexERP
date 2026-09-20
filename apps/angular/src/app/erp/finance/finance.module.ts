import { permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { GeneralJournalComponent } from './general-journal/general-journal.component';
import { GLRegistersComponent } from './gl-registers/gl-registers.component';
import { JournalTemplatesComponent } from './journal-templates/journal-templates.component';

const routes: Routes = [
  { path: '', redirectTo: 'general-journal', pathMatch: 'full' },
  {
    path: 'general-journal',
    component: GeneralJournalComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Journals' },
  },
  {
    path: 'journal-templates',
    component: JournalTemplatesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.Journals' },
  },
  {
    path: 'registers',
    component: GLRegistersComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.GLRegisters' },
  },
];

@NgModule({
  declarations: [GeneralJournalComponent, JournalTemplatesComponent, GLRegistersComponent],
  imports: [ErpSharedModule, RouterModule.forChild(routes)],
})
export class FinanceModule {}

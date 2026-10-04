import { CoreModule, permissionGuard } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ErpSharedModule } from '../erp-shared/erp-shared.module';
import { ConfigPackageCardComponent } from './package-card/config-package-card.component';
import { ConfigPackageRecordsComponent } from './package-records/config-package-records.component';
import { ConfigPackagesComponent } from './packages/config-packages.component';
import { ConfigTemplatesComponent } from './templates/config-templates.component';
import { ConfigWorksheetComponent } from './worksheet/config-worksheet.component';
import { DataImportComponent } from './import/data-import.component';

/**
 * RapidStart: configuration packages, templates and worksheet, and the
 * import wizard, for bringing a company's set-up and master data in.
 */
const routes: Routes = [
  { path: '', redirectTo: 'worksheet', pathMatch: 'full' },
  {
    path: 'worksheet',
    component: ConfigWorksheetComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.RapidStart' },
  },
  {
    path: 'packages',
    component: ConfigPackagesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.RapidStart' },
  },
  {
    path: 'packages/:id',
    component: ConfigPackageCardComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.RapidStart' },
  },
  {
    path: 'packages/:id/tables/:tableId',
    component: ConfigPackageRecordsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.RapidStart' },
  },
  {
    path: 'templates',
    component: ConfigTemplatesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.RapidStart' },
  },
  {
    path: 'import',
    component: DataImportComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'Erp.RapidStart.Apply' },
  },
];

@NgModule({
  declarations: [
    ConfigWorksheetComponent,
    ConfigPackagesComponent,
    ConfigPackageCardComponent,
    ConfigPackageRecordsComponent,
    ConfigTemplatesComponent,
    DataImportComponent,
  ],
  imports: [CoreModule, ThemeSharedModule, ErpSharedModule, RouterModule.forChild(routes)],
})
export class RapidStartModule {}

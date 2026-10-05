import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Loaded with the first page that needs it, so the bar stays out of the initial bundle.
const roleShell = () =>
  import('./erp/components/role-nav-bar/role-shell.component').then(m => m.RoleShellComponent);

const routes: Routes = [
  // The home page and the ERP pages share the role navigation bar.
  {
    path: '',
    pathMatch: 'full',
    loadComponent: roleShell,
    children: [{ path: '', loadChildren: () => import('./home/home.module').then(m => m.HomeModule) }],
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(m => m.AccountModule.forLazy()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(m => m.IdentityModule.forLazy()),
  },
  {
    path: 'tenant-management',
    loadChildren: () =>
      import('@abp/ng.tenant-management').then(m => m.TenantManagementModule.forLazy()),
  },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m => m.SettingManagementModule.forLazy()),
  },
  {
    path: 'erp',
    loadComponent: roleShell,
    children: [{ path: '', loadChildren: () => import('./erp/erp.module').then(m => m.ErpModule) }],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { paramsInheritanceStrategy: 'always' })],
  exports: [RouterModule],
})
export class AppRoutingModule {}

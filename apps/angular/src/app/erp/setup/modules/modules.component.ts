import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ErpModuleDto, ModuleService } from '@proxy/modules';
import { CompanyService } from '../../services/company.service';

/** One heading of the list, with the modules under it. */
interface ModuleGroup {
  name: string;
  modules: ErpModuleDto[];
}

/**
 * Turning the system's own apps on and off for this company.
 * <p>
 * Mirrors Odoo's Apps page and Business Central's per-company feature management. Switching a
 * module off hides its screens and closes its endpoints; its data is kept and comes back
 * untouched when it is switched on again.
 * </p>
 */
@Component({
  selector: 'app-modules',
  templateUrl: './modules.component.html',
})
export class ModulesComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(ModuleService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);

  groups: ModuleGroup[] = [];
  busy = false;

  ngOnInit(): void {
    this.load();

    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  /** Core modules and ones still needed by another cannot be switched off. */
  isLocked(module: ErpModuleDto): boolean {
    return module.isCore || (module.enabled && module.blockedBy.length > 0);
  }

  lockReason(module: ErpModuleDto): string | null {
    if (module.isCore) {
      return 'Erp::ModuleIsCore';
    }

    return module.enabled && module.blockedBy.length > 0 ? 'Erp::ModuleRequiredBy' : null;
  }

  /**
   * Switching a module off closes a part of the system for everyone in the company, so it is
   * confirmed first. Switching one on needs no warning.
   */
  toggle(module: ErpModuleDto): void {
    if (this.isLocked(module) || this.busy) {
      return;
    }

    if (module.enabled) {
      this.confirmation
        .warn('Erp::ModuleWillBeSwitchedOff', 'Erp::AreYouSure', {
          messageLocalizationParams: [module.displayName ?? ''],
        })
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(status => {
          if (status === Confirmation.Status.confirm) {
            this.setEnabled(module, false);
          }
        });

      return;
    }

    this.setEnabled(module, true);
  }

  private setEnabled(module: ErpModuleDto, enabled: boolean): void {
    this.busy = true;
    this.service
      .setEnabled({ code: module.code!, enabled })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.busy = false;
          this.toaster.success('Erp::SavedSuccessfully');

          // The whole list is re-read: one switch changes what the others allow.
          this.load();
        },
        error: () => (this.busy = false),
      });
  }

  private load(): void {
    this.busy = true;
    this.service
      .getList()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: result => {
          this.busy = false;
          this.groups = ModulesComponent.group(result.items ?? []);
        },
        error: () => (this.busy = false),
      });
  }

  private static group(modules: ErpModuleDto[]): ModuleGroup[] {
    const groups: ModuleGroup[] = [];

    for (const module of modules) {
      const name = module.group ?? '';
      const existing = groups.find(g => g.name === name);

      if (existing) {
        existing.modules.push(module);
      } else {
        groups.push({ name, modules: [module] });
      }
    }

    return groups;
  }
}

import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ErpModuleDto, ModuleService } from '@proxy/modules';
import { switchMap } from 'rxjs/operators';
import { CompanyService } from '../../services/company.service';
import { ModuleRoutesService } from '../../services/module-routes.service';

/** One module as the page shows it, with what the template needs worked out once. */
interface ModuleCard {
  module: ErpModuleDto;
  locked: boolean;
  requires: string;
  requiredBy: string;
}

/** One heading of the list, with the modules under it. */
interface ModuleGroup {
  name: string;
  cards: ModuleCard[];
}

/**
 * Turning the system's own apps on and off for this company.
 * <p>
 * Switching a
 * module off hides its screens and closes its endpoints; its data is kept and comes back
 * untouched when it is switched on again.
 * </p>
 * <p>
 * The list is the one the menu is built from: the page draws the copy already in hand straight
 * away and refreshes it with the same request, so opening the page costs one read at most.
 * </p>
 */
@Component({
  selector: 'app-modules',
  templateUrl: './modules.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModulesComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(ModuleService);
  private readonly moduleRoutes = inject(ModuleRoutesService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly cdr = inject(ChangeDetectorRef);

  groups: ModuleGroup[] = [];
  busy = false;

  ngOnInit(): void {
    this.moduleRoutes.changed$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(modules => {
        this.groups = ModulesComponent.group(modules);
        this.cdr.markForCheck();
      });

    this.load();

    // The app reads the new company's modules itself; this only shows the spinner meanwhile.
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  /**
   * Switching a module off closes a part of the system for everyone in the company, so it is
   * confirmed first. Switching one on needs no warning.
   */
  toggle(card: ModuleCard, input: HTMLInputElement): void {
    const module = card.module;

    // The switch moves under the click; it stays where it stands until the server agrees.
    input.checked = !!module.enabled;

    if (card.locked || this.busy) {
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
    this.setBusy(true);
    this.service
      .setEnabled({ code: module.code!, enabled })
      // The whole list is re-read: one switch changes what the others allow, and the menu
      // follows from the same read.
      .pipe(
        switchMap(() => this.moduleRoutes.refresh()),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => {
          this.setBusy(false);
          this.toaster.success('Erp::SavedSuccessfully');
        },
        error: () => this.setBusy(false),
      });
  }

  private load(): void {
    this.setBusy(true);
    this.moduleRoutes
      .refresh()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.setBusy(false),
        error: () => this.setBusy(false),
      });
  }

  private setBusy(busy: boolean): void {
    this.busy = busy;
    this.cdr.markForCheck();
  }

  private static group(modules: ErpModuleDto[]): ModuleGroup[] {
    const groups = new Map<string, ModuleGroup>();

    for (const module of modules) {
      const name = module.group ?? '';
      let group = groups.get(name);

      if (!group) {
        group = { name, cards: [] };
        groups.set(name, group);
      }

      group.cards.push({
        module,
        // Core modules and ones still needed by another cannot be switched off.
        locked: module.isCore || (module.enabled && (module.blockedBy?.length ?? 0) > 0),
        requires: (module.dependsOn ?? []).join(', '),
        requiredBy: module.enabled ? (module.blockedBy ?? []).join(', ') : '',
      });
    }

    return [...groups.values()];
  }
}

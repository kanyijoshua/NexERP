import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { ChangeDetectorRef, DestroyRef, Directive, inject } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { CompanyService } from '../../services/company.service';
import {
  ErpCardAction,
  ErpFactBoxGroup,
  ErpFactBoxItem,
  ErpFactBoxTile,
  ErpFastTab,
} from './erp-card.models';

/**
 * Base class for Business Central style Card pages (e.g. Customer Card,
 * Vendor Card, Item Card, Config Package Card, Setup Cards).
 *
 * Supports FastTabs (collapsible card sections), FactBoxes (side pane with
 * live KPI cues and statistical facts), and standard action bars.
 *
 * Usage:
 * ```ts
 * @Component({ ... })
 * export class CustomerCardComponent extends ErpCardBase<CustomerDto> {
 *   override readonly fastTabs: ErpFastTab[] = [
 *     { id: 'general', titleKey: 'Erp::General' },
 *     { id: 'invoicing', titleKey: 'Erp::Invoicing' },
 *   ];
 *   ...
 * }
 * ```
 */
@Directive()
export abstract class ErpCardBase<TRecord = any> {
  protected readonly toaster = inject(ToasterService);
  protected readonly confirmation = inject(ConfirmationService);
  protected readonly companyService = inject(CompanyService);
  protected readonly destroyRef = inject(DestroyRef);
  protected readonly cdr = inject(ChangeDetectorRef, { optional: true });

  data: TRecord | null = null;
  form!: FormGroup;

  isNew = false;
  isBusy = false;
  isReadOnly = false;

  title = '';
  subtitle = '';
  icon = '';
  breadcrumb = '';
  breadcrumbLink: any[] | null = null;
  status = '';
  statusClass = 'bg-secondary';

  /** FastTabs configuration */
  fastTabs: ErpFastTab[] = [];

  /** FactBoxes configuration for the right-side pane */
  factBoxGroups: ErpFactBoxGroup[] = [];

  /** Promoted actions and process buttons */
  actions: ErpCardAction[] = [];

  /** Whether the FactBox right-hand pane is open (toggled with Alt+F2 or icon) */
  showFactBox = true;

  /**
   * Toggles the right-side FactBox pane visibility.
   */
  toggleFactBox(): void {
    this.showFactBox = !this.showFactBox;
    this.cdr?.markForCheck();
  }

  /**
   * Expands or collapses a FastTab section.
   */
  toggleFastTab(id: string): void {
    const tab = this.fastTabs.find(t => t.id === id);
    if (tab) {
      tab.collapsed = !tab.collapsed;
      this.cdr?.markForCheck();
    }
  }

  isFastTabCollapsed(id: string): boolean {
    const tab = this.fastTabs.find(t => t.id === id);
    return tab?.collapsed ?? false;
  }

  /**
   * Updates or appends a KPI tile in a FactBox group.
   */
  updateTile(groupId: string, tile: ErpFactBoxTile): void {
    let group = this.factBoxGroups.find(g => g.id === groupId);
    if (!group) {
      group = { id: groupId, titleKey: groupId, tiles: [] };
      this.factBoxGroups.push(group);
    }
    if (!group.tiles) group.tiles = [];

    const existingIndex = group.tiles.findIndex(t => t.titleKey === tile.titleKey);
    if (existingIndex >= 0) {
      group.tiles[existingIndex] = { ...group.tiles[existingIndex], ...tile };
    } else {
      group.tiles.push(tile);
    }
    this.cdr?.markForCheck();
  }

  /**
   * Updates or appends a key-value fact in a FactBox group.
   */
  updateFact(groupId: string, fact: ErpFactBoxItem): void {
    let group = this.factBoxGroups.find(g => g.id === groupId);
    if (!group) {
      group = { id: groupId, titleKey: groupId, facts: [] };
      this.factBoxGroups.push(group);
    }
    if (!group.facts) group.facts = [];

    const existingIndex = group.facts.findIndex(f => f.labelKey === fact.labelKey);
    if (existingIndex >= 0) {
      group.facts[existingIndex] = { ...group.facts[existingIndex], ...fact };
    } else {
      group.facts.push(fact);
    }
    this.cdr?.markForCheck();
  }

  /**
   * Builds standard Business Central card actions: Save & Discard.
   */
  protected buildDefaultCardActions(): ErpCardAction[] {
    return [
      {
        key: 'save',
        labelKey: 'Erp::Save',
        icon: 'fas fa-check',
        primary: true,
        disabled: () => this.isBusy || !this.form || this.form.invalid,
        action: () => this.save(),
      },
      {
        key: 'discard',
        labelKey: 'Erp::Discard',
        icon: 'fas fa-undo',
        cssClass: 'btn-outline-secondary',
        disabled: () => this.isBusy || !this.form?.dirty,
        action: () => this.discard(),
      },
    ];
  }

  /**
   * Override in subclass to handle saving.
   */
  save(): void {}

  /**
   * Override in subclass to handle discard/reset.
   */
  discard(): void {
    this.form?.reset();
  }
}

import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, tap } from 'rxjs';
import { DimensionService, DimensionValueDto, ShortcutDimensionSlotDto } from '@proxy/dimensions';
import { PagedResultDto } from '@abp/ng.core';
import { CompanyService } from './company.service';
import { LookupItem, RecordEntity, RecordQuery } from '../erp-shared';

const STORAGE_KEY = 'erp_dimension_slots';

/**
 * Manages the company's configured dimension slots (1..8):
 * - Slots 1 and 2 are Global Dimensions 1 and 2 (as well as Shortcut Dimensions 1 and 2).
 * - Slots 3..8 are Shortcut Dimensions 3..8.
 * Caches them in localStorage for synchronous availability on reload, updates whenever
 * the company switches or General Ledger Setup saves, and provides lookup entities
 * so record cards and journals can select dimension values with friendly names.
 */
@Injectable({
  providedIn: 'root',
})
export class DimensionStateService {
  private readonly dimensionService = inject(DimensionService);
  private readonly companyService = inject(CompanyService);

  private readonly slotsSubject = new BehaviorSubject<ShortcutDimensionSlotDto[]>(this.readInitialSlots());
  readonly slots$ = this.slotsSubject.asObservable();

  private dimensionNames: Record<number, string> = {};
  private dimensionCodes: Record<number, string> = {};

  constructor() {
    this.rebuildMaps(this.slotsSubject.value);
    this.load();
    this.companyService.companyChanged$.subscribe(() => {
      this.load();
    });
  }

  private readInitialSlots(): ShortcutDimensionSlotDto[] {
    try {
      if (typeof window !== 'undefined' && window.localStorage) {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (raw) {
          return JSON.parse(raw);
        }
      }
    } catch {
      // Ignore storage read errors
    }
    return [];
  }

  private persistSlots(slots: ShortcutDimensionSlotDto[]): void {
    try {
      if (typeof window !== 'undefined' && window.localStorage) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(slots));
      }
    } catch {
      // Ignore storage write errors
    }
  }

  load(): void {
    this.dimensionService
      .getShortcutSlots()
      .pipe(
        map(res => res.items ?? []),
        catchError(() => of([] as ShortcutDimensionSlotDto[])),
      )
      .subscribe(slots => {
        this.setSlots(slots);
      });
  }

  reload(): void {
    this.load();
  }

  setSlots(slots: ShortcutDimensionSlotDto[]): void {
    this.slotsSubject.next(slots);
    this.rebuildMaps(slots);
    this.persistSlots(slots);
  }

  private rebuildMaps(slots: ShortcutDimensionSlotDto[]): void {
    const names: Record<number, string> = {};
    const codes: Record<number, string> = {};
    for (const slot of slots) {
      if (slot.slotNumber) {
        if (slot.dimensionName) {
          names[slot.slotNumber] = slot.dimensionName;
        }
        if (slot.dimensionCode) {
          codes[slot.slotNumber] = slot.dimensionCode;
        }
      }
    }
    this.dimensionNames = names;
    this.dimensionCodes = codes;
  }

  getDimensionName(slot: number): string | undefined {
    return this.dimensionNames[slot];
  }

  getDimensionCode(slot: number): string | undefined {
    return this.dimensionCodes[slot];
  }

  getSlot(slot: number): ShortcutDimensionSlotDto | undefined {
    return this.slotsSubject.value.find(s => s.slotNumber === slot);
  }

  searchValues(slotNumber: number, term: string): Observable<LookupItem[]> {
    const slot = this.getSlot(slotNumber);
    if (!slot || !slot.values) {
      return of([]);
    }
    const q = (term ?? '').trim().toLowerCase();
    const items = slot.values
      .filter(v => !q || (v.code ?? '').toLowerCase().includes(q) || (v.name ?? '').toLowerCase().includes(q))
      .map(v => ({
        id: v.id ?? v.code ?? '',
        code: v.code ?? '',
        name: v.name ?? undefined,
      }));
    return of(items);
  }

  getValuesPaged(slotNumber: number, query?: RecordQuery): Observable<PagedResultDto<DimensionValueDto>> {
    const slot = this.getSlot(slotNumber);
    const all = slot?.values ?? [];
    const filter = (query?.filter ?? '').trim().toLowerCase();
    let filtered = all;
    if (filter) {
      filtered = all.filter(
        v =>
          (v.code ?? '').toLowerCase().includes(filter) ||
          (v.name ?? '').toLowerCase().includes(filter),
      );
    }
    const skip = query?.skipCount ?? 0;
    const max = query?.maxResultCount ?? 50;
    const items = filtered.slice(skip, skip + max);
    return of({ items, totalCount: filtered.length });
  }

  getValue(slotNumber: number, idOrCode: string): Observable<DimensionValueDto | undefined> {
    const slot = this.getSlot(slotNumber);
    const val = (slot?.values ?? []).find(
      v => v.id === idOrCode || (v.code ?? '').toLowerCase() === (idOrCode ?? '').toLowerCase(),
    );
    return of(val);
  }

  /**
   * Generates RecordEntity descriptors for globalDimension1, globalDimension2, and shortcutDimension1..8,
   * registered with RecordEntityRegistry so erp-lookup can use `lookupEntity: 'globalDimension1'`.
   */
  buildEntities(): RecordEntity[] {
    const entities: RecordEntity[] = [];

    const makeDescriptor = (key: string, slotNumber: number, labelKey: string): RecordEntity => ({
      key,
      titleKey: labelKey,
      pluralKey: labelKey,
      icon: 'fas fa-cubes',
      permission: 'Erp.Dimensions',
      columns: [
        { field: 'code', labelKey: 'Erp::Code', width: 140 },
        { field: 'name', labelKey: 'Erp::Name', width: 220 },
      ],
      sections: [{ key: 'general', labelKey: 'Erp::General' }],
      fields: [
        { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true },
        { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true },
      ],
      getList: (q: RecordQuery) => this.getValuesPaged(slotNumber, q),
      get: (id: string) => this.getValue(slotNumber, id).pipe(map(v => v || ({} as DimensionValueDto))),
      create: () => of({} as any),
      update: () => of({} as any),
      delete: () => of(undefined),
      toItem: (dto: DimensionValueDto) => ({
        id: dto.code ?? dto.id,
        code: dto.code ?? '',
        name: dto.name ?? undefined,
      }),
      newRecord: () => ({ code: '', name: '' }),
    });

    entities.push(makeDescriptor('globalDimension1', 1, 'Erp::GlobalDimension1Code'));
    entities.push(makeDescriptor('globalDimension2', 2, 'Erp::GlobalDimension2Code'));

    for (let slot = 1; slot <= 8; slot++) {
      entities.push(makeDescriptor(`shortcutDimension${slot}`, slot, `Erp::ShortcutDimension${slot}Code`));
    }

    return entities;
  }
}

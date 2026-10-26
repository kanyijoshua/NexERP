import { LocalizationService, LocalizationWithDefault } from '@abp/ng.core';
import { Injectable, Injector, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { DimensionStateService } from './dimension-state.service';

/** `Erp::Enum:ConsolidationTranslationMethod.Average` -> `Average`. */
const ENUM_KEY = /(?:^|::)Enum:[A-Za-z0-9_]+\.([A-Za-z0-9_]+)$/;

/** Words written in capitals (or otherwise) when an enum member is shown without a text. */
const ACRONYMS: Record<string, string> = {
  GL: 'G/L',
  Vat: 'VAT',
  Lcy: 'LCY',
  Csv: 'CSV',
  Html: 'HTML',
  Json: 'JSON',
  Xlsx: 'XLSX',
  Pdf: 'PDF',
  Fifo: 'FIFO',
  Lifo: 'LIFO',
  Rfq: 'RFQ',
  Eoi: 'EOI',
};

/** `PendingApproval` -> `Pending Approval`, `GLAccount` -> `G/L Account`. */
export function humanizeEnumMember(member: string): string {
  const words = member.match(/[A-Z]+(?=[A-Z][a-z]|\d|$)|[A-Z]?[a-z]+|[A-Z]+|\d+/g) ?? [member];
  const text = words.map(w => ACRONYMS[w] ?? w).join(' ');
  return text.charAt(0).toUpperCase() + text.slice(1);
}

/**
 * Detects whether a localization key refers to a configurable dimension slot:
 * - GlobalDimension1 / GlobalDimension1Code / ShortcutDimension1 / ShortcutDimension1Code -> slot 1
 * - GlobalDimension2 / GlobalDimension2Code / ShortcutDimension2 / ShortcutDimension2Code -> slot 2
 * - ShortcutDimension3..8 / ShortcutDimension3..8Code -> slots 3..8
 * Excludes setup keys like `Erp::Setup:GlobalDimension1Code`.
 */
export function extractDimensionSlot(key: string): number | null {
  if (!key) {
    return null;
  }
  if (key.includes('Setup:')) {
    return null;
  }
  const globalMatch = /(?:^|::)GlobalDimension([12])(?:Code)?$/i.exec(key);
  if (globalMatch) {
    return parseInt(globalMatch[1], 10);
  }
  const shortcutMatch = /(?:^|::)ShortcutDimension([1-8])(?:Code)?$/i.exec(key);
  if (shortcutMatch) {
    return parseInt(shortcutMatch[1], 10);
  }
  return null;
}

/**
 * ABP's localization, enhanced with:
 * 1. Configured dimension names (e.g. `Department`, `Project`) for Global & Shortcut dimension labels.
 * 2. An enum value without a text shows its member name in words ("Average") rather than the key.
 */
@Injectable()
export class ErpLocalizationService extends LocalizationService {
  private dimensionState?: DimensionStateService;
  private readonly ngInjector = inject(Injector);

  private getDimensionState(): DimensionStateService | null {
    if (!this.dimensionState) {
      try {
        this.dimensionState = this.ngInjector.get(DimensionStateService, null) ?? undefined;
      } catch {
        // Ignored during early initialization if injector is not ready
      }
    }
    return this.dimensionState ?? null;
  }

  override instant(key: string | LocalizationWithDefault, ...interpolateParams: string[]): string {
    const text = super.instant(key, ...interpolateParams);
    return withFallbacks(key, text, this.getDimensionState());
  }

  override get(key: string | LocalizationWithDefault, ...interpolateParams: string[]): Observable<string> {
    return super
      .get(key, ...interpolateParams)
      .pipe(map(text => withFallbacks(key, text, this.getDimensionState())));
  }
}

/** The text when there is one; for dimension or enum keys, dynamic fallbacks. */
export function withFallbacks(
  key: string | LocalizationWithDefault,
  text: string,
  dimensionState?: DimensionStateService | null,
): string {
  const rawKey = typeof key === 'string' ? key : key?.key;
  if (!rawKey) {
    return text;
  }

  // 1. Configured dimension name fallback
  if (dimensionState) {
    const slot = extractDimensionSlot(rawKey);
    if (slot !== null) {
      const dimName = dimensionState.getDimensionName(slot) || dimensionState.getDimensionCode(slot);
      if (dimName) {
        return dimName;
      }
    }
  }

  // 2. Enum member fallback
  if (text !== rawKey && !rawKey.endsWith('::' + text)) {
    return text;
  }
  const member = ENUM_KEY.exec(rawKey)?.[1];
  return member ? humanizeEnumMember(member) : text;
}

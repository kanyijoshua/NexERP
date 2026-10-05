import { Component, Input, OnChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { LookupItem } from '../models';
import { RecordEntity, RecordField, RecordSection } from './record-entity';
import { fieldsOf, visibleSections } from './record-form';

/**
 * The fields of a record, grouped by section. The quick dialog renders the sections as plain
 * headings; the card page renders them as collapsible FastTabs (`collapsible`).
 */
@Component({
  selector: 'erp-record-form',
  templateUrl: './record-form.component.html',
  standalone: false,
})
export class RecordFormComponent implements OnChanges {
  @Input({ required: true }) entity!: RecordEntity;
  @Input({ required: true }) form!: FormGroup;
  /** The loaded record, for `readonly` fields and the codes shown for id lookups. */
  @Input() record: Record<string, any> | null = null;
  /** Hides `cardOnly` and `readonly` fields. */
  @Input() quick = false;
  @Input() collapsible = false;
  /** Prefix of the element ids, so two forms on one page do not clash. */
  @Input() idPrefix = 'rec';

  private readonly collapsed = new Map<string, boolean>();
  private _sections: RecordSection[] = [];
  private _fieldsMap = new Map<string, RecordField[]>();

  ngOnChanges(): void {
    if (this.entity) {
      this._sections = visibleSections(this.entity, this.quick);
      this._fieldsMap.clear();
      for (const section of this._sections) {
        this._fieldsMap.set(section.key, fieldsOf(this.entity, section.key, this.quick));
      }
    }
  }

  get sections(): RecordSection[] {
    return this._sections.length ? this._sections : (this.entity ? visibleSections(this.entity, this.quick) : []);
  }

  fields(section: RecordSection): RecordField[] {
    return this._fieldsMap.get(section.key) ?? (this.entity ? fieldsOf(this.entity, section.key, this.quick) : []);
  }

  isCollapsed(section: RecordSection): boolean {
    return this.collapsible && (this.collapsed.get(section.key) ?? !!section.collapsed);
  }

  toggle(section: RecordSection): void {
    this.collapsed.set(section.key, !this.isCollapsed(section));
  }

  controlId(field: RecordField): string {
    return `${this.idPrefix}-${this.entity.key}-${field.field}`;
  }

  /** Values shown on a folded FastTab header: the key fields of the folded tab. */
  summary(section: RecordSection): string {
    return this.fields(section)
      .map(f => this.display(f))
      .filter(text => !!text)
      .slice(0, 3)
      .join(' · ');
  }

  display(field: RecordField): string {
    const value = this.form.get(field.field)?.value ?? this.record?.[field.field];
    if (value === null || value === undefined || value === '' || typeof value === 'boolean') {
      return '';
    }
    if (field.type === 'select') {
      return field.options?.find(o => o.value === value)?.label ?? String(value);
    }
    if (field.type === 'lookup' && field.lookupDisplayField) {
      return this.displayTextOf(field) ?? '';
    }
    return String(value);
  }

  displayTextOf(field: RecordField): string | null {
    if (!field.lookupDisplayField) {
      return null;
    }
    return (
      this.form.get(field.lookupDisplayField)?.value ??
      this.record?.[field.lookupDisplayField] ??
      null
    );
  }

  /** Keeps the code next to an id lookup in step with the pick. */
  onLookupSelected(field: RecordField, item: LookupItem | null): void {
    if (field.lookupDisplayField) {
      this.form.get(field.lookupDisplayField)?.setValue(item?.code ?? null);
    }
  }

  isInvalid(field: RecordField): boolean {
    const control = this.form.get(field.field);
    return !!control && control.invalid && (control.touched || control.dirty);
  }

  trackSection(_index: number, section: RecordSection): string {
    return section.key;
  }

  trackField(_index: number, field: RecordField): string {
    return field.field;
  }
}

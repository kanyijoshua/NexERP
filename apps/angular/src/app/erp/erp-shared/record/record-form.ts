import { FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { RecordEntity, RecordField, RecordSection } from './record-entity';

export interface RecordFormOptions {
  /** A new record: `createOnly` fields stay editable. */
  isNew: boolean;
  /** Everything read-only (no update permission). */
  readonly?: boolean;
}

/** Fields that hold a value the user edits; `readonly` fields are only shown. */
export function editableFields(entity: RecordEntity): RecordField[] {
  return entity.fields.filter(f => f.type !== 'readonly');
}

/**
 * The form of a record card. Every editable field gets a control, also the ones the quick dialog does
 * not show, so that saving from the quick dialog never blanks a field it did not render.
 */
export function buildRecordForm(
  entity: RecordEntity,
  record: Record<string, any> | null | undefined,
  options: RecordFormOptions,
): FormGroup {
  const controls: Record<string, FormControl> = {};

  for (const field of editableFields(entity)) {
    const raw = record?.[field.field];
    // Dates arrive as ISO date-times; a date input wants yyyy-MM-dd.
    const value = field.type === 'date' && typeof raw === 'string' ? raw.substring(0, 10) : raw;
    controls[field.field] = new FormControl(
      {
        value: value === undefined ? defaultValue(field) : value,
        disabled: !!options.readonly || (!!field.createOnly && !options.isNew),
      },
      validatorsOf(field),
    );

    // An id lookup keeps the code it shows next to it (e.g. itemCategoryId + itemCategoryCode).
    if (field.lookupDisplayField && !controls[field.lookupDisplayField]) {
      controls[field.lookupDisplayField] = new FormControl(record?.[field.lookupDisplayField] ?? null);
    }
  }

  return new FormGroup(controls);
}

/** The sections that have at least one field to show, in declaration order. */
export function visibleSections(entity: RecordEntity, quick: boolean): RecordSection[] {
  const fallback = entity.sections[0]?.key;
  return entity.sections.filter(section =>
    fieldsOf(entity, section.key, quick, fallback).length > 0,
  );
}

export function fieldsOf(
  entity: RecordEntity,
  sectionKey: string,
  quick: boolean,
  fallbackSection = entity.sections[0]?.key,
): RecordField[] {
  return entity.fields.filter(
    f =>
      (f.section ?? fallbackSection) === sectionKey &&
      (!quick || !f.cardOnly) &&
      // A read-only figure (balance, inventory) means nothing on a record that does not exist yet.
      (!quick || f.type !== 'readonly'),
  );
}

function defaultValue(field: RecordField): unknown {
  switch (field.type) {
    case 'checkbox':
      return false;
    case 'number':
    case 'currency':
      return 0;
    default:
      return null;
  }
}

function validatorsOf(field: RecordField): ValidatorFn[] {
  const validators: ValidatorFn[] = [];
  if (field.required) {
    validators.push(field.type === 'checkbox' ? Validators.requiredTrue : Validators.required);
  }
  if (field.maxLength) {
    validators.push(Validators.maxLength(field.maxLength));
  }
  if (field.min !== undefined) {
    validators.push(Validators.min(field.min));
  }
  if (field.type === 'email') {
    validators.push(Validators.email);
  }
  return validators;
}

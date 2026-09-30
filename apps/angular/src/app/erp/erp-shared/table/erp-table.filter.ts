import {
  ErpFilterCriterion,
  ErpFilterOperator,
  ErpFilterState,
  ErpTableColumn,
} from './erp-table.models';

/** Operators that take no value: the operator alone is the condition. */
const NO_VALUE: ErpFilterOperator[] = ['isEmpty', 'isNotEmpty', 'today', 'thisWeek', 'thisMonth', 'thisYear', 'isTrue', 'isFalse'];

export function isNoValueOperator(op: ErpFilterOperator): boolean {
  return NO_VALUE.includes(op);
}

/** The operators a column offers, by its type. Labels are localization keys. */
export function operatorsFor(column?: ErpTableColumn): { value: ErpFilterOperator; label: string }[] {
  const op = (value: ErpFilterOperator) => ({ value, label: `Erp::Table:Op:${value}` });
  const type = column?.type ?? 'text';

  switch (type) {
    case 'number':
    case 'currency':
      return ['equals', 'notEquals', 'gt', 'gte', 'lt', 'lte', 'between', 'expression', 'isEmpty', 'isNotEmpty'].map(v => op(v as ErpFilterOperator));
    case 'date':
    case 'datetime':
      return ['equals', 'before', 'after', 'between', 'today', 'thisWeek', 'thisMonth', 'thisYear', 'expression', 'isEmpty', 'isNotEmpty'].map(v =>
        op(v as ErpFilterOperator),
      );
    case 'boolean':
    case 'switch':
      return [op('isTrue'), op('isFalse')];
    case 'select':
    case 'badge':
      return column?.options?.length ? [op('equals'), op('notEquals')] : textOperators().map(op);
    default:
      return textOperators().map(op);
  }
}

function textOperators(): ErpFilterOperator[] {
  return ['contains', 'notContains', 'equals', 'notEquals', 'startsWith', 'endsWith', 'expression', 'isEmpty', 'isNotEmpty'];
}

export function defaultOperatorFor(column?: ErpTableColumn): ErpFilterOperator {
  return operatorsFor(column)[0]?.value ?? 'contains';
}

/** A condition that still waits for its value is left out rather than filtering everything away. */
export function isComplete(criterion: ErpFilterCriterion): boolean {
  if (isNoValueOperator(criterion.operator)) {
    return true;
  }
  if (criterion.operator === 'between') {
    return filled(criterion.value) || filled(criterion.valueTo);
  }
  return filled(criterion.value);
}

function filled(value: unknown): boolean {
  return value !== undefined && value !== null && String(value).trim() !== '';
}

/**
 * The JSON the list services read (`DynamicFilter`), or undefined when there is nothing to
 * filter on. Field names are the column's `filterField` or its `field`.
 */
export function toDynamicFilter(state: ErpFilterState, columns: ErpTableColumn[]): string | undefined {
  const conditions = state.criteria.filter(isComplete).map(c => {
    const column = columns.find(col => col.field === c.field);
    return {
      field: column?.filterField ?? c.field,
      operator: c.operator,
      value: filled(c.value) ? String(c.value) : null,
      valueTo: filled(c.valueTo) ? String(c.valueTo) : null,
    };
  });

  return conditions.length ? JSON.stringify({ logic: state.logic, conditions }) : undefined;
}

// ------------------------------------------------------------------------------------------
// Client-side evaluation, for tables fed with rows already in memory.
// ------------------------------------------------------------------------------------------

export function matchesState(row: any, state: ErpFilterState, columns: ErpTableColumn[], today = new Date()): boolean {
  const search = state.searchTerm.trim().toLowerCase();
  if (search) {
    const hit = columns.some(col => col.filterable !== false && String(row?.[col.field] ?? '').toLowerCase().includes(search));
    if (!hit) {
      return false;
    }
  }

  const active = state.criteria.filter(isComplete);
  if (!active.length) {
    return true;
  }

  const results = active.map(c => matches(row?.[c.field], c, columns.find(col => col.field === c.field), today));
  return state.logic === 'or' ? results.some(Boolean) : results.every(Boolean);
}

type Kind = 'text' | 'number' | 'date' | 'bool';

function kindOf(column?: ErpTableColumn): Kind {
  switch (column?.type) {
    case 'number':
    case 'currency':
      return 'number';
    case 'date':
    case 'datetime':
      return 'date';
    case 'boolean':
    case 'switch':
      return 'bool';
    default:
      return 'text';
  }
}

export function matches(value: any, c: ErpFilterCriterion, column: ErpTableColumn | undefined, today = new Date()): boolean {
  return test(value, c.operator, c.value, c.valueTo, kindOf(column), startOfDay(today));
}

function test(value: any, op: ErpFilterOperator, target: any, targetTo: any, kind: Kind, today: Date): boolean {
  const empty = value === null || value === undefined || value === '';

  switch (op) {
    case 'isEmpty':
      return empty;
    case 'isNotEmpty':
      return !empty;
    case 'isTrue':
      return value === true;
    case 'isFalse':
      return value !== true;
    case 'expression':
      return expression(value, String(target ?? ''), kind, today);
  }

  if (kind === 'date') {
    return dateTest(value, op, target, targetTo, today);
  }

  if (kind === 'number' || (typeof value === 'number' && op !== 'contains')) {
    const n = Number(value);
    const t = Number(target);
    switch (op) {
      case 'equals':
        return !empty && n === t;
      case 'notEquals':
        return empty || n !== t;
      case 'gt':
        return !empty && n > t;
      case 'gte':
        return !empty && n >= t;
      case 'lt':
        return !empty && n < t;
      case 'lte':
        return !empty && n <= t;
      case 'between':
        return !empty && (!filled(target) || n >= t) && (!filled(targetTo) || n <= Number(targetTo));
    }
  }

  const text = String(value ?? '').toLowerCase();
  const t = String(target ?? '').toLowerCase();
  switch (op) {
    case 'contains':
      return text.includes(t);
    case 'notContains':
      return !text.includes(t);
    case 'startsWith':
      return text.startsWith(t);
    case 'endsWith':
      return text.endsWith(t);
    case 'equals':
      return text === t;
    case 'notEquals':
      return text !== t;
    case 'gt':
      return text > t;
    case 'gte':
      return text >= t;
    case 'lt':
      return text < t;
    case 'lte':
      return text <= t;
    case 'between':
      return (!filled(target) || text >= t) && (!filled(targetTo) || text <= String(targetTo).toLowerCase());
    default:
      return true;
  }
}

function dateTest(value: any, op: ErpFilterOperator, target: any, targetTo: any, today: Date): boolean {
  if (value === null || value === undefined || value === '') {
    return false;
  }

  const day = startOfDay(toDate(value)).getTime();
  const at = (v: any) => startOfDay(parseDate(v, today)).getTime();
  const weekStart = addDays(today, -((today.getDay() + 6) % 7));

  switch (op) {
    case 'equals':
      return day === at(target);
    case 'notEquals':
      return day !== at(target);
    case 'before':
    case 'lt':
      return day < at(target);
    case 'lte':
      return day <= at(target);
    case 'after':
    case 'gt':
      return day > at(target);
    case 'gte':
      return day >= at(target);
    case 'between':
      return (!filled(target) || day >= at(target)) && (!filled(targetTo) || day <= at(targetTo));
    case 'today':
      return day === today.getTime();
    case 'thisWeek':
      return day >= weekStart.getTime() && day < addDays(weekStart, 7).getTime();
    case 'thisMonth':
      return new Date(day).getMonth() === today.getMonth() && new Date(day).getFullYear() === today.getFullYear();
    case 'thisYear':
      return new Date(day).getFullYear() === today.getFullYear();
    default:
      return true;
  }
}

/**
 * A Business Central filter expression: `|` is or, `&` is and; a term is a value, a range
 * `a..b` (either end open), a comparison (`<>`, `>=`, `<=`, `>`, `<`, `=`), a text wildcard `*`,
 * or `''` for blank. `@` (ignore case) is accepted; text is compared ignoring case anyway.
 * The server implements the same syntax for lists it filters itself.
 */
export function expression(value: any, expr: string, kind: Kind, today: Date): boolean {
  if (!expr.trim()) {
    return true;
  }
  return expr.split('|').some(alternative => alternative.split('&').every(part => term(value, part.trim(), kind, today)));
}

function term(value: any, raw: string, kind: Kind, today: Date): boolean {
  const t = raw.replace(/^@/, '');
  const empty = value === null || value === undefined || value === '';
  const keyword = (v: string) => (kind === 'date' && /^(t|today)$/i.test(v.trim()) ? isoDate(today) : v.trim());

  if (t === "''" || t === '""') {
    return empty;
  }
  if (t.startsWith('<>')) {
    const rest = t.substring(2);
    return rest === '' || rest === "''" ? !empty : !term(value, rest, kind, today);
  }
  for (const [prefix, op] of [['>=', 'gte'], ['<=', 'lte'], ['>', 'gt'], ['<', 'lt'], ['=', 'equals']] as const) {
    if (t.startsWith(prefix)) {
      return test(value, op, keyword(t.substring(prefix.length)), undefined, kind, today);
    }
  }
  const range = t.indexOf('..');
  if (range >= 0) {
    return test(value, 'between', keyword(t.substring(0, range)), keyword(t.substring(range + 2)), kind, today);
  }
  if (kind === 'text' && t.includes('*')) {
    const pattern = new RegExp('^' + t.split('*').map(escapeRegExp).join('.*') + '$', 'i');
    return pattern.test(String(value ?? ''));
  }
  return test(value, 'equals', keyword(t), undefined, kind, today);
}

function escapeRegExp(text: string): string {
  return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function parseDate(value: any, today: Date): Date {
  return /^(t|today)$/i.test(String(value ?? '').trim()) ? today : toDate(value);
}

/** A date-only string is a local day; `new Date('2026-09-28')` would read it as UTC midnight. */
function toDate(value: any): Date {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(String(value ?? '').trim());
  return match ? new Date(+match[1], +match[2] - 1, +match[3]) : new Date(value);
}

function startOfDay(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

function addDays(date: Date, days: number): Date {
  const next = new Date(date);
  next.setDate(next.getDate() + days);
  return next;
}

function isoDate(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

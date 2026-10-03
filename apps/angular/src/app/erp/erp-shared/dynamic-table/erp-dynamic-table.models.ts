export type ErpDynamicCellType =
  | 'text'
  | 'number'
  | 'currency'
  | 'date'
  | 'badge'
  | 'boolean'
  | 'code';

export interface ErpDynamicTableColumn<T = any> {
  field: string;
  label: string;
  type?: ErpDynamicCellType;
  width?: string | number;
  sortable?: boolean;
  filterable?: boolean;
  visible?: boolean;
  align?: 'left' | 'center' | 'right';
  badgeClass?: string | ((val: any, row: T) => string);
  currencyCode?: string;
  dateFormat?: string;
  formatter?: (val: any, row: T) => string;
}

export interface ErpDynamicTableAction<T = any> {
  key: string;
  label: string;
  icon?: string;
  class?: string;
  tooltip?: string;
  permission?: string;
  disabled?: (row: T) => boolean;
  action: (row: T, event?: MouseEvent) => void;
}

export interface ErpDropdownOption<T = any> {
  value: T;
  label: string;
  sublabel?: string;
  icon?: string;
  badge?: string;
  badgeClass?: string;
  disabled?: boolean;
  group?: string;
}

export interface ErpDropdownGroup<T = any> {
  name: string;
  options: ErpDropdownOption<T>[];
}

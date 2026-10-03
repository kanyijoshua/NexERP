export type ErpButtonVariant =
  | 'primary'
  | 'secondary'
  | 'success'
  | 'danger'
  | 'warning'
  | 'info'
  | 'light'
  | 'dark'
  | 'outline-primary'
  | 'outline-secondary'
  | 'outline-success'
  | 'outline-danger'
  | 'outline-warning'
  | 'outline-info'
  | 'outline-dark'
  | 'link';

export type ErpButtonSize = 'sm' | 'md' | 'lg';

export interface ErpButtonAction {
  key: string;
  label: string;
  icon?: string;
  permission?: string;
  disabled?: boolean;
  divider?: boolean;
  danger?: boolean;
  action: (event?: MouseEvent) => void;
}

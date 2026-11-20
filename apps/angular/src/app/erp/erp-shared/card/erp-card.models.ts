export interface ErpCardAction {
  key: string;
  labelKey: string;
  icon: string;
  cssClass?: string;
  permission?: string;
  primary?: boolean;
  disabled?: boolean | (() => boolean);
  visible?: boolean | (() => boolean);
  action: (event?: MouseEvent) => void;
}

export interface ErpFastTab {
  id: string;
  titleKey: string;
  icon?: string;
  collapsed?: boolean;
  summary?: string | ((data: any) => string);
  badge?: string | number | ((data: any) => string | number | null);
  badgeClass?: string;
}

export interface ErpFactBoxTile {
  titleKey: string;
  value: string | number;
  icon?: string;
  type?: 'currency' | 'number' | 'text';
  color?: 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'secondary';
  routerLink?: any[];
  queryParams?: Record<string, unknown>;
  action?: () => void;
}

export interface ErpFactBoxItem {
  labelKey: string;
  value: any;
  type?: 'text' | 'currency' | 'number' | 'date' | 'boolean' | 'badge';
  warnWhenTrue?: boolean;
  badgeClass?: string;
  routerLink?: any[];
  action?: () => void;
}

export interface ErpFactBoxGroup {
  id: string;
  titleKey: string;
  icon?: string;
  collapsed?: boolean;
  tiles?: ErpFactBoxTile[];
  facts?: ErpFactBoxItem[];
  /** Renders the group as a picture (avatar) FactBox instead of tiles and facts. */
  picture?: ErpFactBoxPicture;
}

export interface ErpFactBoxPicture {
  /** The table's name in the server's entity registry, e.g. `Customer`. */
  entityType: string;
  entityId: string;
  /** The record's name, for the initials shown while there is no picture. */
  name?: string;
  /** Shows upload and remove; defaults to true. */
  editable?: boolean;
}

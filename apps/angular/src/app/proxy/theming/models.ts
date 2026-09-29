import type { ErpNavbarStyle } from './erp-navbar-style.enum';
import type { ErpCornerStyle } from './erp-corner-style.enum';

export interface ErpThemeDto {
  primaryColor: string;
  accentColor: string;
  navbarStyle: ErpNavbarStyle;
  cornerStyle: ErpCornerStyle;
}

import { Injectable, inject } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MenuSuiteComponent } from './menu-suite.component';

/** Opens the menu suite, from the top bar, the home page or Alt+Q. Only one at a time. */
@Injectable({ providedIn: 'root' })
export class MenuSuiteService {
  private readonly dialog = inject(MatDialog);
  private ref: MatDialogRef<MenuSuiteComponent> | null = null;

  open(): void {
    if (this.ref) {
      return;
    }

    this.ref = this.dialog.open(MenuSuiteComponent, {
      width: '72rem',
      maxWidth: '96vw',
      position: { top: '4rem' },
      panelClass: 'erp-menu-suite-panel',
      autoFocus: 'first-tabbable',
      restoreFocus: true,
    });

    this.ref.afterClosed().subscribe(() => (this.ref = null));
  }
}

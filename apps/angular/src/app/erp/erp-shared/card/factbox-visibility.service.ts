import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'erp.factbox.visible';

/**
 * Whether the side FactBox pane of cards is shown. One choice for every card (record cards,
 * document cards), remembered in this browser, toggled with the pane button or Alt+F2.
 */
@Injectable({ providedIn: 'root' })
export class FactBoxVisibilityService {
  readonly visible = signal(readStored());

  toggle(): void {
    this.set(!this.visible());
  }

  set(visible: boolean): void {
    this.visible.set(visible);
    try {
      localStorage.setItem(STORAGE_KEY, visible ? '1' : '0');
    } catch {
      // Storage blocked (private window): the choice lasts for this page only.
    }
  }

  /** True for the Alt+F2 shortcut. */
  static isToggleShortcut(event: KeyboardEvent): boolean {
    return event.altKey && event.key === 'F2';
  }
}

function readStored(): boolean {
  try {
    return localStorage.getItem(STORAGE_KEY) !== '0';
  } catch {
    return true;
  }
}

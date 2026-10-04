import { DOCUMENT } from '@angular/common';
import { Injectable, NgZone, inject } from '@angular/core';

interface SavedModalState {
  width: string;
  height: string;
  marginLeft: string;
  marginRight: string;
  marginTop: string;
  marginBottom: string;
  maxWidth: string;
  maxHeight: string;
}

// Diagonal outward expand arrows
const MAXIMIZE_SVG = `
  <svg width="13" height="13" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" class="erp-modal-icon">
    <polyline points="10 2 14 2 14 6"></polyline>
    <line x1="14" y1="2" x2="9.5" y2="6.5"></line>
    <polyline points="6 14 2 14 2 10"></polyline>
    <line x1="2" y1="14" x2="6.5" y2="9.5"></line>
  </svg>
`;

// Diagonal inward compress arrows ("Minimize to normal size")
const RESTORE_SVG = `
  <svg width="13" height="13" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" class="erp-modal-icon">
    <polyline points="13.5 6.5 9.5 6.5 9.5 2.5"></polyline>
    <line x1="14" y1="2" x2="9.5" y2="6.5"></line>
    <polyline points="2.5 9.5 6.5 9.5 6.5 13.5"></polyline>
    <line x1="2" y1="14" x2="6.5" y2="9.5"></line>
  </svg>
`;

const CORNER_GRIP_SVG = `
  <svg viewBox="0 0 10 10" width="10" height="10" fill="currentColor" class="modal-resize-grip-icon">
    <circle cx="8.5" cy="8.5" r="1.1"/>
    <circle cx="8.5" cy="5" r="1.1"/>
    <circle cx="5" cy="8.5" r="1.1"/>
    <circle cx="8.5" cy="1.5" r="1.1"/>
    <circle cx="5" cy="5" r="1.1"/>
    <circle cx="1.5" cy="8.5" r="1.1"/>
  </svg>
`;

@Injectable({
  providedIn: 'root',
})
export class ModalResizeService {
  private readonly document = inject(DOCUMENT);
  private readonly zone = inject(NgZone);

  private observer: MutationObserver | null = null;
  private isInitialized = false;

  private readonly minWidth = 340;
  private readonly minHeight = 220;

  /**
   * Keyboard handler for the Alt+F11 shortcut to toggle maximize / minimize.
   */
  private readonly onKeyDown = (e: KeyboardEvent): void => {
    if (e.altKey && (e.key === 'F11' || e.code === 'F11')) {
      const openDialog = this.document.querySelector<HTMLElement>(
        '.modal.show .modal-dialog, .modal-dialog.modal-dialog-resizable',
      );
      if (openDialog) {
        e.preventDefault();
        e.stopPropagation();
        const maxBtn = openDialog.querySelector<HTMLButtonElement>('.erp-modal-maximize-btn');
        const restoreBtn = openDialog.querySelector<HTMLButtonElement>('.erp-modal-restore-btn');
        if (maxBtn) {
          this.toggleMaximize(openDialog, maxBtn, restoreBtn || undefined);
        }
      }
    }
  };

  /**
   * Initializes the modal resize and maximize/minimize capabilities across the entire application.
   */
  public init(): void {
    if (this.isInitialized || typeof window === 'undefined') {
      return;
    }

    this.isInitialized = true;

    // Enhance any modals currently in DOM
    this.enhanceExistingModals();

    // Listen for dynamically created modals (NgbModal, abp-modal, bootstrap modals)
    this.observer = new MutationObserver(() => {
      this.enhanceExistingModals();
    });

    if (this.document.body) {
      this.observer.observe(this.document.body, {
        childList: true,
        subtree: true,
      });
    }

    // Add Alt+F11 shortcut
    this.document.addEventListener('keydown', this.onKeyDown);
  }

  public destroy(): void {
    if (this.observer) {
      this.observer.disconnect();
      this.observer = null;
    }
    this.document.removeEventListener('keydown', this.onKeyDown);
    this.isInitialized = false;
  }

  /**
   * Scans document for modal dialogs and applies resize and maximize/minimize capabilities.
   * Continues to check existing dialogs in case header was rendered in subsequent Angular microtasks.
   */
  public enhanceExistingModals(): void {
    const dialogs = this.document.querySelectorAll<HTMLElement>('.modal-dialog');
    dialogs.forEach(dialog => this.enhanceModal(dialog));
  }

  /**
   * Enhances a single modal dialog element.
   */
  public enhanceModal(dialog: HTMLElement): void {
    if (!dialog) {
      return;
    }

    const content = dialog.querySelector<HTMLElement>('.modal-content');
    if (content && dialog.dataset['resizable'] !== 'true') {
      dialog.dataset['resizable'] = 'true';
      dialog.classList.add('modal-dialog-resizable');
      content.classList.add('modal-content-resizable');

      this.addResizeHandles(dialog, content);
    }

    // Setup or update header actions whenever .modal-header becomes available
    this.setupHeaderActions(dialog);
  }

  private addResizeHandles(dialog: HTMLElement, content: HTMLElement): void {
    // 1. Bottom-Right Corner (SE)
    const seHandle = this.document.createElement('div');
    seHandle.className = 'modal-resize-handle modal-resize-handle--se';
    seHandle.title = 'Resize';
    seHandle.innerHTML = CORNER_GRIP_SVG;
    this.bindResizeDrag(dialog, seHandle, 'se');
    content.appendChild(seHandle);

    // 2. Right Border (E)
    const eHandle = this.document.createElement('div');
    eHandle.className = 'modal-resize-handle modal-resize-handle--e';
    this.bindResizeDrag(dialog, eHandle, 'e');
    content.appendChild(eHandle);

    // 3. Bottom Border (S)
    const sHandle = this.document.createElement('div');
    sHandle.className = 'modal-resize-handle modal-resize-handle--s';
    this.bindResizeDrag(dialog, sHandle, 's');
    content.appendChild(sHandle);

    // 4. Left Border (W)
    const wHandle = this.document.createElement('div');
    wHandle.className = 'modal-resize-handle modal-resize-handle--w';
    this.bindResizeDrag(dialog, wHandle, 'w');
    content.appendChild(wHandle);

    // 5. Bottom-Left Corner (SW)
    const swHandle = this.document.createElement('div');
    swHandle.className = 'modal-resize-handle modal-resize-handle--sw';
    this.bindResizeDrag(dialog, swHandle, 'sw');
    content.appendChild(swHandle);
  }

  private setupHeaderActions(dialog: HTMLElement): void {
    const header = dialog.querySelector<HTMLElement>('.modal-header');
    if (!header) {
      return;
    }

    if (header.dataset['actionsSetup'] === 'true' && header.querySelector('.erp-modal-maximize-btn')) {
      return;
    }

    header.dataset['actionsSetup'] = 'true';
    header.classList.add('modal-header-draggable');

    const closeBtn = header.querySelector<HTMLElement>('.btn-close, #abp-modal-close-button');

    // Create "Minimize to normal size" button (active when dialog was custom resized via dragging)
    let restoreBtn = header.querySelector<HTMLButtonElement>('.erp-modal-restore-btn');
    if (!restoreBtn) {
      restoreBtn = this.document.createElement('button');
      restoreBtn.type = 'button';
      restoreBtn.className = 'erp-modal-btn erp-modal-restore-btn d-none';
      restoreBtn.setAttribute('aria-label', 'Minimize to normal size');
      restoreBtn.title = 'Minimize to normal size';
      restoreBtn.innerHTML = RESTORE_SVG;

      restoreBtn.addEventListener('click', (e: MouseEvent) => {
        e.stopPropagation();
        this.minimizeToNormal(dialog, maxBtn || undefined, restoreBtn!);
      });
    }

    // Create Maximize / Minimize button
    let maxBtn = header.querySelector<HTMLButtonElement>('.erp-modal-maximize-btn');
    if (!maxBtn) {
      maxBtn = this.document.createElement('button');
      maxBtn.type = 'button';
      maxBtn.className = 'erp-modal-btn erp-modal-maximize-btn';

      const isMaximized = dialog.classList.contains('modal-is-maximized');
      if (isMaximized) {
        maxBtn.innerHTML = RESTORE_SVG;
        maxBtn.title = 'Minimize to normal size (Alt+F11)';
        maxBtn.setAttribute('aria-label', 'Minimize to normal size');
        maxBtn.classList.add('is-maximized');
      } else {
        maxBtn.innerHTML = MAXIMIZE_SVG;
        maxBtn.title = 'Maximize (Alt+F11)';
        maxBtn.setAttribute('aria-label', 'Maximize');
        maxBtn.classList.remove('is-maximized');
      }

      maxBtn.addEventListener('click', (e: MouseEvent) => {
        e.stopPropagation();
        this.toggleMaximize(dialog, maxBtn!, restoreBtn || undefined);
      });
    }

    // If header doesn't already have an ms-auto element, push buttons to the right
    const hasMsAuto = Array.from(header.children).some(
      c => c !== closeBtn && c !== maxBtn && c !== restoreBtn && (c as HTMLElement).classList?.contains('ms-auto'),
    );
    if (!hasMsAuto) {
      restoreBtn.classList.add('ms-auto');
      maxBtn.classList.add('ms-auto');
    }

    // Insert buttons before the close button
    if (closeBtn && closeBtn.parentNode) {
      closeBtn.parentNode.insertBefore(restoreBtn, closeBtn);
      closeBtn.parentNode.insertBefore(maxBtn, closeBtn);
    } else {
      header.appendChild(restoreBtn);
      header.appendChild(maxBtn);
    }

    // Double-click header to toggle maximize / minimize to normal size (similar to Windows)
    header.addEventListener('dblclick', (e: MouseEvent) => {
      const target = e.target as HTMLElement;
      if (
        target.closest(
          'button, input, select, textarea, a, .btn-close, .erp-modal-btn, .modal-resize-handle',
        )
      ) {
        return;
      }
      this.toggleMaximize(dialog, maxBtn!, restoreBtn || undefined);
    });

    // Draggable header to move modal
    this.bindHeaderDrag(dialog, header, restoreBtn);
  }

  /**
   * Toggles modal between Maximized (full screen) and Normal size.
   */
  public toggleMaximize(
    dialog: HTMLElement,
    maxBtn: HTMLButtonElement,
    restoreBtn?: HTMLButtonElement,
  ): void {
    const isMaximized = dialog.classList.contains('modal-is-maximized');

    if (!isMaximized) {
      // Save current state prior to maximizing
      (dialog as unknown as { _savedState: SavedModalState })._savedState = {
        width: dialog.style.width,
        height: dialog.style.height,
        marginLeft: dialog.style.marginLeft,
        marginRight: dialog.style.marginRight,
        marginTop: dialog.style.marginTop,
        marginBottom: dialog.style.marginBottom,
        maxWidth: dialog.style.maxWidth,
        maxHeight: dialog.style.maxHeight,
      };

      dialog.classList.add('modal-is-maximized');
      dialog.style.width = 'calc(100vw - 2rem)';
      dialog.style.height = 'calc(100vh - 2rem)';
      dialog.style.maxWidth = 'calc(100vw - 2rem)';
      dialog.style.maxHeight = 'calc(100vh - 2rem)';
      dialog.style.margin = '1rem auto';
      dialog.style.top = '0';
      dialog.style.left = '0';

      maxBtn.innerHTML = RESTORE_SVG;
      maxBtn.title = 'Minimize to normal size (Alt+F11)';
      maxBtn.setAttribute('aria-label', 'Minimize to normal size');
      maxBtn.classList.add('is-maximized');

      if (restoreBtn) {
        restoreBtn.classList.add('d-none');
      }
    } else {
      // Return to normal size
      this.minimizeToNormal(dialog, maxBtn, restoreBtn);
    }

    // Trigger window resize to notify ngx-datatable and other components to recalculate layout
    if (typeof window !== 'undefined') {
      window.dispatchEvent(new Event('resize'));
    }
  }

  /**
   * Minimizes the modal back to its default, centered normal size.
   */
  public minimizeToNormal(
    dialog: HTMLElement,
    maxBtn?: HTMLButtonElement,
    restoreBtn?: HTMLButtonElement,
  ): void {
    dialog.classList.remove('modal-is-maximized', 'modal-is-custom-resized');
    delete (dialog as unknown as { _savedState?: SavedModalState })._savedState;

    dialog.style.width = '';
    dialog.style.height = '';
    dialog.style.margin = '';
    dialog.style.marginLeft = '';
    dialog.style.marginRight = '';
    dialog.style.marginTop = '';
    dialog.style.marginBottom = '';
    dialog.style.maxWidth = '';
    dialog.style.maxHeight = '';
    dialog.style.top = '';
    dialog.style.left = '';

    const btn = maxBtn || dialog.querySelector<HTMLButtonElement>('.erp-modal-maximize-btn');
    if (btn) {
      btn.innerHTML = MAXIMIZE_SVG;
      btn.title = 'Maximize (Alt+F11)';
      btn.setAttribute('aria-label', 'Maximize');
      btn.classList.remove('is-maximized');
    }

    const resBtn = restoreBtn || dialog.querySelector<HTMLButtonElement>('.erp-modal-restore-btn');
    if (resBtn) {
      resBtn.classList.add('d-none');
    }

    if (typeof window !== 'undefined') {
      window.dispatchEvent(new Event('resize'));
    }
  }

  private bindResizeDrag(
    dialog: HTMLElement,
    handle: HTMLElement,
    direction: 'se' | 'e' | 's' | 'w' | 'sw',
  ): void {
    const onMouseDown = (e: MouseEvent | TouchEvent) => {
      if (dialog.classList.contains('modal-is-maximized')) {
        return;
      }

      e.preventDefault();
      e.stopPropagation();

      const clientX = 'touches' in e ? e.touches[0].clientX : e.clientX;
      const clientY = 'touches' in e ? e.touches[0].clientY : e.clientY;

      const startX = clientX;
      const startY = clientY;

      const rect = dialog.getBoundingClientRect();
      const startWidth = rect.width;
      const startHeight = rect.height;

      // Lock position into explicit margin offset before resizing to prevent jumps
      const currentOffsetLeft = dialog.offsetLeft;
      const currentOffsetTop = dialog.offsetTop;

      dialog.style.marginLeft = `${currentOffsetLeft}px`;
      dialog.style.marginRight = 'auto';
      dialog.style.marginTop = `${currentOffsetTop}px`;
      dialog.style.marginBottom = 'auto';
      dialog.style.width = `${startWidth}px`;
      dialog.style.height = `${startHeight}px`;
      dialog.style.maxWidth = 'none';
      dialog.style.maxHeight = 'none';

      dialog.classList.add('is-resizing');
      this.document.body.classList.add('modal-is-resizing', `modal-is-resizing-${direction}`);

      this.zone.runOutsideAngular(() => {
        const onMouseMove = (moveEvent: MouseEvent | TouchEvent) => {
          const moveX = 'touches' in moveEvent ? moveEvent.touches[0].clientX : moveEvent.clientX;
          const moveY = 'touches' in moveEvent ? moveEvent.touches[0].clientY : moveEvent.clientY;

          const deltaX = moveX - startX;
          const deltaY = moveY - startY;

          const maxAvailWidth = window.innerWidth - currentOffsetLeft - 16;
          const maxAvailHeight = window.innerHeight - currentOffsetTop - 16;

          let newWidth = startWidth;
          let newHeight = startHeight;

          if (direction === 'e' || direction === 'se') {
            newWidth = Math.max(this.minWidth, Math.min(maxAvailWidth, startWidth + deltaX));
            dialog.style.width = `${newWidth}px`;
          }

          if (direction === 's' || direction === 'se' || direction === 'sw') {
            newHeight = Math.max(this.minHeight, Math.min(maxAvailHeight, startHeight + deltaY));
            dialog.style.height = `${newHeight}px`;
          }

          if (direction === 'w' || direction === 'sw') {
            newWidth = Math.max(this.minWidth, startWidth - deltaX);
            const proposedLeft = currentOffsetLeft + (startWidth - newWidth);
            if (proposedLeft >= 8) {
              dialog.style.width = `${newWidth}px`;
              dialog.style.marginLeft = `${proposedLeft}px`;
            }
          }
        };

        const onMouseUp = () => {
          dialog.classList.remove('is-resizing');
          dialog.classList.add('modal-is-custom-resized');
          this.document.body.classList.remove(
            'modal-is-resizing',
            `modal-is-resizing-${direction}`,
          );

          // Show the "Minimize to normal size" button so the user can quickly reset
          const resBtn = dialog.querySelector<HTMLButtonElement>('.erp-modal-restore-btn');
          if (resBtn) {
            resBtn.classList.remove('d-none');
          }

          this.document.removeEventListener('mousemove', onMouseMove);
          this.document.removeEventListener('mouseup', onMouseUp);
          this.document.removeEventListener('touchmove', onMouseMove);
          this.document.removeEventListener('touchend', onMouseUp);

          if (typeof window !== 'undefined') {
            window.dispatchEvent(new Event('resize'));
          }
        };

        this.document.addEventListener('mousemove', onMouseMove);
        this.document.addEventListener('mouseup', onMouseUp);
        this.document.addEventListener('touchmove', onMouseMove, { passive: false });
        this.document.addEventListener('touchend', onMouseUp);
      });
    };

    handle.addEventListener('mousedown', onMouseDown);
    handle.addEventListener('touchstart', onMouseDown, { passive: false });
  }

  private bindHeaderDrag(
    dialog: HTMLElement,
    header: HTMLElement,
    restoreBtn?: HTMLButtonElement,
  ): void {
    const onMouseDown = (e: MouseEvent | TouchEvent) => {
      if (dialog.classList.contains('modal-is-maximized')) {
        return;
      }

      const target = e.target as HTMLElement;
      if (
        target.closest(
          'button, input, select, textarea, a, .btn-close, .erp-modal-btn, .modal-resize-handle',
        )
      ) {
        return;
      }

      e.preventDefault();

      const clientX = 'touches' in e ? e.touches[0].clientX : e.clientX;
      const clientY = 'touches' in e ? e.touches[0].clientY : e.clientY;

      const startX = clientX;
      const startY = clientY;

      const rect = dialog.getBoundingClientRect();
      const currentOffsetLeft = dialog.offsetLeft;
      const currentOffsetTop = dialog.offsetTop;
      const dialogWidth = rect.width;
      const dialogHeight = rect.height;

      dialog.style.marginLeft = `${currentOffsetLeft}px`;
      dialog.style.marginRight = 'auto';
      dialog.style.marginTop = `${currentOffsetTop}px`;
      dialog.style.marginBottom = 'auto';
      dialog.style.width = `${dialogWidth}px`;
      dialog.style.height = `${dialogHeight}px`;
      dialog.style.maxWidth = 'none';
      dialog.style.maxHeight = 'none';

      dialog.classList.add('is-dragging');
      this.document.body.classList.add('modal-is-resizing', 'modal-is-dragging');

      this.zone.runOutsideAngular(() => {
        const onMouseMove = (moveEvent: MouseEvent | TouchEvent) => {
          const moveX = 'touches' in moveEvent ? moveEvent.touches[0].clientX : moveEvent.clientX;
          const moveY = 'touches' in moveEvent ? moveEvent.touches[0].clientY : moveEvent.clientY;

          const deltaX = moveX - startX;
          const deltaY = moveY - startY;

          const newLeft = Math.max(
            0,
            Math.min(window.innerWidth - dialogWidth, currentOffsetLeft + deltaX),
          );
          const newTop = Math.max(
            0,
            Math.min(window.innerHeight - 50, currentOffsetTop + deltaY),
          );

          dialog.style.marginLeft = `${newLeft}px`;
          dialog.style.marginTop = `${newTop}px`;
        };

        const onMouseUp = () => {
          dialog.classList.remove('is-dragging');
          dialog.classList.add('modal-is-custom-resized');
          this.document.body.classList.remove('modal-is-resizing', 'modal-is-dragging');

          const resBtn = restoreBtn || dialog.querySelector<HTMLButtonElement>('.erp-modal-restore-btn');
          if (resBtn) {
            resBtn.classList.remove('d-none');
          }

          this.document.removeEventListener('mousemove', onMouseMove);
          this.document.removeEventListener('mouseup', onMouseUp);
          this.document.removeEventListener('touchmove', onMouseMove);
          this.document.removeEventListener('touchend', onMouseUp);
        };

        this.document.addEventListener('mousemove', onMouseMove);
        this.document.addEventListener('mouseup', onMouseUp);
        this.document.addEventListener('touchmove', onMouseMove, { passive: false });
        this.document.addEventListener('touchend', onMouseUp);
      });
    };

    header.addEventListener('mousedown', onMouseDown);
    header.addEventListener('touchstart', onMouseDown, { passive: false });
  }
}

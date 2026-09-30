import { TestBed } from '@angular/core/testing';
import { ModalResizeService } from './modal-resize.service';

describe('ModalResizeService', () => {
  let service: ModalResizeService;
  let container: HTMLDivElement;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ModalResizeService],
    });
    service = TestBed.inject(ModalResizeService);

    container = document.createElement('div');
    document.body.appendChild(container);
  });

  afterEach(() => {
    service.destroy();
    if (container && container.parentNode) {
      container.parentNode.removeChild(container);
    }
  });

  it('should be created and initialize cleanly', () => {
    expect(service).toBeTruthy();
    service.init();
    expect(service).toBeTruthy();
  });

  it('should enhance a modal dialog with resize handles and maximize/minimize buttons', () => {
    const dialog = document.createElement('div');
    dialog.className = 'modal-dialog';

    const content = document.createElement('div');
    content.className = 'modal-content';

    const header = document.createElement('div');
    header.className = 'modal-header';

    const closeBtn = document.createElement('button');
    closeBtn.className = 'btn-close';
    header.appendChild(closeBtn);

    const body = document.createElement('div');
    body.className = 'modal-body';

    content.appendChild(header);
    content.appendChild(body);
    dialog.appendChild(content);
    container.appendChild(dialog);

    service.enhanceModal(dialog);

    expect(dialog.classList.contains('modal-dialog-resizable')).toBeTrue();
    expect(dialog.dataset['resizable']).toBe('true');

    // Check handles
    const seHandle = content.querySelector('.modal-resize-handle--se');
    expect(seHandle).toBeTruthy();

    const eHandle = content.querySelector('.modal-resize-handle--e');
    expect(eHandle).toBeTruthy();

    const sHandle = content.querySelector('.modal-resize-handle--s');
    expect(sHandle).toBeTruthy();

    // Check maximize button
    const maxBtn = header.querySelector<HTMLButtonElement>('.erp-modal-maximize-btn');
    expect(maxBtn).toBeTruthy();
    expect(maxBtn?.title).toContain('Maximize');
    expect(maxBtn?.getAttribute('aria-label')).toBe('Maximize');

    // Click maximize
    maxBtn?.click();
    expect(dialog.classList.contains('modal-is-maximized')).toBeTrue();
    expect(maxBtn?.title).toContain('Minimize to normal size');
    expect(maxBtn?.getAttribute('aria-label')).toBe('Minimize to normal size');

    // Click minimize to normal size
    maxBtn?.click();
    expect(dialog.classList.contains('modal-is-maximized')).toBeFalse();
    expect(maxBtn?.title).toContain('Maximize');
    expect(maxBtn?.getAttribute('aria-label')).toBe('Maximize');
    expect(dialog.style.width).toBe('');
    expect(dialog.style.height).toBe('');
  });

  it('should support late-rendered modal header and attach maximize/minimize actions', () => {
    const dialog = document.createElement('div');
    dialog.className = 'modal-dialog';

    const content = document.createElement('div');
    content.className = 'modal-content';
    dialog.appendChild(content);
    container.appendChild(dialog);

    // Initial pass when header not yet rendered (e.g., in ng-template or NgbModal)
    service.enhanceModal(dialog);
    expect(dialog.dataset['resizable']).toBe('true');

    // Now Angular renders the header
    const header = document.createElement('div');
    header.className = 'modal-header';
    const closeBtn = document.createElement('button');
    closeBtn.className = 'btn-close';
    header.appendChild(closeBtn);
    content.appendChild(header);

    // Subsequent enhance call (e.g. from MutationObserver)
    service.enhanceExistingModals();

    const maxBtn = header.querySelector<HTMLButtonElement>('.erp-modal-maximize-btn');
    expect(maxBtn).toBeTruthy();
    expect(maxBtn?.title).toContain('Maximize');
  });

  it('should minimize to normal size when custom resized via restore button', () => {
    const dialog = document.createElement('div');
    dialog.className = 'modal-dialog';

    const content = document.createElement('div');
    content.className = 'modal-content';

    const header = document.createElement('div');
    header.className = 'modal-header';

    const closeBtn = document.createElement('button');
    closeBtn.className = 'btn-close';
    header.appendChild(closeBtn);

    content.appendChild(header);
    dialog.appendChild(content);
    container.appendChild(dialog);

    service.enhanceModal(dialog);

    const restoreBtn = header.querySelector<HTMLButtonElement>('.erp-modal-restore-btn');
    expect(restoreBtn).toBeTruthy();

    // Simulate custom resize
    dialog.style.width = '880px';
    dialog.style.height = '600px';
    dialog.classList.add('modal-is-custom-resized');
    restoreBtn?.classList.remove('d-none');

    // Click restore
    restoreBtn?.click();

    expect(dialog.classList.contains('modal-is-custom-resized')).toBeFalse();
    expect(dialog.classList.contains('modal-is-maximized')).toBeFalse();
    expect(dialog.style.width).toBe('');
    expect(dialog.style.height).toBe('');
  });

  it('should toggle maximize and minimize to normal size via Alt+F11 shortcut', () => {
    service.init();

    const modalWrapper = document.createElement('div');
    modalWrapper.className = 'modal show';

    const dialog = document.createElement('div');
    dialog.className = 'modal-dialog';

    const content = document.createElement('div');
    content.className = 'modal-content';

    const header = document.createElement('div');
    header.className = 'modal-header';

    const closeBtn = document.createElement('button');
    closeBtn.className = 'btn-close';
    header.appendChild(closeBtn);

    content.appendChild(header);
    dialog.appendChild(content);
    modalWrapper.appendChild(dialog);
    container.appendChild(modalWrapper);

    service.enhanceModal(dialog);

    // Trigger Alt+F11 keydown
    const event = new KeyboardEvent('keydown', {
      key: 'F11',
      altKey: true,
      bubbles: true,
      cancelable: true,
    });
    document.dispatchEvent(event);

    expect(dialog.classList.contains('modal-is-maximized')).toBeTrue();

    // Trigger Alt+F11 again
    document.dispatchEvent(event);
    expect(dialog.classList.contains('modal-is-maximized')).toBeFalse();
  });
});

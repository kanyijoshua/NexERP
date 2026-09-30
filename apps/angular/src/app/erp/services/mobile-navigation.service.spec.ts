import { TestBed } from '@angular/core/testing';
import { MobileNavigationService } from './mobile-navigation.service';

describe('MobileNavigationService', () => {
  let service: MobileNavigationService;
  let navContainer: HTMLDivElement;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MobileNavigationService],
    });
    service = TestBed.inject(MobileNavigationService);

    navContainer = document.createElement('div');
    navContainer.innerHTML = `
      <nav id="main-navbar">
        <button class="navbar-toggler" aria-expanded="false"></button>
        <div id="main-navbar-collapse">
          <div class="abp-collapse-margin abp-collapse-margin-collapsed">
            <ul class="navbar-nav">
              <li class="nav-item"><a class="nav-link" href="#">Home</a></li>
            </ul>
          </div>
        </div>
      </nav>
    `;
    document.body.appendChild(navContainer);
  });

  afterEach(() => {
    service.destroy();
    if (navContainer && navContainer.parentNode) {
      navContainer.parentNode.removeChild(navContainer);
    }
  });

  it('should initialize cleanly and enhance navbar collapse with header', () => {
    service.init();
    const collapse = document.querySelector('#main-navbar-collapse');
    expect(collapse).toBeTruthy();

    const drawerHeader = collapse?.querySelector('.erp-mobile-sidenav-header');
    expect(drawerHeader).toBeTruthy();
    expect(drawerHeader?.textContent).toContain('NexERP');
  });

  it('should click toggler when mobile drawer close button is clicked', () => {
    service.init();
    const toggler = document.querySelector<HTMLButtonElement>('#main-navbar .navbar-toggler');
    const closeBtn = document.querySelector<HTMLButtonElement>('.erp-mobile-sidenav-close');

    expect(toggler).toBeTruthy();
    expect(closeBtn).toBeTruthy();

    let clicked = false;
    toggler?.addEventListener('click', () => {
      clicked = true;
    });

    closeBtn?.click();
    expect(clicked).toBeTrue();
  });
});

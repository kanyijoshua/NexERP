import { DefaultQueueManager, QUEUE_MANAGER } from '@abp/ng.core';
import { CoreTestingModule } from '@abp/ng.core/testing';
import { ToasterService } from '@abp/ng.theme.shared';
import { ThemeSharedTestingModule } from '@abp/ng.theme.shared/testing';
import { NO_ERRORS_SCHEMA, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MyProfileDto, ProfileSource } from '@proxy/profiles';
import { of } from 'rxjs';
import { RoleNavigationService } from '../../services/role-navigation.service';
import { CompanyMenuComponent } from '../company-menu/company-menu.component';
import { MenuSuiteService } from '../menu-suite/menu-suite.service';
import { RoleNavBarComponent } from './role-nav-bar.component';

const ACCOUNTANT: MyProfileDto = {
  profileId: 'ACCOUNTANT',
  displayName: 'Accountant',
  source: ProfileSource.Role,
  roleName: 'finance',
  navigation: [
    { key: '/erp/chart-of-accounts', displayName: 'Chart of Accounts', route: '/erp/chart-of-accounts', children: [] },
    {
      key: 'journals',
      displayName: 'Journals',
      children: [
        { key: '/erp/finance/general-journal', displayName: 'General Journals', route: '/erp/finance/general-journal', children: [] },
      ],
    },
  ],
  profiles: [
    { id: 'BUSINESS_MANAGER', displayName: 'Business Manager', description: 'Runs the company' },
    { id: 'ACCOUNTANT', displayName: 'Accountant', description: 'Keeps the books' },
  ],
};

describe('RoleNavBarComponent', () => {
  let fixture: ComponentFixture<RoleNavBarComponent>;
  let navigation: { profile: ReturnType<typeof signal<MyProfileDto | null>>; start: jasmine.Spy; change: jasmine.Spy; reload: jasmine.Spy };
  let menuSuite: jasmine.SpyObj<MenuSuiteService>;
  let toaster: jasmine.SpyObj<ToasterService>;

  beforeEach(async () => {
    navigation = {
      profile: signal<MyProfileDto | null>(ACCOUNTANT),
      start: jasmine.createSpy('start'),
      change: jasmine.createSpy('change').and.returnValue(of({ ...ACCOUNTANT, displayName: 'Business Manager' })),
      reload: jasmine.createSpy('reload'),
    };
    menuSuite = jasmine.createSpyObj('MenuSuiteService', ['open']);
    toaster = jasmine.createSpyObj('ToasterService', ['success', 'error', 'warn', 'info']);

    await TestBed.configureTestingModule({
      imports: [CoreTestingModule.withConfig(), ThemeSharedTestingModule.withConfig(), RoleNavBarComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: RoleNavigationService, useValue: navigation },
        { provide: MenuSuiteService, useValue: menuSuite },
        { provide: ToasterService, useValue: toaster },
        // Needed by *abpPermission; provideAbpCore() registers it, CoreTestingModule does not.
        { provide: QUEUE_MANAGER, useClass: DefaultQueueManager },
      ],
    })
      // The company menu has its own spec and loads companies; it is not under test here.
      .overrideComponent(RoleNavBarComponent, {
        remove: { imports: [CompanyMenuComponent] },
        add: { schemas: [NO_ERRORS_SCHEMA] },
      })
      .compileComponents();

    fixture = TestBed.createComponent(RoleNavBarComponent);
    fixture.detectChanges();
  });

  const text = () => (fixture.nativeElement as HTMLElement).textContent ?? '';

  it('starts loading the profile when it is drawn', () => {
    expect(navigation.start).toHaveBeenCalled();
  });

  it('shows the profile as links and menus, and the role it works as', () => {
    const items = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('.role-bar__item'));

    expect(items.map(i => i.textContent?.trim())).toEqual(['Chart of Accounts', 'Journals']);
    expect(items[0].tagName).toBe('A');
    expect(items[0].getAttribute('href')).toBe('/erp/chart-of-accounts');
    // A menu is a button that opens its links, not a link itself.
    expect(items[1].tagName).toBe('BUTTON');
    expect(text()).toContain('Accountant');
  });

  it('shows nothing to navigate while the profile is not loaded', () => {
    navigation.profile.set(null);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).querySelectorAll('.role-bar__item').length).toBe(0);
    expect((fixture.nativeElement as HTMLElement).querySelector('.role-bar__role')).toBeNull();
  });

  it('changes the role and says so', () => {
    fixture.componentInstance.changeRole('BUSINESS_MANAGER');

    expect(navigation.change).toHaveBeenCalledWith('BUSINESS_MANAGER');
    expect(toaster.success).toHaveBeenCalled();
    expect(fixture.componentInstance.busy()).toBeFalse();
  });

  it('opens the full menu from the menu button', () => {
    fixture.componentInstance.openMenuSuite();

    expect(menuSuite.open).toHaveBeenCalled();
  });
});

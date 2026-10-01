import { ConfigStateService } from '@abp/ng.core';
import { TestBed } from '@angular/core/testing';
import { MyProfileDto, ProfileService, ProfileSource } from '@proxy/profiles';
import { BehaviorSubject, Subject, of, throwError } from 'rxjs';
import { CompanyService } from './company.service';
import { RoleNavigationService } from './role-navigation.service';

const profile = (id: string): MyProfileDto => ({
  profileId: id,
  displayName: id,
  source: ProfileSource.Default,
  navigation: [],
  profiles: [],
});

describe('RoleNavigationService', () => {
  let user$: BehaviorSubject<{ isAuthenticated: boolean }>;
  let companyChanged$: Subject<string>;
  let profiles: jasmine.SpyObj<ProfileService>;
  let service: RoleNavigationService;

  beforeEach(() => {
    user$ = new BehaviorSubject({ isAuthenticated: false });
    companyChanged$ = new Subject<string>();
    profiles = jasmine.createSpyObj('ProfileService', ['getMy', 'setMy']);
    profiles.getMy.and.returnValue(of(profile('BUSINESS_MANAGER')));

    TestBed.configureTestingModule({
      providers: [
        { provide: ConfigStateService, useValue: { getOne$: () => user$ } },
        { provide: CompanyService, useValue: { companyChanged$ } },
        { provide: ProfileService, useValue: profiles },
      ],
    });
    service = TestBed.inject(RoleNavigationService);
  });

  it('loads nothing until someone signs in, then loads their profile', () => {
    service.start();
    expect(profiles.getMy).not.toHaveBeenCalled();
    expect(service.profile()).toBeNull();

    user$.next({ isAuthenticated: true });

    expect(service.profile()?.profileId).toBe('BUSINESS_MANAGER');
  });

  it('loads again when the company changes, since modules are on or off per company', () => {
    user$.next({ isAuthenticated: true });
    service.start();
    profiles.getMy.and.returnValue(of(profile('ACCOUNTANT')));

    companyChanged$.next('other');

    expect(profiles.getMy).toHaveBeenCalledTimes(2);
    expect(service.profile()?.profileId).toBe('ACCOUNTANT');
  });

  it('starts once however many bars ask', () => {
    user$.next({ isAuthenticated: true });
    service.start();
    service.start();

    expect(profiles.getMy).toHaveBeenCalledTimes(1);
  });

  it('keeps listening after a failed load', () => {
    profiles.getMy.and.returnValue(throwError(() => new Error('down')));
    user$.next({ isAuthenticated: true });
    service.start();
    expect(service.profile()).toBeNull();

    profiles.getMy.and.returnValue(of(profile('ACCOUNTANT')));
    companyChanged$.next('other');

    expect(service.profile()?.profileId).toBe('ACCOUNTANT');
  });

  it('forgets the profile on sign-out', () => {
    user$.next({ isAuthenticated: true });
    service.start();

    user$.next({ isAuthenticated: false });

    expect(service.profile()).toBeNull();
  });

  it('takes the profile the server sends back after a change', () => {
    profiles.setMy.and.returnValue(of(profile('PURCHASING_AGENT')));

    service.change('PURCHASING_AGENT').subscribe();

    expect(profiles.setMy).toHaveBeenCalledWith({ profileId: 'PURCHASING_AGENT' });
    expect(service.profile()?.profileId).toBe('PURCHASING_AGENT');
  });
});

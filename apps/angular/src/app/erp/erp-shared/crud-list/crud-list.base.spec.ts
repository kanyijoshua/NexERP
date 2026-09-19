import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { CoreTestingModule } from '@abp/ng.core/testing';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { ThemeSharedTestingModule } from '@abp/ng.theme.shared/testing';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed, fakeAsync, flush, tick, waitForAsync } from '@angular/core/testing';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, Subject, of } from 'rxjs';
import { CompanyService } from '../../services/company.service';
import { CrudListBase } from './crud-list.base';

interface TestDto {
  id?: string;
  name: string;
}

interface TestInput {
  name: string;
}

@Component({ selector: 'erp-test-crud-list', template: '' })
class TestCrudListComponent extends CrudListBase<TestDto, TestInput> {
  getListSpy = jasmine.createSpy('getList').and.returnValue(of({ items: [], totalCount: 0 }));
  createSpy = jasmine.createSpy('create').and.returnValue(of({ id: '1', name: 'A' }));
  updateSpy = jasmine.createSpy('update').and.returnValue(of({ id: '1', name: 'B' }));
  deleteSpy = jasmine.createSpy('delete').and.returnValue(of(undefined));

  protected getList(query: ABP.PageQueryParams): Observable<PagedResultDto<TestDto>> {
    return this.getListSpy(query);
  }
  protected buildForm(item?: TestDto): FormGroup {
    return new FormGroup({ name: new FormControl(item?.name ?? '', Validators.required) });
  }
  protected create(input: TestInput): Observable<unknown> {
    return this.createSpy(input);
  }
  protected update(id: string, input: TestInput): Observable<unknown> {
    return this.updateSpy(id, input);
  }
  protected delete(id: string): Observable<unknown> {
    return this.deleteSpy(id);
  }
}

describe('CrudListBase', () => {
  let fixture: ComponentFixture<TestCrudListComponent>;
  let component: TestCrudListComponent;
  let confirmation$: Subject<Confirmation.Status>;
  let companyChanged$: Subject<string>;

  const mockList = {
    page: 0,
    filter: '',
    get: jasmine.createSpy('get'),
    hookToQuery: jasmine
      .createSpy('hookToQuery')
      .and.callFake((callback: (query: ABP.PageQueryParams) => Observable<unknown>) =>
        callback({ skipCount: 0, maxResultCount: 10 }),
      ),
  };
  const mockToaster = jasmine.createSpyObj('ToasterService', ['success']);
  const mockConfirmation = jasmine.createSpyObj('ConfirmationService', ['warn']);

  beforeEach(waitForAsync(() => {
    confirmation$ = new Subject<Confirmation.Status>();
    companyChanged$ = new Subject<string>();
    mockList.page = 0;
    mockList.filter = '';
    mockList.get.calls.reset();
    mockToaster.success.calls.reset();
    mockConfirmation.warn.calls.reset();
    mockConfirmation.warn.and.returnValue(confirmation$);

    TestBed.configureTestingModule({
      declarations: [TestCrudListComponent],
      imports: [CoreTestingModule.withConfig(), ThemeSharedTestingModule.withConfig()],
      providers: [
        { provide: ListService, useValue: mockList },
        { provide: ToasterService, useValue: mockToaster },
        { provide: ConfirmationService, useValue: mockConfirmation },
        { provide: CompanyService, useValue: { companyChanged$ } },
      ],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TestCrudListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('hooks the list service to getList and exposes the result', () => {
    expect(mockList.hookToQuery).toHaveBeenCalled();
    expect(component.getListSpy).toHaveBeenCalled();
    expect(component.data).toEqual({ items: [], totalCount: 0 });
  });

  it('save() does nothing but mark the form as touched when it is invalid', () => {
    component.openCreate();
    component.save();

    expect(component.createSpy).not.toHaveBeenCalled();
    expect(component.form.get('name')?.touched).toBeTrue();
    expect(component.isModalOpen).toBeTrue();
    expect(mockToaster.success).not.toHaveBeenCalled();
    expect(mockList.get).not.toHaveBeenCalled();
  });

  it('save() creates, closes the modal, toasts and refreshes when the form is valid', () => {
    component.openCreate();
    component.form.setValue({ name: 'Acme' });
    component.save();

    expect(component.createSpy).toHaveBeenCalledOnceWith({ name: 'Acme' });
    expect(component.updateSpy).not.toHaveBeenCalled();
    expect(component.isModalOpen).toBeFalse();
    expect(component.isBusy).toBeFalse();
    expect(mockToaster.success).toHaveBeenCalledOnceWith('Erp::SavedSuccessfully');
    expect(mockList.get).toHaveBeenCalledTimes(1);
  });

  it('save() updates when a row is being edited', () => {
    component.openEdit({ id: '42', name: 'Old' });
    expect(component.selected).toEqual({ id: '42', name: 'Old' });
    expect(component.form.value).toEqual({ name: 'Old' });

    component.form.setValue({ name: 'New' });
    component.save();

    expect(component.updateSpy).toHaveBeenCalledOnceWith('42', { name: 'New' });
    expect(component.createSpy).not.toHaveBeenCalled();
    expect(component.isModalOpen).toBeFalse();
  });

  it('save() keeps the modal open, clears busy and does not swallow the error when the request fails', fakeAsync(() => {
    const failing$ = new Subject<unknown>();
    component.createSpy.and.returnValue(failing$);
    component.openCreate();
    component.form.setValue({ name: 'Acme' });
    component.save();
    expect(component.isBusy).toBeTrue();

    // The error is left to ABP's HTTP error handler: it must surface as unhandled, not be swallowed.
    failing$.error(new Error('boom'));
    expect(() => flush()).toThrowError('boom');

    expect(component.isBusy).toBeFalse();
    expect(component.isModalOpen).toBeTrue();
    expect(mockToaster.success).not.toHaveBeenCalled();
  }));

  it('remove() deletes only after the confirmation is confirmed', () => {
    component.remove({ id: '7', name: 'X' });
    expect(mockConfirmation.warn).toHaveBeenCalledOnceWith(
      'Erp::ItemWillBeDeletedMessage',
      'Erp::AreYouSure',
    );
    expect(component.deleteSpy).not.toHaveBeenCalled();

    confirmation$.next(Confirmation.Status.confirm);

    expect(component.deleteSpy).toHaveBeenCalledOnceWith('7');
    expect(mockToaster.success).toHaveBeenCalledOnceWith('Erp::DeletedSuccessfully');
    expect(mockList.get).toHaveBeenCalledTimes(1);
  });

  it('remove() does not delete when the confirmation is rejected or dismissed', () => {
    component.remove({ id: '7', name: 'X' });
    confirmation$.next(Confirmation.Status.reject);
    component.remove({ id: '7', name: 'X' });
    confirmation$.next(Confirmation.Status.dismiss);

    expect(component.deleteSpy).not.toHaveBeenCalled();
    expect(mockList.get).not.toHaveBeenCalled();
  });

  it('debounces the filter, resets the page and pushes it to the list service', fakeAsync(() => {
    mockList.page = 3;
    component.filter = 'a';
    component.filter = 'ab';
    tick(299);
    expect(mockList.filter).toBe('');

    tick(1);
    expect(mockList.filter).toBe('ab');
    expect(mockList.page).toBe(0);
    expect(component.filter).toBe('ab');
  }));

  it('re-queries when the active company changes', () => {
    companyChanged$.next('company-2');
    expect(mockList.get).toHaveBeenCalledTimes(1);
  });
});

import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { of } from 'rxjs';
import { ErpButtonBase } from './erp-button.base';
import { ErpButtonComponent } from './erp-button.component';
import { ErpSplitButtonComponent } from './erp-split-button.component';
import { ErpButtonGroupComponent } from './erp-button-group.component';
import { ErpButtonAction } from './erp-button.models';

// Test subclass proving inheritance from ErpButtonBase
class CustomPostButton extends ErpButtonBase {
  constructor() {
    super();
    this.variant = 'success';
    this.icon = 'fas fa-paper-plane';
    this.confirm = true;
    this.confirmTitle = 'Post Ledger';
  }
}

describe('ErpButtonComponent & ErpButtonBase', () => {
  let fixture: ComponentFixture<ErpButtonComponent>;
  let component: ErpButtonComponent;
  let confirmationService: jasmine.SpyObj<ConfirmationService>;
  let permissionService: jasmine.SpyObj<PermissionService>;

  beforeEach(async () => {
    confirmationService = jasmine.createSpyObj('ConfirmationService', ['warn']);
    permissionService = jasmine.createSpyObj('PermissionService', ['getGrantedPolicy']);
    permissionService.getGrantedPolicy.and.callFake((p: string) => p !== 'Denied');

    await TestBed.configureTestingModule({
      declarations: [
        ErpButtonComponent,
        ErpSplitButtonComponent,
        ErpButtonGroupComponent,
      ],
      imports: [NgbDropdownModule],
      providers: [
        { provide: ConfirmationService, useValue: confirmationService },
        { provide: PermissionService, useValue: permissionService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ErpButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('allows custom buttons to inherit from ErpButtonBase', () => {
    const custom = TestBed.runInInjectionContext(() => new CustomPostButton());
    expect(custom.variant).toBe('success');
    expect(custom.icon).toBe('fas fa-paper-plane');
    expect(custom.confirm).toBeTrue();
    expect(custom.buttonCssClass).toContain('btn-success');
  });

  it('emits btnClick when clicked in normal state', () => {
    spyOn(component.btnClick, 'emit');
    const mockEvent = new MouseEvent('click');

    component.handleClick(mockEvent);

    expect(component.btnClick.emit).toHaveBeenCalledWith(mockEvent);
  });

  it('prevents click when button is disabled', () => {
    component.disabled = true;
    spyOn(component.btnClick, 'emit');
    const mockEvent = jasmine.createSpyObj('MouseEvent', ['preventDefault', 'stopPropagation']);

    component.handleClick(mockEvent);

    expect(mockEvent.preventDefault).toHaveBeenCalled();
    expect(component.btnClick.emit).not.toHaveBeenCalled();
  });

  it('prevents click when button is loading', () => {
    component.loading = true;
    spyOn(component.btnClick, 'emit');
    const mockEvent = jasmine.createSpyObj('MouseEvent', ['preventDefault', 'stopPropagation']);

    component.handleClick(mockEvent);

    expect(mockEvent.preventDefault).toHaveBeenCalled();
    expect(component.btnClick.emit).not.toHaveBeenCalled();
  });

  it('evaluates ABP permissions and hides/blocks when permission is denied', () => {
    component.permission = 'Denied';
    expect(component.isGranted).toBeFalse();

    spyOn(component.btnClick, 'emit');
    const mockEvent = jasmine.createSpyObj('MouseEvent', ['preventDefault', 'stopPropagation']);
    component.handleClick(mockEvent);

    expect(component.btnClick.emit).not.toHaveBeenCalled();
  });

  it('prompts confirmation when confirm is true and emits only on confirm', () => {
    component.confirm = true;
    confirmationService.warn.and.returnValue(of(Confirmation.Status.confirm));

    spyOn(component.btnClick, 'emit');
    const mockEvent = jasmine.createSpyObj('MouseEvent', ['preventDefault', 'stopPropagation']);

    component.handleClick(mockEvent);

    expect(confirmationService.warn).toHaveBeenCalled();
    expect(component.btnClick.emit).toHaveBeenCalledWith(mockEvent);
  });

  it('does not emit if user dismisses confirmation', () => {
    component.confirm = true;
    confirmationService.warn.and.returnValue(of(Confirmation.Status.dismiss));

    spyOn(component.btnClick, 'emit');
    const mockEvent = jasmine.createSpyObj('MouseEvent', ['preventDefault', 'stopPropagation']);

    component.handleClick(mockEvent);

    expect(confirmationService.warn).toHaveBeenCalled();
    expect(component.btnClick.emit).not.toHaveBeenCalled();
  });

  it('executes split button secondary action', () => {
    const splitFixture = TestBed.createComponent(ErpSplitButtonComponent);
    const splitComp = splitFixture.componentInstance;

    const actionSpy = jasmine.createSpy('action');
    const testAction: ErpButtonAction = {
      key: 'postAndPrint',
      label: 'Post & Print',
      action: actionSpy,
    };
    splitComp.actions = [testAction];
    splitFixture.detectChanges();

    spyOn(splitComp.actionClick, 'emit');
    const mockEvent = new MouseEvent('click');
    splitComp.onActionItemClick(testAction, mockEvent);

    expect(actionSpy).toHaveBeenCalledWith(mockEvent);
    expect(splitComp.actionClick.emit).toHaveBeenCalledWith(testAction);
  });
});

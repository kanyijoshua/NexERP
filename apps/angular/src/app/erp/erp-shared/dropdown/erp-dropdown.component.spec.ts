import { Component, ElementRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { ErpDropdownBase } from './erp-dropdown.base';
import { ErpDropdownComponent } from './erp-dropdown.component';
import { ErpActionDropdownComponent, ErpActionDropdownItem } from './erp-action-dropdown.component';
import { ErpDropdownOption } from './erp-dropdown.models';
import { PermissionService } from '@abp/ng.core';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';

// Test class proving inheritance from ErpDropdownBase
class CustomStatusDropdown extends ErpDropdownBase<string> {
  constructor() {
    super();
    this.options = [
      { value: 'draft', label: 'Draft' },
      { value: 'active', label: 'Active' },
      { value: 'closed', label: 'Closed' },
    ];
  }
}

describe('ErpDropdownComponent & ErpDropdownBase', () => {
  let fixture: ComponentFixture<ErpDropdownComponent>;
  let component: ErpDropdownComponent;

  const sampleOptions: ErpDropdownOption<string>[] = [
    { value: 'usd', label: 'USD - US Dollar', icon: 'fas fa-dollar-sign' },
    { value: 'eur', label: 'EUR - Euro', icon: 'fas fa-euro-sign' },
    { value: 'gbp', label: 'GBP - British Pound', disabled: true },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ErpDropdownComponent, ErpActionDropdownComponent],
      imports: [FormsModule, NgbDropdownModule],
      providers: [
        {
          provide: PermissionService,
          useValue: { getGrantedPolicy: (policy: string) => policy !== 'Denied' },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ErpDropdownComponent);
    component = fixture.componentInstance;
    component.options = [...sampleOptions];
    fixture.detectChanges();
  });

  it('allows subclasses to inherit ErpDropdownBase cleanly', () => {
    const custom = new CustomStatusDropdown();
    expect(custom.options.length).toBe(3);
    expect(custom.displayLabel).toBe('Select option...');

    custom.selectOption(custom.options[1]);
    expect(custom.selectedOption?.value).toBe('active');
    expect(custom.displayLabel).toBe('Active');
  });

  it('toggles open and close state on trigger click', () => {
    expect(component.isOpen).toBeFalse();

    component.toggleDropdown();
    expect(component.isOpen).toBeTrue();

    component.toggleDropdown();
    expect(component.isOpen).toBeFalse();
  });

  it('selects single option, emits selectionChange, and closes dropdown', () => {
    spyOn(component.selectionChange, 'emit');

    component.openDropdown();
    component.selectOption(sampleOptions[0]);

    expect(component.selectedOption?.value).toBe('usd');
    expect(component.selectionChange.emit).toHaveBeenCalledWith('usd');
    expect(component.isOpen).toBeFalse();
    expect(component.displayLabel).toBe('USD - US Dollar');
  });

  it('does not select disabled options', () => {
    spyOn(component.selectionChange, 'emit');

    component.selectOption(sampleOptions[2]); // disabled option
    expect(component.selectedOption).toBeNull();
    expect(component.selectionChange.emit).not.toHaveBeenCalled();
  });

  it('supports multi-select with add and remove chips', () => {
    component.isMulti = true;
    spyOn(component.selectionChange, 'emit');

    component.selectOption(sampleOptions[0]);
    expect(component.selectedOptions.length).toBe(1);
    expect(component.isSelected(sampleOptions[0])).toBeTrue();

    component.selectOption(sampleOptions[1]);
    expect(component.selectedOptions.length).toBe(2);
    expect(component.displayLabel).toBe('2 selected');

    // Remove option
    component.removeOption(sampleOptions[0]);
    expect(component.selectedOptions.length).toBe(1);
    expect(component.isSelected(sampleOptions[0])).toBeFalse();
  });

  it('clears selection when clearSelection is called', () => {
    component.selectOption(sampleOptions[0]);
    expect(component.selectedOption).not.toBeNull();

    component.clearSelection();
    expect(component.selectedOption).toBeNull();
    expect(component.displayLabel).toBe(component.placeholder);
  });

  it('filters options by search query', () => {
    component.onSearchInput('Euro');
    expect(component.filteredOptions.length).toBe(1);
    expect(component.filteredOptions[0].value).toBe('eur');

    component.onSearchInput('xyz');
    expect(component.filteredOptions.length).toBe(0);
  });

  it('navigates with keyboard ArrowDown, ArrowUp, Enter and Escape', () => {
    component.openDropdown();
    expect(component.isOpen).toBeTrue();

    // Arrow down moves highlight
    component.onKeyDown(new KeyboardEvent('keydown', { key: 'ArrowDown' }));
    expect(component.activeHighlightIndex).toBe(0);

    component.onKeyDown(new KeyboardEvent('keydown', { key: 'ArrowDown' }));
    expect(component.activeHighlightIndex).toBe(1);

    // Enter selects highlighted
    component.onKeyDown(new KeyboardEvent('keydown', { key: 'Enter' }));
    expect(component.selectedOption?.value).toBe('eur');

    // Escape closes
    component.openDropdown();
    component.onKeyDown(new KeyboardEvent('keydown', { key: 'Escape' }));
    expect(component.isOpen).toBeFalse();
  });

  it('implements ControlValueAccessor correctly', () => {
    const changeSpy = jasmine.createSpy('change');
    const touchSpy = jasmine.createSpy('touch');
    component.registerOnChange(changeSpy);
    component.registerOnTouched(touchSpy);

    component.writeValue('eur');
    expect(component.selectedOption?.value).toBe('eur');

    component.setDisabledState(true);
    expect(component.disabled).toBeTrue();
  });

  it('closes when clicking outside host element', () => {
    component.openDropdown();
    expect(component.isOpen).toBeTrue();

    const outsideEl = document.createElement('div');
    document.body.appendChild(outsideEl);
    outsideEl.click();

    component.onDocumentClick({ target: outsideEl } as unknown as MouseEvent);
    expect(component.isOpen).toBeFalse();

    document.body.removeChild(outsideEl);
  });

  it('executes actions in ErpActionDropdownComponent', () => {
    const actionFixture = TestBed.createComponent(ErpActionDropdownComponent);
    const actionComp = actionFixture.componentInstance;
    const testAction: ErpActionDropdownItem = {
      key: 'export',
      label: 'Export to Excel',
      action: jasmine.createSpy('action'),
    };
    actionComp.actions = [testAction];
    actionFixture.detectChanges();

    spyOn(actionComp.actionClick, 'emit');
    actionComp.handleAction(testAction);

    expect(testAction.action).toHaveBeenCalled();
    expect(actionComp.actionClick.emit).toHaveBeenCalledWith(testAction);
  });
});

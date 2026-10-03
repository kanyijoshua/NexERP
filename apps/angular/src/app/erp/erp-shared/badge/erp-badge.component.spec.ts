import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ErpBadgeComponent } from './erp-badge.component';

describe('ErpBadgeComponent', () => {
  let fixture: ComponentFixture<ErpBadgeComponent>;
  let component: ErpBadgeComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ErpBadgeComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ErpBadgeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('computes display label from status or text override', () => {
    component.status = 'Draft';
    expect(component.displayLabel).toBe('Draft');

    component.text = 'Custom Label';
    expect(component.displayLabel).toBe('Custom Label');

    component.text = undefined;
    component.status = true;
    expect(component.displayLabel).toBe('Yes');

    component.status = false;
    expect(component.displayLabel).toBe('No');
  });

  it('auto-resolves tones from ERP status keywords', () => {
    component.status = 'Active';
    expect(component.computedTone).toBe('success');

    component.status = 'Posted';
    expect(component.computedTone).toBe('success');

    component.status = 'Pending';
    expect(component.computedTone).toBe('warning');

    component.status = 'Draft';
    expect(component.computedTone).toBe('warning');

    component.status = 'Failed';
    expect(component.computedTone).toBe('danger');

    component.status = 'Cancelled';
    expect(component.computedTone).toBe('danger');

    component.status = 'Queued';
    expect(component.computedTone).toBe('info');

    component.status = 'Arbitrary';
    expect(component.computedTone).toBe('neutral');
  });

  it('respects explicit tone overrides', () => {
    component.status = 'Active';
    component.tone = 'danger';
    expect(component.computedTone).toBe('danger');
    expect(component.containerClasses).toContain('tone-danger');
  });

  it('renders pill and size classes', () => {
    component.pill = true;
    component.size = 'sm';
    expect(component.containerClasses).toContain('is-pill');
    expect(component.containerClasses).toContain('size-sm');
  });
});

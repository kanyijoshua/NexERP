import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CoreTestingModule } from '@abp/ng.core/testing';
import { DefaultQueueManager, QUEUE_MANAGER } from '@abp/ng.core';
import { ThemeSharedTestingModule } from '@abp/ng.theme.shared/testing';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule } from '@angular/forms';
import { PageToolbarComponent } from './page-toolbar.component';

describe('PageToolbarComponent', () => {
  let component: PageToolbarComponent;
  let fixture: ComponentFixture<PageToolbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PageToolbarComponent],
      imports: [
        CoreTestingModule.withConfig(),
        ThemeSharedTestingModule.withConfig(),
        NgbDropdownModule,
        FormsModule,
      ],
      providers: [
        { provide: QUEUE_MANAGER, useClass: DefaultQueueManager },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PageToolbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle view mode between table and card', () => {
    spyOn(component.viewModeChange, 'emit');
    expect(component.viewMode).toBe('table');

    component.setViewMode('card');
    expect(component.viewMode).toBe('card');
    expect(component.viewModeChange.emit).toHaveBeenCalledWith('card');

    component.setViewMode('table');
    expect(component.viewMode).toBe('table');
    expect(component.viewModeChange.emit).toHaveBeenCalledWith('table');
  });

  it('should emit export format when onExport is invoked', () => {
    spyOn(component.export, 'emit');

    component.onExport('excel');
    expect(component.export.emit).toHaveBeenCalledWith('excel');

    component.onExport('csv');
    expect(component.export.emit).toHaveBeenCalledWith('csv');

    component.onExport('print');
    expect(component.export.emit).toHaveBeenCalledWith('print');

    component.onExport('clipboard');
    expect(component.export.emit).toHaveBeenCalledWith('clipboard');
  });

  it('should emit filter change and handle clearFilter', () => {
    spyOn(component.filterChange, 'emit');

    component.onFilterChange('test query');
    expect(component.filter).toBe('test query');
    expect(component.filterChange.emit).toHaveBeenCalledWith('test query');

    component.clearFilter();
    expect(component.filter).toBe('');
    expect(component.filterChange.emit).toHaveBeenCalledWith('');
  });
});

import { TestBed } from '@angular/core/testing';
import { DimensionStateService } from './dimension-state.service';
import { DimensionService } from '@proxy/dimensions';
import { CompanyService } from './company.service';
import { of } from 'rxjs';

describe('DimensionStateService', () => {
  let service: DimensionStateService;
  let mockDimensionService: jasmine.SpyObj<DimensionService>;
  let mockCompanyService: Partial<CompanyService>;

  const mockSlots = [
    {
      slotNumber: 1,
      isGlobal: true,
      dimensionCode: 'DEPARTMENT',
      dimensionName: 'Department',
      values: [
        { id: '1', code: 'SALES', name: 'Sales', blocked: false },
        { id: '2', code: 'ADMIN', name: 'Administration', blocked: false },
      ],
    },
    {
      slotNumber: 2,
      isGlobal: true,
      dimensionCode: 'PROJECT',
      dimensionName: 'Project',
      values: [
        { id: '3', code: 'ALPHA', name: 'Project Alpha', blocked: false },
      ],
    },
    {
      slotNumber: 3,
      isGlobal: false,
      dimensionCode: 'COSTCENTER',
      dimensionName: 'Cost Center',
      values: [],
    },
  ];

  beforeEach(() => {
    mockDimensionService = jasmine.createSpyObj('DimensionService', ['getShortcutSlots']);
    mockDimensionService.getShortcutSlots.and.returnValue(of({ items: mockSlots } as any));
    mockCompanyService = {
      companyChanged$: of('company-1'),
    };

    TestBed.configureTestingModule({
      providers: [
        DimensionStateService,
        { provide: DimensionService, useValue: mockDimensionService },
        { provide: CompanyService, useValue: mockCompanyService },
      ],
    });

    service = TestBed.inject(DimensionStateService);
  });

  it('should load slots and return dimension names and codes', () => {
    expect(service.getDimensionName(1)).toBe('Department');
    expect(service.getDimensionCode(1)).toBe('DEPARTMENT');
    expect(service.getDimensionName(2)).toBe('Project');
    expect(service.getDimensionCode(2)).toBe('PROJECT');
    expect(service.getDimensionName(3)).toBe('Cost Center');
    expect(service.getDimensionName(4)).toBeUndefined();
  });

  it('should search dimension values by code or name', done => {
    service.searchValues(1, 'sal').subscribe(items => {
      expect(items.length).toBe(1);
      expect(items[0].code).toBe('SALES');
      expect(items[0].name).toBe('Sales');
      done();
    });
  });

  it('should build RecordEntity descriptors for lookup resolution', () => {
    const entities = service.buildEntities();
    const g1 = entities.find(e => e.key === 'globalDimension1');
    const g2 = entities.find(e => e.key === 'globalDimension2');

    expect(g1).toBeDefined();
    expect(g1?.titleKey).toBe('Erp::GlobalDimension1Code');
    expect(g2).toBeDefined();
    expect(g2?.titleKey).toBe('Erp::GlobalDimension2Code');
  });
});

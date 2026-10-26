import { extractDimensionSlot, humanizeEnumMember, withFallbacks } from './erp-localization.service';
import { DimensionStateService } from './dimension-state.service';

describe('humanizeEnumMember', () => {
  it('splits a member name into words', () => {
    expect(humanizeEnumMember('Average')).toBe('Average');
    expect(humanizeEnumMember('PendingApproval')).toBe('Pending Approval');
    expect(humanizeEnumMember('YearToDateNetChange')).toBe('Year To Date Net Change');
  });

  it('keeps acronyms readable', () => {
    expect(humanizeEnumMember('GLAccount')).toBe('G/L Account');
    expect(humanizeEnumMember('NormalVat')).toBe('Normal VAT');
    expect(humanizeEnumMember('RFQ')).toBe('RFQ');
    expect(humanizeEnumMember('Rfq')).toBe('RFQ');
  });
});

describe('extractDimensionSlot', () => {
  it('extracts global dimension slots 1 and 2', () => {
    expect(extractDimensionSlot('Erp::GlobalDimension1Code')).toBe(1);
    expect(extractDimensionSlot('GlobalDimension1Code')).toBe(1);
    expect(extractDimensionSlot('Erp::GlobalDimension1')).toBe(1);
    expect(extractDimensionSlot('GlobalDimension1')).toBe(1);

    expect(extractDimensionSlot('Erp::GlobalDimension2Code')).toBe(2);
    expect(extractDimensionSlot('GlobalDimension2Code')).toBe(2);
    expect(extractDimensionSlot('Erp::GlobalDimension2')).toBe(2);
    expect(extractDimensionSlot('GlobalDimension2')).toBe(2);
  });

  it('extracts shortcut dimension slots 1 to 8', () => {
    expect(extractDimensionSlot('Erp::ShortcutDimension1Code')).toBe(1);
    expect(extractDimensionSlot('ShortcutDimension1Code')).toBe(1);
    expect(extractDimensionSlot('Erp::ShortcutDimension2Code')).toBe(2);
    expect(extractDimensionSlot('ShortcutDimension2Code')).toBe(2);
    expect(extractDimensionSlot('Erp::ShortcutDimension3Code')).toBe(3);
    expect(extractDimensionSlot('ShortcutDimension4Code')).toBe(4);
    expect(extractDimensionSlot('Erp::ShortcutDimension8Code')).toBe(8);
  });

  it('ignores setup-specific keys and unrelated headers', () => {
    expect(extractDimensionSlot('Erp::Setup:GlobalDimension1Code')).toBeNull();
    expect(extractDimensionSlot('Erp::Setup:ShortcutDimension3Code')).toBeNull();
    expect(extractDimensionSlot('Erp::GlobalDimensions')).toBeNull();
    expect(extractDimensionSlot('Erp::ShortcutDimensions')).toBeNull();
    expect(extractDimensionSlot('Erp::Customer')).toBeNull();
  });
});

describe('withFallbacks', () => {
  const mockDimensionState = {
    getDimensionName: (slot: number) => {
      if (slot === 1) return 'Department';
      if (slot === 2) return 'Project';
      if (slot === 3) return 'Cost Center';
      return undefined;
    },
    getDimensionCode: (slot: number) => {
      if (slot === 1) return 'DEPARTMENT';
      if (slot === 2) return 'PROJECT';
      if (slot === 3) return 'COSTCENTER';
      return undefined;
    },
  } as unknown as DimensionStateService;

  it('resolves dimension name for GlobalDimension1Code and ShortcutDimension1Code', () => {
    expect(withFallbacks('Erp::GlobalDimension1Code', 'Global Dimension 1 Code', mockDimensionState)).toBe('Department');
    expect(withFallbacks('GlobalDimension1Code', 'Global Dimension 1 Code', mockDimensionState)).toBe('Department');
    expect(withFallbacks('Erp::ShortcutDimension1Code', 'Shortcut Dimension 1 Code', mockDimensionState)).toBe('Department');
  });

  it('resolves dimension name for GlobalDimension2Code and ShortcutDimension2Code', () => {
    expect(withFallbacks('Erp::GlobalDimension2Code', 'Global Dimension 2 Code', mockDimensionState)).toBe('Project');
    expect(withFallbacks('Erp::ShortcutDimension2Code', 'Shortcut Dimension 2 Code', mockDimensionState)).toBe('Project');
  });

  it('resolves shortcut dimension slots 3..8 when configured', () => {
    expect(withFallbacks('Erp::ShortcutDimension3Code', 'Shortcut Dimension 3 Code', mockDimensionState)).toBe('Cost Center');
  });

  it('falls back to default text when slot is not configured', () => {
    expect(withFallbacks('Erp::ShortcutDimension4Code', 'Shortcut Dimension 4 Code', mockDimensionState)).toBe('Shortcut Dimension 4 Code');
  });

  it('falls back to default text when dimensionState is not provided', () => {
    expect(withFallbacks('Erp::GlobalDimension1Code', 'Global Dimension 1 Code', null)).toBe('Global Dimension 1 Code');
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ErpCardComponent } from './erp-card.component';
import { ErpFastTabComponent } from './erp-fasttab.component';
import { ErpFactBoxComponent } from './erp-factbox.component';
import { ErpCardBase } from './erp-card.base';
import { ErpCardAction, ErpFactBoxGroup, ErpFactBoxTile, ErpFastTab } from './erp-card.models';
import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { CompanyService } from '../../services/company.service';
import { of } from 'rxjs';

class DummyCardComponent extends ErpCardBase<{ id: string; name: string }> {
  override fastTabs: ErpFastTab[] = [
    { id: 'general', titleKey: 'General', summary: 'Summary 1' },
    { id: 'invoicing', titleKey: 'Invoicing', collapsed: true },
  ];
}

describe('ErpCard, ErpFastTab, ErpFactBox & ErpCardBase', () => {
  describe('ErpCardComponent', () => {
    let card: ErpCardComponent;

    beforeEach(() => {
      card = new ErpCardComponent();
    });

    it('should initialize with FactBox enabled by default', () => {
      expect(card.showFactBox).toBeTrue();
      expect(card.showFactBoxToggle).toBeTrue();
      expect(card.actions).toEqual([]);
    });

    it('should toggle FactBox visibility on toggleFactBox()', () => {
      spyOn(card.showFactBoxChange, 'emit');
      card.toggleFactBox();
      expect(card.showFactBox).toBeFalse();
      expect(card.showFactBoxChange.emit).toHaveBeenCalledWith(false);

      card.toggleFactBox();
      expect(card.showFactBox).toBeTrue();
      expect(card.showFactBoxChange.emit).toHaveBeenCalledWith(true);
    });

    it('should toggle FactBox on Alt+F2 keyboard shortcut', () => {
      spyOn(card, 'toggleFactBox');
      const event = new KeyboardEvent('keydown', { key: 'F2', altKey: true });
      spyOn(event, 'preventDefault');

      card.onKeyDown(event);
      expect(event.preventDefault).toHaveBeenCalled();
      expect(card.toggleFactBox).toHaveBeenCalled();
    });

    it('should run action and emit actionClick', () => {
      spyOn(card.actionClick, 'emit');
      const actionSpy = jasmine.createSpy('action');
      const action: ErpCardAction = {
        key: 'post',
        labelKey: 'Post',
        icon: 'fas fa-check',
        action: actionSpy,
      };

      const mouseEvent = new MouseEvent('click');
      card.runAction(action, mouseEvent);

      expect(actionSpy).toHaveBeenCalledWith(mouseEvent);
      expect(card.actionClick.emit).toHaveBeenCalledWith(action);
    });
  });

  describe('ErpFastTabComponent', () => {
    let tab: ErpFastTabComponent;

    beforeEach(() => {
      tab = new ErpFastTabComponent();
    });

    it('should toggle collapsed state on toggle()', () => {
      spyOn(tab.collapsedChange, 'emit');
      expect(tab.collapsed).toBeFalse();

      tab.toggle();
      expect(tab.collapsed).toBeTrue();
      expect(tab.collapsedChange.emit).toHaveBeenCalledWith(true);
    });
  });

  describe('ErpFactBoxComponent', () => {
    let factBox: ErpFactBoxComponent;

    beforeEach(() => {
      factBox = new ErpFactBoxComponent();
    });

    it('should emit tileClick and execute tile action', () => {
      spyOn(factBox.tileClick, 'emit');
      const tileAction = jasmine.createSpy('tileAction');
      const tile: ErpFactBoxTile = {
        titleKey: 'Orders',
        value: 5,
        action: tileAction,
      };

      factBox.onTileClick(tile);
      expect(tileAction).toHaveBeenCalled();
      expect(factBox.tileClick.emit).toHaveBeenCalledWith(tile);
    });
  });

  describe('ErpCardBase', () => {
    it('should manage FastTabs and FactBox state via ErpCardBase', () => {
      TestBed.configureTestingModule({
        providers: [
          { provide: ToasterService, useValue: {} },
          { provide: ConfirmationService, useValue: {} },
          { provide: CompanyService, useValue: { companyChanged$: of('comp-1') } },
        ],
      });

      TestBed.runInInjectionContext(() => {
        const dummy = new DummyCardComponent();
        expect(dummy.fastTabs.length).toBe(2);
        expect(dummy.isFastTabCollapsed('general')).toBeFalse();
        expect(dummy.isFastTabCollapsed('invoicing')).toBeTrue();

        dummy.toggleFastTab('general');
        expect(dummy.isFastTabCollapsed('general')).toBeTrue();

        dummy.updateTile('stats', { titleKey: 'Balance', value: 1200 });
        expect(dummy.factBoxGroups.length).toBe(1);
        expect(dummy.factBoxGroups[0].tiles?.length).toBe(1);
        expect(dummy.factBoxGroups[0].tiles?.[0].value).toBe(1200);

        dummy.updateFact('stats', { labelKey: 'Credit Limit', value: 5000 });
        expect(dummy.factBoxGroups[0].facts?.length).toBe(1);
      });
    });
  });
});

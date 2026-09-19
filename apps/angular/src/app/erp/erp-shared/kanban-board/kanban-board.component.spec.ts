import { CoreTestingModule } from '@abp/ng.core/testing';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { KanbanColumn, KanbanMoveEvent } from '../models';
import { KanbanBoardComponent } from './kanban-board.component';

interface Card {
  id: string;
  name: string;
}

describe('KanbanBoardComponent', () => {
  let fixture: ComponentFixture<KanbanBoardComponent<Card>>;
  let component: KanbanBoardComponent<Card>;
  let columns: KanbanColumn<Card>[];
  let events: KanbanMoveEvent[];

  const ids = (columnId: string) => columns.find(c => c.id === columnId)?.cards.map(c => c.id);

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [KanbanBoardComponent],
      imports: [CoreTestingModule.withConfig(), DragDropModule],
    }).compileComponents();
  }));

  beforeEach(() => {
    columns = [
      {
        id: 'new',
        title: 'New',
        cards: [
          { id: 'a', name: 'A' },
          { id: 'b', name: 'B' },
          { id: 'c', name: 'C' },
        ],
      },
      { id: 'qualified', title: 'Qualified', cards: [{ id: 'x', name: 'X' }] },
      { id: 'won', title: 'Won', cards: [], isWon: true, folded: true },
    ];
    events = [];

    fixture = TestBed.createComponent<KanbanBoardComponent<Card>>(KanbanBoardComponent);
    component = fixture.componentInstance;
    component.columns = columns;
    component.cardId = card => card.id;
    component.moved.subscribe(e => events.push(e));
    fixture.detectChanges();
  });

  it('renders a column per entry with its card count, folded columns hide their cards', () => {
    const rendered = fixture.nativeElement.querySelectorAll('.erp-kanban__column');
    expect(rendered.length).toBe(3);
    expect(rendered[0].querySelector('.badge').textContent.trim()).toBe('3');
    expect(rendered[0].querySelectorAll('.erp-kanban__card').length).toBe(3);
    expect(rendered[2].classList).toContain('erp-kanban__column--folded');
    expect(rendered[2].querySelectorAll('.erp-kanban__card').length).toBe(0);
  });

  it('moves a card to another column optimistically and emits the move', () => {
    const cardsReference = columns[1].cards;
    component.moveCard('new', 'qualified', 1, 0);

    expect(ids('new')).toEqual(['a', 'c']);
    expect(ids('qualified')).toEqual(['b', 'x']);
    expect(columns[1].cards).toBe(cardsReference); // mutated in place
    expect(events.length).toBe(1);
    expect(events[0]).toEqual(
      jasmine.objectContaining({
        cardId: 'b',
        fromColumnId: 'new',
        toColumnId: 'qualified',
        index: 0,
      }),
    );
  });

  it('revert() restores both columns exactly (and is idempotent)', () => {
    const fromReference = columns[0].cards;
    const toReference = columns[1].cards;
    component.moveCard('new', 'qualified', 0, 1);
    expect(ids('new')).toEqual(['b', 'c']);
    expect(ids('qualified')).toEqual(['x', 'a']);

    events[0].revert();
    events[0].revert();

    expect(ids('new')).toEqual(['a', 'b', 'c']);
    expect(ids('qualified')).toEqual(['x']);
    expect(columns[0].cards).toBe(fromReference);
    expect(columns[1].cards).toBe(toReference);
  });

  it('reorders inside a column and reverts', () => {
    component.moveCard('new', 'new', 0, 2);
    expect(ids('new')).toEqual(['b', 'c', 'a']);
    expect(events[0].index).toBe(2);

    events[0].revert();
    expect(ids('new')).toEqual(['a', 'b', 'c']);
  });

  it('does not emit when the card is dropped where it already was', () => {
    expect(component.moveCard('new', 'new', 1, 1)).toBeNull();
    expect(events.length).toBe(0);
    expect(ids('new')).toEqual(['a', 'b', 'c']);
  });

  it('does nothing when moving is disabled or the source is unknown', () => {
    expect(component.moveCard('nope', 'qualified', 0, 0)).toBeNull();
    expect(component.moveCard('new', 'qualified', 9, 0)).toBeNull();

    component.canMove = false;
    expect(component.moveCard('new', 'qualified', 0, 0)).toBeNull();

    expect(events.length).toBe(0);
    expect(ids('new')).toEqual(['a', 'b', 'c']);
    expect(ids('qualified')).toEqual(['x']);
  });

  it('applies a CDK drop event', () => {
    component.onDrop({
      previousContainer: { data: columns[0] },
      container: { data: columns[2] },
      previousIndex: 2,
      currentIndex: 0,
    } as never);

    expect(ids('won')).toEqual(['c']);
    expect(events[0].toColumnId).toBe('won');
  });

  it('folds and unfolds a column', () => {
    component.toggleFold(columns[0]);
    fixture.detectChanges();
    expect(columns[0].folded).toBeTrue();
    expect(fixture.nativeElement.querySelectorAll('.erp-kanban__card').length).toBe(1);
  });
});

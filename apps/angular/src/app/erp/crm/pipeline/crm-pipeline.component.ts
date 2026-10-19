import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { OpportunityDto, OpportunityService, PipelineDto } from '@proxy/crm';
import { finalize } from 'rxjs';
import { KanbanColumn, KanbanMoveEvent } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/**
 * The lead and opportunity pipelines as kanban boards: a column per stage with its weighted
 * revenue, cards dragged from stage to stage. Dropping on a lost stage asks why.
 */
@Component({
  selector: 'app-crm-pipeline',
  templateUrl: './crm-pipeline.component.html',
  standalone: false,
})
export class CrmPipelineComponent implements OnInit {
  private readonly service = inject(OpportunityService);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);
  private readonly companyService = inject(CompanyService);
  private readonly destroyRef = inject(DestroyRef);

  readonly pipeline = signal<'Lead' | 'Opportunity'>('Opportunity');
  readonly data = signal<PipelineDto | null>(null);
  readonly columns = signal<KanbanColumn<OpportunityDto>[]>([]);
  readonly busy = signal(false);
  readonly view = signal<'board' | 'list'>('board');

  /** A drop on a lost stage waits here for its close code. */
  readonly pendingLoss = signal<KanbanMoveEvent | null>(null);
  closeCode = '';

  readonly cardId = (card: OpportunityDto) => card.id!;

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  switchPipeline(pipeline: 'Lead' | 'Opportunity'): void {
    this.pipeline.set(pipeline);
    this.load();
  }

  load(): void {
    this.busy.set(true);
    this.service
      .getPipeline({ pipeline: this.pipeline() })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe(data => {
        this.data.set(data);
        this.columns.set(
          data.stages.map(stage => ({
            id: stage.stageId,
            title: stage.name ?? '',
            cards: data.cards.filter(c => c.stageId === stage.stageId),
            aggregateLabel: `${stage.weightedRevenue.toLocaleString(undefined, { maximumFractionDigits: 0 })} / ${stage.expectedRevenue.toLocaleString(undefined, { maximumFractionDigits: 0 })}`,
            folded: stage.isWon || stage.isLost,
            isWon: stage.isWon,
            isLost: stage.isLost,
          })),
        );
      });
  }

  onMoved(event: KanbanMoveEvent): void {
    if (event.fromColumnId === event.toColumnId) {
      return;
    }

    const target = this.data()?.stages.find(s => s.stageId === event.toColumnId);
    if (target?.isLost) {
      this.closeCode = '';
      this.pendingLoss.set(event);
      return;
    }

    this.move(event);
  }

  confirmLoss(): void {
    const event = this.pendingLoss();
    if (event && this.closeCode.trim()) {
      this.pendingLoss.set(null);
      this.move(event, this.closeCode.trim());
    }
  }

  cancelLoss(): void {
    this.pendingLoss()?.revert();
    this.pendingLoss.set(null);
  }

  open(card: OpportunityDto): void {
    void this.router.navigate(['/erp/opportunities', card.id]);
  }

  private move(event: KanbanMoveEvent, closeCode?: string): void {
    this.service.move(event.cardId, { stageId: event.toColumnId, closeCode }).subscribe({
      next: () => {
        this.toaster.success('Erp::SavedSuccessfully');
        this.load();
      },
      error: () => event.revert(),
    });
  }
}

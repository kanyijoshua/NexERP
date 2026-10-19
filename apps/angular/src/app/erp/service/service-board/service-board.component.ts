import { LocalizationService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { ServiceOrderDto, ServiceOrderService, ServiceOrderStatus } from '@proxy/service-management';
import { finalize } from 'rxjs';
import { KanbanColumn, KanbanMoveEvent } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

const COLUMNS: ServiceOrderStatus[] = [ServiceOrderStatus.Pending, ServiceOrderStatus.InProcess, ServiceOrderStatus.OnHold, ServiceOrderStatus.Finished];

/**
 * Service orders as a board: a column per status, optionally narrowed to one technician.
 * Cards move between Pending, In Process and On Hold; an order only reaches Finished by posting.
 */
@Component({
  selector: 'app-service-board',
  templateUrl: './service-board.component.html',
  standalone: false,
})
export class ServiceBoardComponent implements OnInit {
  private readonly service = inject(ServiceOrderService);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);
  private readonly localization = inject(LocalizationService);
  private readonly companyService = inject(CompanyService);
  private readonly destroyRef = inject(DestroyRef);

  readonly columns = signal<KanbanColumn<ServiceOrderDto>[]>([]);
  readonly busy = signal(false);
  readonly priorityHigh = 2;
  technician = '';

  readonly cardId = (card: ServiceOrderDto) => card.id!;

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  load(): void {
    this.busy.set(true);
    this.service
      .getList({ resourceNo: this.technician || undefined, maxResultCount: 1000, skipCount: 0, sorting: 'orderDate' })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe(result => {
        const orders = result.items ?? [];
        this.columns.set(
          COLUMNS.map(status => ({
            id: String(status),
            title: this.localization.instant(`Erp::Enum:ServiceOrderStatus.${ServiceOrderStatus[status]}`),
            cards: orders.filter(o => o.status === status),
            folded: status === ServiceOrderStatus.Finished,
            isWon: status === ServiceOrderStatus.Finished,
          })),
        );
      });
  }

  onMoved(event: KanbanMoveEvent): void {
    if (event.fromColumnId === event.toColumnId) {
      return;
    }

    const status = Number(event.toColumnId) as ServiceOrderStatus;
    if (status === ServiceOrderStatus.Finished) {
      // Finishing is done by posting the order, which the card page offers.
      this.toaster.warn('Erp::FinishServiceOrderByPosting');
      event.revert();
      return;
    }

    this.service.changeStatus(event.cardId, { status }).subscribe({
      next: () => {
        this.toaster.success('Erp::SavedSuccessfully');
        this.load();
      },
      error: () => event.revert(),
    });
  }

  open(card: ServiceOrderDto): void {
    void this.router.navigate(['/erp/service-orders', card.id]);
  }
}

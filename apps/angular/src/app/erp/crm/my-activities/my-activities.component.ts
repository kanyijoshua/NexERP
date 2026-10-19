import { ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ChatterService, DocumentActivityTaskDto } from '@proxy/chatter';
import { finalize } from 'rxjs';

/** The open activities assigned to the current user, overdue ones first, each marked done in one click. */
@Component({
  selector: 'app-my-activities',
  templateUrl: './my-activities.component.html',
  standalone: false,
})
export class MyActivitiesComponent implements OnInit {
  private readonly chatter = inject(ChatterService);
  private readonly toaster = inject(ToasterService);

  readonly tasks = signal<DocumentActivityTaskDto[]>([]);
  readonly busy = signal(false);
  readonly today = new Date().toISOString().substring(0, 10);
  readonly overdue = computed(() => this.tasks().filter(t => (t.dueDate ?? '').substring(0, 10) < this.today).length);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.busy.set(true);
    this.chatter
      .getMyTasks()
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe(result => this.tasks.set(result.items ?? []));
  }

  isOverdue(task: DocumentActivityTaskDto): boolean {
    return (task.dueDate ?? '').substring(0, 10) < this.today;
  }

  complete(task: DocumentActivityTaskDto): void {
    this.chatter.completeTask(task.id!).subscribe(() => {
      this.toaster.success('Erp::ActivityDone');
      this.tasks.set(this.tasks().filter(t => t.id !== task.id));
    });
  }
}

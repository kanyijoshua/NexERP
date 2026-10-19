import { ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProjectJournalBatchService, TimeSheetDto, TimeSheetService, TimeSheetStatus } from '@proxy/projects';
import { Observable, finalize } from 'rxjs';
import { WeekGridRow } from '../../erp-shared';

/** An editable line of the week: which project task, and the hours of each day. */
interface SheetLine {
  id?: string;
  projectNo: string;
  taskNo: string;
  description?: string;
  chargeable: boolean;
  days: number[];
  posted: boolean;
}

/**
 * A time sheet: a resource's week as a grid of project tasks by day. It is filled in, submitted,
 * approved (or rejected) and then moved into a project journal batch for posting.
 */
@Component({
  selector: 'app-time-sheet',
  templateUrl: './time-sheet.component.html',
  standalone: false,
})
export class TimeSheetComponent implements OnInit {
  private readonly service = inject(TimeSheetService);
  private readonly batches = inject(ProjectJournalBatchService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);

  readonly status = TimeSheetStatus;
  readonly sheet = signal<TimeSheetDto | null>(null);
  readonly lines = signal<SheetLine[]>([]);
  readonly busy = signal(false);
  readonly dirty = signal(false);
  readonly editable = computed(() => {
    const s = this.sheet();
    return !s || s.status === TimeSheetStatus.Open || s.status === TimeSheetStatus.Rejected;
  });
  readonly rows = computed<WeekGridRow[]>(() =>
    this.lines().map(l => ({ label: `${l.projectNo} / ${l.taskNo}`, sublabel: l.description, values: l.days, readonly: l.posted })),
  );

  resourceNo = '';
  startingDate = new Date().toISOString().substring(0, 10);
  newProjectNo = '';
  newTaskNo = '';
  newDescription = '';
  rejectComment = '';
  batchName = 'DEFAULT';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.load(id);
    }
  }

  load(id: string): void {
    this.busy.set(true);
    this.service
      .get(id)
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe(sheet => this.show(sheet));
  }

  addLine(): void {
    if (!this.newProjectNo || !this.newTaskNo) {
      return;
    }

    this.lines.set([
      ...this.lines(),
      { projectNo: this.newProjectNo, taskNo: this.newTaskNo, description: this.newDescription || undefined, chargeable: true, days: [0, 0, 0, 0, 0, 0, 0], posted: false },
    ]);
    this.newTaskNo = '';
    this.newDescription = '';
    this.dirty.set(true);
  }

  removeLine(index: number): void {
    this.lines.set(this.lines().filter((_, i) => i !== index));
    this.dirty.set(true);
  }

  onCellChanged(): void {
    this.dirty.set(true);
  }

  save(): void {
    const sheet = this.sheet();
    const input = {
      resourceNo: sheet?.resourceNo ?? this.resourceNo,
      startingDate: sheet?.startingDate ?? this.startingDate,
      lines: this.lines().map(l => ({ id: l.id, projectNo: l.projectNo, taskNo: l.taskNo, description: l.description, chargeable: l.chargeable, days: l.days })),
    };
    const call = sheet ? this.service.update(sheet.id!, input) : this.service.create(input);
    this.run(call, saved => {
      if (!sheet) {
        void this.router.navigate(['/erp/project-work/time-sheet', saved.id], { replaceUrl: true });
      }
    });
  }

  submit(): void {
    this.run(this.service.submit(this.sheet()!.id!));
  }

  approve(): void {
    this.run(this.service.approve(this.sheet()!.id!));
  }

  reject(): void {
    this.run(this.service.reject(this.sheet()!.id!, { comment: this.rejectComment }));
  }

  reopen(): void {
    this.run(this.service.reopen(this.sheet()!.id!));
  }

  transfer(): void {
    const sheet = this.sheet()!;
    this.batches.getList({ filter: this.batchName, maxResultCount: 10, skipCount: 0 }).subscribe(result => {
      const batch = (result.items ?? []).find(b => b.name === this.batchName.toUpperCase());
      if (!batch) {
        this.toaster.error('Erp::ProjectJournalBatchNotFound');
        return;
      }

      this.service.transferToJournal(sheet.id!, { projectJournalBatchId: batch.id! }).subscribe(count => {
        this.toaster.success('Erp::TimeSheetTransferred', undefined, { messageLocalizationParams: [String(count)] });
        this.load(sheet.id!);
      });
    });
  }

  private run(call: Observable<TimeSheetDto>, after?: (sheet: TimeSheetDto) => void): void {
    this.busy.set(true);
    call.pipe(finalize(() => this.busy.set(false))).subscribe(sheet => {
      this.show(sheet);
      this.toaster.success('Erp::SavedSuccessfully');
      after?.(sheet);
    });
  }

  private show(sheet: TimeSheetDto): void {
    this.sheet.set(sheet);
    this.lines.set(
      sheet.lines.map(l => ({
        id: l.id,
        projectNo: l.projectNo ?? '',
        taskNo: l.taskNo ?? '',
        description: l.description,
        chargeable: l.chargeable,
        days: [...l.days],
        posted: l.posted,
      })),
    );
    this.dirty.set(false);
  }
}

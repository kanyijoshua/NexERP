import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProjectDto, ProjectService, ProjectTaskStatisticsDto, ProjectTaskType } from '@proxy/projects';

/** A project's budget against its actual cost, usage price and invoicing, task by task. */
@Component({
  selector: 'app-project-statistics',
  templateUrl: './project-statistics.component.html',
  standalone: false,
})
export class ProjectStatisticsComponent implements OnInit {
  private readonly service = inject(ProjectService);
  private readonly route = inject(ActivatedRoute);

  readonly posting = ProjectTaskType.Posting;
  readonly project = signal<ProjectDto | null>(null);
  readonly rows = signal<ProjectTaskStatisticsDto[]>([]);
  readonly totals = computed(() => {
    const rows = this.rows();
    const sum = (field: keyof ProjectTaskStatisticsDto) => rows.reduce((s, r) => s + (Number(r[field]) || 0), 0);
    return {
      budgetCost: sum('budgetCost'),
      budgetPrice: sum('budgetPrice'),
      usageCost: sum('usageCost'),
      usagePrice: sum('usagePrice'),
      invoicedPrice: sum('invoicedPrice'),
    };
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.service.get(id).subscribe(p => this.project.set(p));
    this.service.getStatistics(id).subscribe(rows => this.rows.set(rows));
  }

  percent(actual: number, budget: number): number {
    return budget ? Math.round((actual / budget) * 100) : 0;
  }
}

import { CoreModule } from '@abp/ng.core';
import { AsyncPipe, NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { CompanyService } from '../../services/company.service';
import { MenuIndexService } from './menu-index.service';
import { SuiteEntry, filterMenuSuite, pagesOf } from './menu-suite.model';

/**
 * Business Central's menu suite: every area of the menu as a column of pages, with a Find box.
 * The columns come from the live menu, so they follow permissions and the modules this company
 * has switched on.
 */
@Component({
  selector: 'app-menu-suite',
  templateUrl: './menu-suite.component.html',
  styleUrls: ['./menu-suite.component.scss'],
  imports: [
    AsyncPipe,
    CoreModule,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    NgClass,
    RouterLink,
  ],
})
export class MenuSuiteComponent {
  private readonly index = inject(MenuIndexService);
  private readonly router = inject(Router);
  private readonly dialogRef = inject(MatDialogRef<MenuSuiteComponent>);

  readonly company$ = inject(CompanyService).currentCompany$;

  readonly find = signal('');

  /** Groups the user has opened, by section and name. */
  private readonly opened = signal(new Set<string>());

  readonly shown = computed(() => filterMenuSuite(this.index.sections(), this.find()));

  readonly filtering = computed(() => this.find().trim().length > 0);

  isOpen(section: string, entry: SuiteEntry): boolean {
    return this.filtering() || this.opened().has(`${section}/${entry.name}`);
  }

  toggle(section: string, entry: SuiteEntry): void {
    const key = `${section}/${entry.name}`;
    const next = new Set(this.opened());

    if (next.has(key)) {
      next.delete(key);
    } else {
      next.add(key);
    }

    this.opened.set(next);
  }

  /** Enter in Find opens the first page that matches, the way Tell Me does. */
  openFirstMatch(): void {
    const first = pagesOf(this.shown())[0];

    if (first?.path) {
      this.go(first.path);
    }
  }

  go(path: string): void {
    this.dialogRef.close();
    this.router.navigateByUrl(path);
  }
}

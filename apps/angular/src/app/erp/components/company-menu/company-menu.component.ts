import { ConfigStateService, CoreModule } from '@abp/ng.core';
import { Component, DestroyRef, Input, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { filter, switchMap } from 'rxjs/operators';
import { CompanyDto, CompanyService } from '../../services/company.service';
import { CopyCompanyDialogComponent } from './copy-company-dialog.component';

/**
 * The company you are working in, and a menu to switch to another one.
 * <p>
 * Business Central shows the company as the Role Center's title; here it sits in the top bar
 * (variant "nav") and as the home page title (variant "title"), both opening the same menu.
 * Switching does not reload the browser: pages listen to CompanyService.companyChanged$.
 * </p>
 */
@Component({
  selector: 'app-company-menu',
  templateUrl: './company-menu.component.html',
  styleUrls: ['./company-menu.component.scss'],
  imports: [CoreModule, MatDividerModule, MatMenuModule],
})
export class CompanyMenuComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly config = inject(ConfigStateService);
  private readonly dialog = inject(MatDialog);
  readonly service = inject(CompanyService);

  @Input() variant: 'nav' | 'title' = 'nav';

  readonly companies$ = this.service.companies$;
  readonly current$ = this.service.currentCompany$;

  ngOnInit(): void {
    // The top bar is there before sign-in; the list can only be read once someone is signed in.
    // Only the top bar reads it: the home page title shares what the top bar loaded.
    if (this.variant !== 'nav') {
      return;
    }

    this.config
      .getOne$('currentUser')
      .pipe(
        filter(user => !!user?.isAuthenticated),
        switchMap(() => this.service.loadCompanies()),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  isActive(company: CompanyDto): boolean {
    return company.id === this.service.getActiveCompanyId();
  }

  switchTo(company: CompanyDto): void {
    this.service.setActiveCompany(company.id);
  }

  copy(source: CompanyDto): void {
    this.dialog
      .open(CopyCompanyDialogComponent, { data: source, width: '28rem', maxWidth: '96vw' })
      .afterClosed()
      .pipe(
        filter(created => !!created),
        switchMap(() => this.service.loadCompanies()),
      )
      .subscribe();
  }
}

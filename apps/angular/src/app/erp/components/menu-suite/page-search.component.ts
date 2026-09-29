import { ConfigStateService, CoreModule } from '@abp/ng.core';
import { Component, ElementRef, HostListener, computed, inject, signal, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { Router } from '@angular/router';
import { MenuIndexService } from './menu-index.service';
import { MenuSuiteService } from './menu-suite.service';
import { SuitePage, searchPages } from './menu-suite.model';

/**
 * The top bar's page search, Business Central's "Tell me what you want to do". Type part of a
 * page's name (or the area it sits in) and pick it; Alt+Q jumps here from anywhere. The last
 * option opens the full menu suite, for when the name does not come to mind.
 */
@Component({
  selector: 'app-page-search',
  templateUrl: './page-search.component.html',
  styleUrls: ['./page-search.component.scss'],
  imports: [CoreModule, MatAutocompleteModule],
})
export class PageSearchComponent {
  private readonly index = inject(MenuIndexService);
  private readonly suite = inject(MenuSuiteService);
  private readonly router = inject(Router);

  readonly user = toSignal(inject(ConfigStateService).getOne$('currentUser'));

  private readonly input = viewChild<ElementRef<HTMLInputElement>>('input');

  readonly text = signal('');
  readonly results = computed(() => searchPages(this.index.pages(), this.text()));

  /** Sentinel value for the "open the menu" option, so it can share the autocomplete. */
  readonly openMenu = '__menu-suite__';

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    if (event.altKey && !event.ctrlKey && event.key.toLowerCase() === 'q' && this.user()?.isAuthenticated) {
      event.preventDefault();
      this.input()?.nativeElement.focus();
      this.input()?.nativeElement.select();
    }
  }

  display = (): string => '';

  select(event: MatAutocompleteSelectedEvent): void {
    const value = event.option.value as SuitePage | string;

    this.text.set('');
    this.input()?.nativeElement.blur();

    if (value === this.openMenu) {
      this.suite.open();
    } else if (typeof value !== 'string') {
      this.router.navigateByUrl(value.path);
    }
  }

  clear(): void {
    this.text.set('');
    this.input()?.nativeElement.blur();
  }
}

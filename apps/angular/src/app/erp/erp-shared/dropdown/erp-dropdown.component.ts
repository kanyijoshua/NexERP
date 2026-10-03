import {
  Component,
  ElementRef,
  forwardRef,
  HostListener,
  Input,
  OnChanges,
  SimpleChanges,
  TemplateRef,
} from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { Observable } from 'rxjs';
import { ErpDropdownBase } from './erp-dropdown.base';
import { ErpDropdownOption } from './erp-dropdown.models';

@Component({
  selector: 'erp-dropdown',
  templateUrl: './erp-dropdown.component.html',
  styleUrls: ['./erp-dropdown.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ErpDropdownComponent),
      multi: true,
    },
  ],
  standalone: false,
})
export class ErpDropdownComponent<T = any> extends ErpDropdownBase<T> implements OnChanges {
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() loadOptions?: (query: string) => Observable<ErpDropdownOption<T>[]>;
  @Input() customOptionTemplate?: TemplateRef<any>;
  @Input() customSelectedTemplate?: TemplateRef<any>;
  @Input() dropdownClass = '';

  constructor(private readonly elementRef: ElementRef) {
    super();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['options'] && this.options) {
      if (this.selectedOption) {
        const match = this.options.find(o => this.areValuesEqual(o.value, this.selectedOption?.value));
        if (match) this.selectedOption = match;
      }
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.closeDropdown();
    }
  }

  override onSearchInput(query: string): void {
    super.onSearchInput(query);
    if (this.loadOptions) {
      this.loading = true;
      this.loadOptions(query).subscribe({
        next: items => {
          this.options = items;
          this.loading = false;
        },
        error: () => (this.loading = false),
      });
    }
  }
}

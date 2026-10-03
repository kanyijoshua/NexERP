import { Component } from '@angular/core';
import { ErpButtonBase } from './erp-button.base';

@Component({
  selector: 'erp-button',
  templateUrl: './erp-button.component.html',
  styleUrls: ['./erp-button.component.scss'],
  standalone: false,
})
export class ErpButtonComponent extends ErpButtonBase {}

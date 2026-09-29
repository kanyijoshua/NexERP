import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatRippleModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CompanyMenuComponent } from '../erp/components/company-menu/company-menu.component';
import { SharedModule } from '../shared/shared.module';
import { HomeRoutingModule } from './home-routing.module';
import { HomeComponent } from './home.component';

@NgModule({
  declarations: [HomeComponent],
  imports: [
    SharedModule,
    HomeRoutingModule,
    CompanyMenuComponent,
    MatButtonModule,
    MatProgressBarModule,
    MatRippleModule,
    MatTooltipModule,
  ],
})
export class HomeModule {}

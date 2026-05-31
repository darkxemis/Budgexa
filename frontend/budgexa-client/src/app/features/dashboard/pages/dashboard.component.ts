import { Component, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserStore } from '../../../core/state/user.store';
import { TranslateModule } from '@ngx-translate/core';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';
import { DashboardCardComponent } from '../../../shared/components/dashboard-card/dashboard-card.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, CommonModule, TranslateModule, DashboardCardComponent, UserMenuComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  private readonly userStore = inject(UserStore);
  private readonly router = inject(Router);
  
  readonly user = this.userStore.user;

  protected navigateToInvoices(): void {
    this.router.navigate(['/invoices']);
  }

  protected navigateToUsers(): void {
    this.router.navigate(['/users']);
  }
}

import { Component, computed, inject, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { UserStore } from '../../../core/state/user.store';
import { TranslateModule } from '@ngx-translate/core';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';
import { DashboardCardComponent } from '../../../shared/components/dashboard-card/dashboard-card.component';
import { ADMIN_ROLES, RoleName } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, TranslateModule, DashboardCardComponent, UserMenuComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  private readonly userStore = inject(UserStore);
  private readonly router = inject(Router);

  readonly user = this.userStore.user;

  /** True when the current user can access the users section. */
  protected readonly canManageUsers = computed(() => {
    const roles = (this.user()?.roles ?? []) as RoleName[];
    return roles.some(role => ADMIN_ROLES.includes(role));
  });

  protected navigateToInvoices(): void {
    this.router.navigate(['/invoices']);
  }

  protected navigateToUsers(): void {
    this.router.navigate(['/users']);
  }
}

import { Component, computed, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { UserStore } from '../../../core/state/user.store';
import { TranslateModule } from '@ngx-translate/core';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';
import { DashboardCardComponent } from '../../../shared/components/dashboard-card/dashboard-card.component';
import { ADMIN_ROLES, ALL_ROLES, RoleName } from '../../../core/constants/roles.constants';

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
  protected readonly linkCopied = signal(false);

  /** True when the current user can access the customers section. */
  protected readonly canManageCustomers = computed(() => {
    const roles = (this.user()?.roles ?? []) as RoleName[];
    return roles.some(role => ALL_ROLES.includes(role));
  });

  /** True when the current user can access the items section. */
  protected readonly canManageItems = computed(() => {
    const roles = (this.user()?.roles ?? []) as RoleName[];
    return roles.some(role => ALL_ROLES.includes(role));
  });

  /** True when the current user can access the users section. */
  protected readonly canManageUsers = computed(() => {
    const roles = (this.user()?.roles ?? []) as RoleName[];
    return roles.some(role => ADMIN_ROLES.includes(role));
  });

  protected readonly publicBudgetLink = computed(() => {
    const companyId = this.user()?.companyId;
    if (!companyId) return '';
    const origin = typeof window !== 'undefined' ? window.location.origin : '';
    return `${origin}/public-budget/${companyId}/prompt`;
  });

  protected copyPublicLink(): void {
    const link = this.publicBudgetLink();
    if (!link) return;
    navigator.clipboard.writeText(link).then(() => {
      this.linkCopied.set(true);
      setTimeout(() => this.linkCopied.set(false), 3000);
    });
  }

  protected navigateToCustomers(): void {
    this.router.navigate(['/customers']);
  }

  protected navigateToItems(): void {
    this.router.navigate(['/items']);
  }

  protected navigateToBudgets(): void {
    this.router.navigate(['/budgets']);
  }

  protected navigateToInvoices(): void {
    this.router.navigate(['/invoices']);
  }

  protected navigateToUsers(): void {
    this.router.navigate(['/users']);
  }
}

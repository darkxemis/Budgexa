import { Component, signal, inject, computed, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { UserMenuComponent } from '../../components/user-menu/user-menu.component';
import { IconComponent, IconName } from '../../components/icon/icon.component';
import { UserStore } from '../../../core/state/user.store';
import { ADMIN_ROLES, RoleName } from '../../../core/constants/roles.constants';

interface NavItem {
  path: string;
  icon: IconName;
  label: string;
  /** Optional role allow-list. When omitted the item is visible to every authenticated user. */
  roles?: readonly RoleName[];
  active?: boolean;
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterModule, TranslateModule, UserMenuComponent, IconComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent {
  private readonly router = inject(Router);
  private readonly userStore = inject(UserStore);

  protected sidebarCollapsed = signal(true);

  private readonly allNavItems: readonly NavItem[] = [
    { path: '/dashboard', icon: 'home', label: 'nav.dashboard' },
    { path: '/customers', icon: 'briefcase', label: 'nav.customers' },
    { path: '/items', icon: 'package', label: 'nav.items' },
    { path: '/budgets', icon: 'clipboard', label: 'nav.budgets' },
    { path: '/invoices', icon: 'invoice', label: 'nav.invoices' },
    { path: '/users', icon: 'shield', label: 'nav.users', roles: ADMIN_ROLES },
  ];

  protected readonly navItems = computed<readonly NavItem[]>(() => {
    const userRoles = (this.userStore.user()?.roles ?? []) as RoleName[];
    return this.allNavItems.filter(item =>
      !item.roles || item.roles.some(role => userRoles.includes(role))
    );
  });

  protected toggleSidebar(): void {
    this.sidebarCollapsed.update(value => !value);
  }

  protected isActiveRoute(path: string): boolean {
    return this.router.url.startsWith(path);
  }

  protected navigateTo(path: string): void {
    this.router.navigate([path]);
    // Close sidebar on mobile after navigation
    if (this.isMobile()) {
      this.sidebarCollapsed.set(true);
    }
  }

  private isMobile(): boolean {
    return typeof window !== 'undefined' && window.innerWidth < 1024;
  }
}

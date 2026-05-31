import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { UserMenuComponent } from '../../components/user-menu/user-menu.component';

interface NavItem {
  path: string;
  icon: string;
  label: string;
  active?: boolean;
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, UserMenuComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  protected sidebarCollapsed = signal(this.isMobile());

  protected navItems: NavItem[] = [
    { path: '/dashboard', icon: 'home', label: 'nav.dashboard' },
    { path: '/invoices', icon: 'invoice', label: 'nav.invoices' },
    { path: '/users', icon: 'users', label: 'nav.users' },
  ];

  constructor(private router: Router) {}

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

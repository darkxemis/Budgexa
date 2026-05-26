import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserStore } from '../../../core/state/user.store';
import { TranslateModule } from '@ngx-translate/core';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ToastType } from '../../../shared/components/toast/toast.type';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';
import { DashboardCardComponent } from '../../../shared/components/dashboard-card/dashboard-card.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, CommonModule, TranslateModule, UserMenuComponent, DashboardCardComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly userStore = inject(UserStore);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  
  readonly user = this.userStore.user;

  ngOnInit(): void {
    const user = this.user();
    if (user?.firstName) {
      this.toast.show('welcome', ToastType.Success, { name: user.firstName });
    } else {
      this.toast.show('welcome', ToastType.Success);
    }
  }

  protected navigateToInvoices(): void {
    this.router.navigate(['/invoices']);
  }

  protected navigateToUsers(): void {
    this.router.navigate(['/users']);
  }
}

import { Component, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserStore } from '../../../core/state/user.store';
import { TranslateModule } from '@ngx-translate/core';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ToastType } from '../../../shared/components/toast/toast.type';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, CommonModule, TranslateModule, UserMenuComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly userStore = inject(UserStore);
  private readonly toast = inject(ToastService);
  readonly user = this.userStore.user;

  ngOnInit() {
    const user = this.user();
    if (user?.firstName) {
      this.toast.show('welcome', ToastType.Success, { name: user.firstName });
    } else {
      this.toast.show('welcome', ToastType.Success);
    }
  }
}

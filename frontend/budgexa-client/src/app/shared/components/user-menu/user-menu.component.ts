import { Component, inject, HostListener, signal } from '@angular/core';
import { UpperCasePipe } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { UserStore } from '../../../core/state/user.store';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { performLogout } from '../../../core/utils/auth.utils';
import { UserSettingsModalComponent } from '../user-settings-modal/user-settings-modal.component';

@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [UpperCasePipe, TranslateModule, UserSettingsModalComponent],
  templateUrl: './user-menu.component.html',
  styleUrl: './user-menu.component.scss',
})
export class UserMenuComponent {
  private readonly userStore = inject(UserStore);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly user = this.userStore.user;
  menuOpen = false;
  showSettingsModal = signal(false);

  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }

  openSettings() {
    this.showSettingsModal.set(true);
    this.menuOpen = false;
  }

  closeSettings() {
    this.showSettingsModal.set(false);
  }

  logout() {
    performLogout(this.auth, this.userStore, this.router);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-menu-container')) {
      this.menuOpen = false;
    }
  }
}

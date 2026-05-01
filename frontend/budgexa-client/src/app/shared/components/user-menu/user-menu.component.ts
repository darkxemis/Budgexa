import { Component, inject, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserStore } from '../../../core/state/user.store';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { performLogout } from '../../../core/utils/auth.utils';

@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './user-menu.component.html',
  styleUrl: './user-menu.component.scss',
})
export class UserMenuComponent {
  private readonly userStore = inject(UserStore);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly user = this.userStore.user;
  menuOpen = false;
  private readonly elRef = inject(ElementRef);

  toggleMenu() {
    this.menuOpen = !this.menuOpen;
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

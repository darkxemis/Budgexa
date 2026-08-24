import { Component, inject, signal, ChangeDetectionStrategy, ElementRef, viewChild } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { UserStore } from '../../../core/state/user.store';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { performLogout } from '../../../core/utils/auth.utils';
import { UserSettingsModalComponent } from '../user-settings-modal/user-settings-modal.component';
import { UserAvatarComponent } from '../user-avatar/user-avatar.component';
import { IconComponent } from '../icon/icon.component';
import { ToastService } from '../toast/toast.service';
import { ToastType } from '../toast/toast.type';

@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [TranslateModule, UserSettingsModalComponent, UserAvatarComponent, IconComponent],
  templateUrl: './user-menu.component.html',
  styleUrl: './user-menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:click)': 'onDocumentClick($event)',
  },
})
export class UserMenuComponent {
  private readonly userStore = inject(UserStore);
  private readonly userService = inject(UserService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  readonly user = this.userStore.user;
  readonly menuOpen = signal(false);
  readonly imageMenuOpen = signal(false);
  readonly showSettingsModal = signal(false);
  readonly savingImage = signal(false);

  readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');

  toggleMenu() {
    this.menuOpen.update(v => !v);
    this.imageMenuOpen.set(false);
  }

  toggleImageMenu(event: MouseEvent) {
    event.stopPropagation();
    this.imageMenuOpen.update(v => !v);
    this.menuOpen.set(false);
  }

  openSettings() {
    this.showSettingsModal.set(true);
    this.menuOpen.set(false);
  }

  closeSettings() {
    this.showSettingsModal.set(false);
  }

  triggerFileInput() {
    this.fileInput()?.nativeElement.click();
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.toastService.show('invalidImageType', ToastType.Error);
      return;
    }

    const maxSize = 5 * 1024 * 1024;
    if (file.size > maxSize) {
      this.toastService.show('imageTooLarge', ToastType.Error);
      return;
    }

    this.savingImage.set(true);
    this.userService.uploadProfileImage(file).subscribe({
      next: () => {
        this.savingImage.set(false);
        this.imageMenuOpen.set(false);
        this.toastService.show('profileImageUpdated', ToastType.Success);
      },
      error: () => {
        this.savingImage.set(false);
        this.toastService.show('profileImageUpdateFailed', ToastType.Error);
      },
    });

    input.value = '';
  }

  deleteProfileImage() {
    this.savingImage.set(true);
    this.userService.deleteProfileImage().subscribe({
      next: () => {
        this.savingImage.set(false);
        this.imageMenuOpen.set(false);
        this.toastService.show('profileImageDeleted', ToastType.Success);
      },
      error: () => {
        this.savingImage.set(false);
        this.toastService.show('profileImageUpdateFailed', ToastType.Error);
      },
    });
  }

  logout() {
    performLogout(this.auth, this.userStore, this.router);
  }

  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-menu-container')) {
      this.menuOpen.set(false);
      this.imageMenuOpen.set(false);
    }
  }
}

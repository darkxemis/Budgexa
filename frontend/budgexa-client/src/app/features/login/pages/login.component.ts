import { Component, signal, ChangeDetectionStrategy, inject, effect, computed } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { UserStore } from '../../../core/state/user.store';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ToastType } from '../../../shared/components/toast/toast.type';
import { TranslateService } from '@ngx-translate/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  protected readonly email = signal('');
  protected readonly password = signal('');
  protected readonly error = signal('');

  private readonly auth = inject(AuthService);
  private readonly userStore = inject(UserStore);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly translate = inject(TranslateService);

  ngOnInit() {
    const user = this.userStore.user();
    if (user && user.id) {
      this.router.navigate(['/dashboard']);
    }
  }

  login() {
    this.auth.login({
      email: this.email(),
      password: this.password(),
    }).subscribe({
      next: () => {
        this.toast.show(this.translate.instant('welcome'), ToastType.Success);
        this.router.navigate(['/dashboard']);
      }
    });
  }
}

import { Component, signal, ChangeDetectionStrategy, inject, effect, computed } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { UserStore } from '../../state/user.store';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  protected readonly email = signal('');
  protected readonly password = signal('');
  protected readonly error = signal('');

  private readonly auth = inject(AuthService);
  private readonly userStore = inject(UserStore);
  private readonly router = inject(Router);

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
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        console.error(err);
      }
    });
  }
}

import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserStore } from '../state/user.store';

/**
 * Performs a complete logout: clears session, store, and redirects to login
 */
export function performLogout(
  authService: AuthService,
  userStore: UserStore,
  router: Router
): void {
  authService.logout();
  userStore.clearUser();
  router.navigate(['/login']);
}

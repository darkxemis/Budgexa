import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { of } from 'rxjs';
import { catchError, map } from 'rxjs';
import { UserService } from '../services/user.service';
import { ADMIN_ROLES } from '../constants/roles.constants';

/**
 * Allows navigation only when the current user has one of the admin roles
 * (administrator or superadministrator). Redirects to `/dashboard` otherwise,
 * and to `/login` if there is no authenticated user.
 */
export const adminGuard: CanActivateFn = () => {
  const userService = inject(UserService);
  const router = inject(Router);

  return userService.getUser().pipe(
    map(user => {
      if (!user || !user.id) {
        return router.createUrlTree(['/login']);
      }
      const hasAdminRole = user.roles?.some(role =>
        ADMIN_ROLES.includes(role as (typeof ADMIN_ROLES)[number])
      );
      return hasAdminRole ? true : router.createUrlTree(['/dashboard']);
    }),
    catchError(() => of(router.createUrlTree(['/login'])))
  );
};

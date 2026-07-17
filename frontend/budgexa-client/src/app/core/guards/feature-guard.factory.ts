import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { of } from 'rxjs';
import { catchError, map } from 'rxjs';
import { UserService } from '../services/user.service';
import { RoleName } from '../constants/roles.constants';

/**
 * Factory that produces a route guard for a feature area following the same pattern:
 *  - Not authenticated -> redirect to `/login`.
 *  - Authenticated but missing one of `allowedRoles` -> redirect to `/dashboard`.
 *  - Authenticated (and role match when `allowedRoles` is provided) -> allow navigation.
 *
 * When `allowedRoles` is omitted the guard only enforces authentication, which keeps
 * the door open for feature guards that don't need role checks while still exposing a
 * dedicated `xxxGuard` symbol per feature to keep routing consistent.
 */
export function createFeatureGuard(allowedRoles?: readonly RoleName[]): CanActivateFn {
  return () => {
    const userService = inject(UserService);
    const router = inject(Router);

    return userService.getUser().pipe(
      map(user => {
        if (!user || !user.id) {
          return router.createUrlTree(['/login']);
        }
        if (!allowedRoles || allowedRoles.length === 0) {
          return true;
        }
        const hasRole = user.roles?.some(role => allowedRoles.includes(role as RoleName));
        return hasRole ? true : router.createUrlTree(['/dashboard']);
      }),
      catchError(() => of(router.createUrlTree(['/login'])))
    );
  };
}

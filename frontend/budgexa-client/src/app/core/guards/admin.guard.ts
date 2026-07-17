import { CanActivateFn } from '@angular/router';
import { ADMIN_ROLES } from '../constants/roles.constants';
import { createFeatureGuard } from './feature-guard.factory';

/**
 * Allows navigation only when the current user has one of the admin roles
 * (administrator or superadministrator). Redirects to `/dashboard` otherwise,
 * and to `/login` if there is no authenticated user.
 */
export const adminGuard: CanActivateFn = createFeatureGuard(ADMIN_ROLES);

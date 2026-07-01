import { CanActivateFn } from '@angular/router';
import { ALL_ROLES } from '../constants/roles.constants';
import { createFeatureGuard } from './feature-guard.factory';

/**
 * Allows navigation only when the current user has one of the roles allowed
 * to manage customers (freelance, administrator or superadministrator).
 * Redirects to `/dashboard` when authenticated but not authorized, and to
 * `/login` when there is no authenticated user.
 */
export const customersGuard: CanActivateFn = createFeatureGuard(ALL_ROLES);

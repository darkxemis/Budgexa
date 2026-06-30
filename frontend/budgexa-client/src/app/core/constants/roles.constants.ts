/**
 * Canonical role names. Must stay in sync with backend `Budgexa.Domain.Constants.RoleNames`.
 */
export const ROLE_NAMES = {
  Freelance: 'freelance',
  Administrator: 'administrator',
  SuperAdministrator: 'superadministrator',
} as const;

export type RoleName = (typeof ROLE_NAMES)[keyof typeof ROLE_NAMES];

/** Roles allowed to manage the users section. */
export const ADMIN_ROLES: readonly RoleName[] = [
  ROLE_NAMES.Administrator,
  ROLE_NAMES.SuperAdministrator,
];

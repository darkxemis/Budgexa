/**
 * Branded type for GUID/UUID strings to provide type safety
 * @example
 * const userId: Guid = '123e4567-e89b-12d3-a456-426614174000' as Guid;
 */
export type Guid = string & { readonly __brand: 'Guid' };

/**
 * Type guard to validate GUID format
 */
export function isGuid(value: string): value is Guid {
  const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
  return guidRegex.test(value);
}

/**
 * Convert string to Guid (use with caution, validates format)
 */
export function toGuid(value: string): Guid {
  if (!isGuid(value)) {
    throw new Error(`Invalid GUID format: ${value}`);
  }
  return value as Guid;
}

/**
 * Generate a new GUID v4 (requires crypto API)
 */
export function newGuid(): Guid {
  return crypto.randomUUID() as Guid;
}

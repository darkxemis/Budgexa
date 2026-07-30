import { Guid } from './guid.model';

/**
 * Generic selector option model
 * Used across the application for dropdowns, filters, and selectors
 */
export interface SelectorOption {
  id: Guid;
  name: string;
}

/**
 * Integer-based selector option model
 * Used specifically for unit measures where IDs are integers instead of GUIDs
 */
export interface SelectorOptionInt {
  id: number;
  name: string;
}

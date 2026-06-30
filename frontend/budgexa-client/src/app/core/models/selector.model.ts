import { Guid } from './guid.model';

/**
 * Generic selector option model
 * Used across the application for dropdowns, filters, and selectors
 */
export interface SelectorOption {
  id: Guid;
  name: string;
}

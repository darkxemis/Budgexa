import { Guid } from '../../../core/models/guid.model';

/** ISO date string in the format `YYYY-MM-DD` (matches backend `DateOnly`). */
export type DateOnlyString = string;

export interface BudgetLineDto {
  id: Guid;
  itemId: Guid | null;
  sortOrder: number;
  description: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  taxRate: number;
  subtotal: number;
  taxAmount: number;
  total: number;
}

export interface BudgetLineUpsertDto {
  id: Guid | null;
  itemId: Guid | null;
  sortOrder: number;
  description: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  taxRate: number;
}

export interface BudgetDto {
  id: Guid;
  number: string;
  issueDate: DateOnlyString;
  validUntil: DateOnlyString | null;
  currency: string;
  subtotal: number;
  taxAmount: number;
  total: number;
  notes: string | null;
  termsAndConditions: string | null;
  companyId: Guid;
  companyName: string;
  customerId: Guid;
  customerName: string;
  statusId: Guid;
  statusName: string;
  lines: BudgetLineDto[];
  createdAt: Date;
  updatedAt: Date | null;
}

export interface BudgetCreateDto {
  customerId: Guid;
  number: string;
  issueDate: DateOnlyString;
  validUntil: DateOnlyString | null;
  currency: string;
  notes: string | null;
  termsAndConditions: string | null;
  lines: BudgetLineUpsertDto[];
}

export interface BudgetUpdateDto {
  customerId: Guid;
  number: string;
  issueDate: DateOnlyString;
  validUntil: DateOnlyString | null;
  currency: string;
  notes: string | null;
  termsAndConditions: string | null;
  lines: BudgetLineUpsertDto[];
}

export interface BudgetGridDto {
  id: Guid;
  number: string;
  issueDate: DateOnlyString;
  validUntil: DateOnlyString | null;
  currency: string;
  total: number;
  customerId: Guid;
  customerName: string;
  statusId: Guid;
  statusName: string;
  createdAt: Date;
  updatedAt: Date | null;
}

/**
 * Calculates the totals for a single budget line following the backend formula:
 *   lineSubtotal = quantity * unitPrice * (1 - discount/100)
 *   lineTax      = lineSubtotal * (taxRate/100)
 *   lineTotal    = lineSubtotal + lineTax
 */
export function computeLineTotals(input: {
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  taxRate: number;
}): { subtotal: number; taxAmount: number; total: number } {
  const quantity = Number(input.quantity) || 0;
  const unitPrice = Number(input.unitPrice) || 0;
  const discount = Number(input.discountPercentage) || 0;
  const taxRate = Number(input.taxRate) || 0;

  const subtotal = quantity * unitPrice * (1 - discount / 100);
  const taxAmount = subtotal * (taxRate / 100);
  const total = subtotal + taxAmount;

  return { subtotal, taxAmount, total };
}

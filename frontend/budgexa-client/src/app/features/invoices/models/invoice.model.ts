import { Guid } from '../../../core/models/guid.model';

/** ISO date string in the format `YYYY-MM-DD` (matches backend `DateOnly`). */
export type DateOnlyString = string;

/**
 * Mirrors the backend `Budgexa.Domain.Enums.PaymentMethod` enum.
 * Kept as numeric literals because the API serializes the enum as its integer value.
 */
export enum PaymentMethod {
  BankTransfer = 1,
  Cash = 2,
  Card = 3,
  DirectDebit = 4,
  Bizum = 5,
  Other = 99,
}

export interface InvoiceLineDto {
  id: Guid;
  itemId: Guid | null;
  sortOrder: number;
  description: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  taxRate: number;
  withholdingRate: number;
  subtotal: number;
  taxAmount: number;
  withholdingAmount: number;
  total: number;
}

export interface InvoiceLineUpsertDto {
  id: Guid | null;
  itemId: Guid | null;
  sortOrder: number;
  description: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  taxRate: number;
  withholdingRate: number;
}

export interface InvoiceDto {
  id: Guid;
  series: string;
  number: string;
  issueDate: DateOnlyString;
  dueDate: DateOnlyString;
  currency: string;
  subtotal: number;
  taxAmount: number;
  withholdingAmount: number;
  total: number;
  amountPaid: number;
  amountDue: number;
  isFullyPaid: boolean;
  paymentMethod: PaymentMethod | null;
  paymentReference: string | null;
  notes: string | null;
  companyId: Guid;
  companyName: string;
  customerId: Guid;
  customerName: string;
  budgetId: Guid | null;
  budgetNumber: string | null;
  statusId: Guid;
  statusName: string;
  lines: InvoiceLineDto[];
  createdAt: Date;
  updatedAt: Date | null;
}

export interface InvoiceCreateDto {
  customerId: Guid;
  budgetId: Guid | null;
  series: string;
  number: string;
  issueDate: DateOnlyString;
  dueDate: DateOnlyString;
  currency: string;
  notes: string | null;
  lines: InvoiceLineUpsertDto[];
}

export interface InvoiceUpdateDto {
  customerId: Guid;
  series: string;
  number: string;
  issueDate: DateOnlyString;
  dueDate: DateOnlyString;
  currency: string;
  notes: string | null;
  lines: InvoiceLineUpsertDto[];
}

export interface InvoiceGridDto {
  id: Guid;
  series: string;
  number: string;
  issueDate: DateOnlyString;
  dueDate: DateOnlyString;
  currency: string;
  total: number;
  amountPaid: number;
  amountDue: number;
  customerId: Guid;
  customerName: string;
  statusId: Guid;
  statusName: string;
  createdAt: Date;
  updatedAt: Date | null;
}

export interface ChangeInvoiceStatusDto {
  statusId: Guid;
}

export interface RegisterInvoicePaymentDto {
  amount: number;
  method: PaymentMethod;
  reference: string | null;
}

/**
 * Calculates the totals for a single invoice line following the backend formula:
 *   lineSubtotal          = quantity * unitPrice * (1 - discount/100)
 *   lineTax               = lineSubtotal * (taxRate/100)
 *   lineWithholding       = lineSubtotal * (withholdingRate/100)
 *   lineTotal             = lineSubtotal + lineTax - lineWithholding
 */
export function computeLineTotals(input: {
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  taxRate: number;
  withholdingRate: number;
}): { subtotal: number; taxAmount: number; withholdingAmount: number; total: number } {
  const quantity = Number(input.quantity) || 0;
  const unitPrice = Number(input.unitPrice) || 0;
  const discount = Number(input.discountPercentage) || 0;
  const tax = Number(input.taxRate) || 0;
  const withholding = Number(input.withholdingRate) || 0;

  const subtotal = round2(quantity * unitPrice * (1 - discount / 100));
  const taxAmount = round2(subtotal * (tax / 100));
  const withholdingAmount = round2(subtotal * (withholding / 100));
  const total = round2(subtotal + taxAmount - withholdingAmount);

  return { subtotal, taxAmount, withholdingAmount, total };
}

function round2(value: number): number {
  return Math.round(value * 100) / 100;
}

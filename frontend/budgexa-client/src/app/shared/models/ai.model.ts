import { Guid } from '../../core/models/guid.model';
import { DateOnlyString } from '../../features/budgets/models/budget.model';

/** Raw AI-extracted item before matching against DB. */
export interface AiItemDto {
  productName: string;
  quantity: number;
  discountPercentage?: number | null;
}

/** Item resolved after matching with company items in DB. */
export interface MatchedItemDto {
  itemId: Guid | null;
  productName: string;
  quantity: number;
  discountPercentage: number | null;
  unitPrice: number | null;
  taxRate: number | null;
  unit: string | null;
  unitMeasure: number | null;
}

/** Request to generate a budget with AI. */
export interface PrivateBudgetAiRequestDto {
  userRequest: string;
}

/** Response from AI budget generation. */
export interface PrivateBudgetAiResponseDto {
  customerId: Guid | null;
  number: string | null;
  issueDate: DateOnlyString | null;
  validUntil: DateOnlyString | null;
  currency: string | null;
  notes: string | null;
  termsAndConditions: string | null;
  items: MatchedItemDto[];
}

/** Request to generate an invoice with AI. */
export interface PrivateInvoiceAiRequestDto {
  userRequest: string;
}

/** Response from AI invoice generation. */
export interface PrivateInvoiceAiResponseDto {
  customerId: Guid | null;
  series: string | null;
  number: string | null;
  issueDate: DateOnlyString | null;
  dueDate: DateOnlyString | null;
  currency: string | null;
  notes: string | null;
  items: MatchedItemDto[];
}

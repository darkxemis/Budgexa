import { Guid } from '../../../core/models/guid.model';

/** Request DTO for the AI generation endpoint. */
export interface PublicBudgetAiRequest {
  companyId: Guid;
  userRequest: string;
}

/** Single item returned by the AI generation endpoint. */
export interface PublicBudgetAiItem {
  itemId: Guid;
  productName: string;
  unitPrice: number;
  taxRate: number;
  unit: string;
  quantity: number;
}

/** Response DTO from the AI generation endpoint. */
export interface PublicBudgetAiResponse {
  originalRequest: string;
  items: PublicBudgetAiItem[];
  model: string;
}

/** A single line in the confirm-and-download request. */
export interface ConfirmPublicBudgetLine {
  itemId: Guid;
  quantity: number;
}

/** Request DTO for the confirm-and-download endpoint. */
export interface ConfirmPublicBudgetRequest {
  companyId: Guid;
  customerFirstName: string;
  customerLastName: string;
  customerPhone: string | null;
  customerEmail: string | null;
  lines: ConfirmPublicBudgetLine[];
}

/** DTO returned by the public items search endpoint. */
export interface PublicItemDto {
  itemId: Guid;
  productName: string;
  unitPrice: number;
  taxRate: number;
  unit: string;
}

import { Injectable, signal } from '@angular/core';
import { PublicBudgetAiResponse } from '../models/public-budget.model';
import { Guid } from '../../../core/models/guid.model';

@Injectable({ providedIn: 'root' })
export class PublicBudgetStore {
  private readonly _aiResponse = signal<PublicBudgetAiResponse | null>(null);
  private readonly _companyId = signal<Guid | null>(null);

  readonly aiResponse = this._aiResponse.asReadonly();
  readonly companyId = this._companyId.asReadonly();

  setAiResponse(response: PublicBudgetAiResponse): void {
    this._aiResponse.set(response);
  }

  setCompanyId(id: Guid): void {
    this._companyId.set(id);
  }

  clear(): void {
    this._aiResponse.set(null);
    this._companyId.set(null);
  }
}

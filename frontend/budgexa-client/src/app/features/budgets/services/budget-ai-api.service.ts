import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PrivateBudgetAiRequestDto, PrivateBudgetAiResponseDto } from '../../../shared/models/ai.model';

@Injectable({ providedIn: 'root' })
export class BudgetAiApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/budgets`;

  /**
   * Generates a budget from a natural language prompt using AI.
   * Endpoint: POST /api/v1/budgets/generate-with-ai
   */
  generateWithAi(request: PrivateBudgetAiRequestDto): Observable<PrivateBudgetAiResponseDto> {
    return this.http.post<PrivateBudgetAiResponseDto>(
      `${this.baseUrl}/generate-with-ai`,
      request,
      { withCredentials: true }
    );
  }
}

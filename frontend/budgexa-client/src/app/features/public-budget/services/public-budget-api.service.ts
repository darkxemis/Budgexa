import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Guid } from '../../../core/models/guid.model';
import {
  PublicBudgetAiRequest,
  PublicBudgetAiResponse,
  PublicItemDto,
  ConfirmPublicBudgetRequest,
} from '../models/public-budget.model';

@Injectable({ providedIn: 'root' })
export class PublicBudgetApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/public-budgets`;

  generateWithAi(request: PublicBudgetAiRequest): Observable<PublicBudgetAiResponse> {
    return this.http.post<PublicBudgetAiResponse>(`${this.baseUrl}/ai-generate`, request);
  }

  confirmAndDownload(request: ConfirmPublicBudgetRequest, languageCode: string): Observable<Blob> {
    const headers = new HttpHeaders().set('X-Language-Code', languageCode);
    return this.http.post(`${this.baseUrl}/confirm-and-download`, request, {
      headers,
      responseType: 'blob',
    });
  }

  getItems(companyId: Guid, searchQuery?: string): Observable<PublicItemDto[]> {
    let url = `${this.baseUrl}/${companyId}/items`;
    if (searchQuery && searchQuery.trim().length > 0) {
      url += `?searchQuery=${encodeURIComponent(searchQuery.trim())}`;
    }
    return this.http.get<PublicItemDto[]>(url);
  }
}


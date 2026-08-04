import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PrivateInvoiceAiRequestDto, PrivateInvoiceAiResponseDto } from '../../../shared/models/ai.model';

@Injectable({ providedIn: 'root' })
export class InvoiceAiApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/invoices`;

  /**
   * Generates an invoice from a natural language prompt using AI.
   * Endpoint: POST /api/v1/invoices/generate-with-ai
   */
  generateWithAi(request: PrivateInvoiceAiRequestDto): Observable<PrivateInvoiceAiResponseDto> {
    return this.http.post<PrivateInvoiceAiResponseDto>(
      `${this.baseUrl}/generate-with-ai`,
      request,
      { withCredentials: true }
    );
  }
}

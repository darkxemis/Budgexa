import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Guid } from '../../../core/models/guid.model';
import {
  InvoiceCreateDto,
  InvoiceDto,
  InvoiceUpdateDto,
  RegisterInvoicePaymentDto,
} from '../models/invoice.model';

@Injectable({ providedIn: 'root' })
export class InvoiceApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/invoices`;

  getById(id: Guid): Observable<InvoiceDto> {
    return this.http.get<InvoiceDto>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }

  create(dto: InvoiceCreateDto): Observable<InvoiceDto> {
    return this.http.post<InvoiceDto>(this.baseUrl, dto, {
      withCredentials: true,
    });
  }

  update(id: Guid, dto: InvoiceUpdateDto): Observable<InvoiceDto> {
    return this.http.put<InvoiceDto>(`${this.baseUrl}/${id}`, dto, {
      withCredentials: true,
    });
  }

  delete(id: Guid): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }

  changeStatus(id: Guid, statusId: Guid): Observable<InvoiceDto> {
    return this.http.patch<InvoiceDto>(
      `${this.baseUrl}/${id}/status`,
      { statusId },
      { withCredentials: true }
    );
  }

  registerPayment(id: Guid, dto: RegisterInvoicePaymentDto): Observable<InvoiceDto> {
    return this.http.post<InvoiceDto>(`${this.baseUrl}/${id}/payments`, dto, {
      withCredentials: true,
    });
  }
}

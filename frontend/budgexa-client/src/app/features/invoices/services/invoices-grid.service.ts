import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GridRequestDto, GridResponseDto } from '../../../core/models/grid.model';
import { InvoiceGridDto } from '../models/invoice.model';

@Injectable({ providedIn: 'root' })
export class InvoicesGridService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/invoices`;

  getGrid(request: GridRequestDto): Observable<GridResponseDto<InvoiceGridDto>> {
    return this.http.post<GridResponseDto<InvoiceGridDto>>(
      `${this.baseUrl}/grid`,
      request,
      { withCredentials: true }
    );
  }
}

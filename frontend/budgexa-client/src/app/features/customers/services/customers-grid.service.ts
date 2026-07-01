import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GridRequestDto, GridResponseDto } from '../../../core/models/grid.model';
import { CustomerGridDto } from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class CustomersGridService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/customers`;

  getGrid(request: GridRequestDto): Observable<GridResponseDto<CustomerGridDto>> {
    return this.http.post<GridResponseDto<CustomerGridDto>>(
      `${this.baseUrl}/grid`,
      request,
      { withCredentials: true }
    );
  }
}

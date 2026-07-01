import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Guid } from '../../../core/models/guid.model';
import { CustomerCreateDto, CustomerDto, CustomerUpdateDto } from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class CustomerApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/customers`;

  getById(id: Guid): Observable<CustomerDto> {
    return this.http.get<CustomerDto>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }

  create(dto: CustomerCreateDto): Observable<CustomerDto> {
    return this.http.post<CustomerDto>(this.baseUrl, dto, {
      withCredentials: true,
    });
  }

  update(id: Guid, dto: CustomerUpdateDto): Observable<CustomerDto> {
    return this.http.put<CustomerDto>(`${this.baseUrl}/${id}`, dto, {
      withCredentials: true,
    });
  }

  delete(id: Guid): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }
}

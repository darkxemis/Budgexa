import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Guid } from '../../../core/models/guid.model';
import { BudgetCreateDto, BudgetDto, BudgetUpdateDto } from '../models/budget.model';

@Injectable({ providedIn: 'root' })
export class BudgetApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/budgets`;

  getById(id: Guid): Observable<BudgetDto> {
    return this.http.get<BudgetDto>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }

  create(dto: BudgetCreateDto): Observable<BudgetDto> {
    return this.http.post<BudgetDto>(this.baseUrl, dto, {
      withCredentials: true,
    });
  }

  update(id: Guid, dto: BudgetUpdateDto): Observable<BudgetDto> {
    return this.http.put<BudgetDto>(`${this.baseUrl}/${id}`, dto, {
      withCredentials: true,
    });
  }

  delete(id: Guid): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }

  changeStatus(id: Guid, statusId: Guid): Observable<BudgetDto> {
    return this.http.patch<BudgetDto>(
      `${this.baseUrl}/${id}/status`,
      { statusId },
      { withCredentials: true }
    );
  }
}

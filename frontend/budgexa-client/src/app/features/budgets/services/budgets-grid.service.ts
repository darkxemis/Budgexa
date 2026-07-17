import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GridRequestDto, GridResponseDto } from '../../../core/models/grid.model';
import { BudgetGridDto } from '../models/budget.model';

@Injectable({ providedIn: 'root' })
export class BudgetsGridService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/budgets`;

  getGrid(request: GridRequestDto): Observable<GridResponseDto<BudgetGridDto>> {
    return this.http.post<GridResponseDto<BudgetGridDto>>(
      `${this.baseUrl}/grid`,
      request,
      { withCredentials: true }
    );
  }
}

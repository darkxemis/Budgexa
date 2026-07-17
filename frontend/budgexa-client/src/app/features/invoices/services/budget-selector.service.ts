import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SelectorOption } from '../../../core/models/selector.model';

/**
 * Lightweight service that consumes the budgets selector endpoint.
 * Used by the invoice form to pick an origin budget (optional).
 */
@Injectable({ providedIn: 'root' })
export class BudgetSelectorService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/budgets`;

  getForSelector(searchQuery?: string): Observable<SelectorOption[]> {
    let url = `${this.baseUrl}/selector`;
    if (searchQuery && searchQuery.trim().length > 0) {
      url += `?searchQuery=${encodeURIComponent(searchQuery.trim())}`;
    }
    return this.http.get<SelectorOption[]>(url, {
      withCredentials: true,
    });
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SelectorOption } from '../../../core/models/selector.model';

/**
 * Lightweight service that consumes the items selector endpoint.
 * Used by budget/invoice forms to pick an item.
 */
@Injectable({ providedIn: 'root' })
export class ItemSelectorService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/items`;

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

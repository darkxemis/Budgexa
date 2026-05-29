import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { SelectorOption } from '../models/selector.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class StatusApiService {
  private readonly http = inject(HttpClient);

  getStatusForSelector(group: string, searchQuery?: string): Observable<SelectorOption[]> {
    let url = `${environment.apiUrl}/status/selector?group=${group}`;
    if (searchQuery) {
      url += `&searchQuery=${encodeURIComponent(searchQuery)}`;
    }
    return this.http.get<SelectorOption[]>(url, {
      withCredentials: true,
    });
  }
}

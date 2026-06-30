import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Language } from '../models/user.model';
import { SelectorOption } from '../models/selector.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LanguageApiService {
  private readonly http = inject(HttpClient);

  getLanguages(): Observable<Language[]> {
    return this.http.get<Language[]>(`${environment.apiUrl}/languages`, {
      withCredentials: true,
    });
  }

  getLanguagesForSelector(searchQuery?: string): Observable<SelectorOption[]> {
    let url = `${environment.apiUrl}/languages/selector`;
    if (searchQuery) {
      url += `?searchQuery=${encodeURIComponent(searchQuery)}`;
    }
    return this.http.get<SelectorOption[]>(url, {
      withCredentials: true,
    });
  }
}

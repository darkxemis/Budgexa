import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Language } from '../models/user.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LanguageApiService {
  private readonly http = inject(HttpClient);

  getLanguages(): Observable<Language[]> {
    return this.http.get<Language[]>(`${environment.apiUrl}/languages`, {
      withCredentials: true,
    });
  }
}

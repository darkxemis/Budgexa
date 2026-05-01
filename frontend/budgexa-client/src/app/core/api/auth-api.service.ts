import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AuthCredentials } from '../services/auth.service';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);

  login(credentials: AuthCredentials): Observable<{ token?: string }> {
    return this.http.post<{ token?: string }>(`${environment.apiUrl}/auth/login`, credentials, {
      withCredentials: true,
    });
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/logout`, {}, {
      withCredentials: true,
    });
  }

  refreshToken(): Observable<{ token?: string }> {
    return this.http.post<{ token?: string }>(`${environment.apiUrl}/auth/refresh`, {}, {
      withCredentials: true,
    });
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { UserProfileResult, UpdateCurrentUserDto } from '../models/user.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserApiService {
  private readonly http = inject(HttpClient);

  me(): Observable<UserProfileResult> {
    return this.http.get<UserProfileResult>(`${environment.apiUrl}/users/me`, {
      withCredentials: true,
    });
  }

  updateMe(dto: UpdateCurrentUserDto): Observable<UserProfileResult> {
    return this.http.patch<UserProfileResult>(`${environment.apiUrl}/users/me`, dto, {
      withCredentials: true,
    });
  }
}

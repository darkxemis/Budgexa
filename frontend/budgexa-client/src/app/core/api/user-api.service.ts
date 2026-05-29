import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserProfileResult, UpdateCurrentUserDto } from '../models/user.model';

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

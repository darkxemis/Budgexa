import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  UserProfileResult,
  UpdateCurrentUserDto,
  UserCreateDto,
  UserUpdateDto,
  UserDetailDto,
} from '../models/user.model';
import { Guid } from '../models/guid.model';

@Injectable({ providedIn: 'root' })
export class UserApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  me(): Observable<UserProfileResult> {
    return this.http.get<UserProfileResult>(`${this.baseUrl}/me`, {
      withCredentials: true,
    });
  }

  updateMe(dto: UpdateCurrentUserDto): Observable<UserProfileResult> {
    return this.http.patch<UserProfileResult>(`${this.baseUrl}/me`, dto, {
      withCredentials: true,
    });
  }

  getById(id: Guid): Observable<UserDetailDto> {
    return this.http.get<UserDetailDto>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }

  create(dto: UserCreateDto): Observable<UserDetailDto> {
    return this.http.post<UserDetailDto>(this.baseUrl, dto, {
      withCredentials: true,
    });
  }

  update(id: Guid, dto: UserUpdateDto): Observable<UserDetailDto> {
    return this.http.put<UserDetailDto>(`${this.baseUrl}/${id}`, dto, {
      withCredentials: true,
    });
  }

  delete(id: Guid): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }
}

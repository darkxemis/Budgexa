import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RoleInfo } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class RoleApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/roles`;

  getAllRoles(): Observable<RoleInfo[]> {
    return this.http.get<RoleInfo[]>(this.baseUrl, { withCredentials: true });
  }
}

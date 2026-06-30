import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GridRequestDto, GridResponseDto } from '../../../core/models/grid.model';
import { UserGridDto } from '../models/user-grid.model';

@Injectable({ providedIn: 'root' })
export class UsersGridService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  getGrid(request: GridRequestDto): Observable<GridResponseDto<UserGridDto>> {
    return this.http.post<GridResponseDto<UserGridDto>>(
      `${this.baseUrl}/grid`,
      request,
      { withCredentials: true }
    );
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GridRequestDto, GridResponseDto } from '../../../core/models/grid.model';
import { ItemGridDto } from '../models/item.model';

@Injectable({ providedIn: 'root' })
export class ItemsGridService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/items`;

  getGrid(request: GridRequestDto): Observable<GridResponseDto<ItemGridDto>> {
    return this.http.post<GridResponseDto<ItemGridDto>>(
      `${this.baseUrl}/grid`,
      request,
      { withCredentials: true }
    );
  }
}

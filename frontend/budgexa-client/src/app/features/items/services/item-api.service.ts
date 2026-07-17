import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Guid } from '../../../core/models/guid.model';
import { ItemCreateDto, ItemDto, ItemUpdateDto } from '../models/item.model';

@Injectable({ providedIn: 'root' })
export class ItemApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/items`;

  getById(id: Guid): Observable<ItemDto> {
    return this.http.get<ItemDto>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }

  create(dto: ItemCreateDto): Observable<ItemDto> {
    return this.http.post<ItemDto>(this.baseUrl, dto, {
      withCredentials: true,
    });
  }

  update(id: Guid, dto: ItemUpdateDto): Observable<ItemDto> {
    return this.http.put<ItemDto>(`${this.baseUrl}/${id}`, dto, {
      withCredentials: true,
    });
  }

  delete(id: Guid): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, {
      withCredentials: true,
    });
  }
}

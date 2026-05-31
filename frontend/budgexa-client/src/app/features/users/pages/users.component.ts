import { Component, inject, signal, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';
import { DataGridComponent } from '../../../shared/components/data-grid/data-grid.component';
import { GridRequestDto, GridColumnDef } from '../../../core/models/grid.model';
import { UsersGridService } from '../services/users-grid.service';
import { UserGridDto } from '../models/user-grid.model';
import { USERS_GRID_COLUMNS } from '../config/users-grid-columns.config';
import { LanguageApiService } from '../../../core/api/language-api.service';
import { StatusApiService } from '../../../core/api/status-api.service';
import { SelectorOption } from '../../../core/models/selector.model';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, TranslateModule, DataGridComponent],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  private readonly usersGridService = inject(UsersGridService);
  private readonly languageApiService = inject(LanguageApiService);
  private readonly statusApiService = inject(StatusApiService);
  
  @ViewChild(DataGridComponent) grid?: DataGridComponent<UserGridDto>;
  
  protected readonly loading = signal(false);
  protected columns: GridColumnDef<UserGridDto>[] = [];

  ngOnInit(): void {
    this.initializeColumns();
  }

  private initializeColumns(): void {
    // Configure columns with filter options for select types
    this.columns = USERS_GRID_COLUMNS.map(column => {
      // Language selector: filters by languageId
      if (column.field === 'languageName') {
        return {
          ...column,
          filterField: 'languageId',
          filterType: 'select',
          filterOptions: this.loadLanguageOptions.bind(this)
        };
      }
      // Status selector: filters by statusId (group "Base")
      if (column.field === 'statusName') {
        return {
          ...column,
          filterField: 'statusId',
          filterType: 'select',
          filterOptions: this.loadStatusOptions.bind(this)
        };
      }
      // Other columns remain as text filters (companyName, etc.)
      return column;
    });
  }

  private async loadLanguageOptions(searchQuery?: string): Promise<SelectorOption[]> {
    const languages = await firstValueFrom(this.languageApiService.getLanguagesForSelector(searchQuery));
    return languages;
  }

  private async loadStatusOptions(searchQuery?: string): Promise<SelectorOption[]> {
    const statuses = await firstValueFrom(this.statusApiService.getStatusForSelector('Base', searchQuery));
    return statuses;
  }

  protected onGridStateChange(request: GridRequestDto): void {
    this.loading.set(true);
    
    this.usersGridService.getGrid(request).subscribe({
      next: (response) => {
        this.grid?.setData(response);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading users grid:', error);
        this.loading.set(false);
      }
    });
  }
}

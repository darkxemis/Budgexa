import { Component, inject, signal, viewChild, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { DataGridComponent } from '../../../shared/components/data-grid/data-grid.component';
import { GridRequestDto, GridColumnDef, GridAction } from '../../../core/models/grid.model';
import { UsersGridService } from '../services/users-grid.service';
import { UserGridDto } from '../models/user-grid.model';
import { USERS_GRID_COLUMNS } from '../config/users-grid-columns.config';
import { LanguageApiService } from '../../../core/api/language-api.service';
import { StatusApiService } from '../../../core/api/status-api.service';
import { UserApiService } from '../../../core/api/user-api.service';
import { SelectorOption } from '../../../core/models/selector.model';
import { Guid } from '../../../core/models/guid.model';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  UserFormModalComponent,
  UserFormMode,
} from '../../../shared/components/user-form-modal/user-form-modal.component';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ToastType } from '../../../shared/components/toast/toast.type';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    TranslateModule,
    DataGridComponent,
    IconComponent,
    ConfirmDialogComponent,
    UserFormModalComponent,
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent implements OnInit {
  private readonly usersGridService = inject(UsersGridService);
  private readonly languageApiService = inject(LanguageApiService);
  private readonly statusApiService = inject(StatusApiService);
  private readonly userApiService = inject(UserApiService);
  private readonly toast = inject(ToastService);

  grid = viewChild(DataGridComponent<UserGridDto>);

  protected readonly loading = signal(false);
  protected columns: GridColumnDef<UserGridDto>[] = [];
  protected actions: GridAction<UserGridDto>[] = [];

  // Form modal state
  protected readonly formModalOpen = signal(false);
  protected readonly formMode = signal<UserFormMode>('create');
  protected readonly editingUserId = signal<Guid | null>(null);

  // Delete confirmation state
  protected readonly deleteDialogOpen = signal(false);
  protected readonly deleting = signal(false);
  protected readonly userToDelete = signal<UserGridDto | null>(null);
  protected readonly deleteMessageParams = signal<Record<string, unknown> | undefined>(undefined);

  ngOnInit(): void {
    this.initializeColumns();
    this.initializeActions();
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

  private initializeActions(): void {
    this.actions = [
      {
        kind: 'edit',
        label: 'edit',
        handler: (row) => this.openEdit(row),
      },
      {
        kind: 'delete',
        label: 'delete',
        handler: (row) => this.openDelete(row),
      },
    ];
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
        this.grid()?.setData(response);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading users grid:', error);
        this.loading.set(false);
      }
    });
  }

  // ------------------------------------------------------------------
  // Create / Edit
  // ------------------------------------------------------------------
  protected openCreate(): void {
    this.formMode.set('create');
    this.editingUserId.set(null);
    this.formModalOpen.set(true);
  }

  protected openEdit(row: UserGridDto): void {
    this.formMode.set('edit');
    this.editingUserId.set(row.id);
    this.formModalOpen.set(true);
  }

  protected onFormSaved(): void {
    this.formModalOpen.set(false);
    this.grid()?.reload();
  }

  protected onFormClosed(): void {
    this.formModalOpen.set(false);
  }

  // ------------------------------------------------------------------
  // Delete
  // ------------------------------------------------------------------
  protected openDelete(row: UserGridDto): void {
    this.userToDelete.set(row);
    this.deleteMessageParams.set({
      name: `${row.firstName} ${row.lastName}`.trim() || row.email,
    });
    this.deleteDialogOpen.set(true);
  }

  protected confirmDelete(): void {
    const user = this.userToDelete();
    if (!user || this.deleting()) return;

    this.deleting.set(true);

    this.userApiService.delete(user.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteDialogOpen.set(false);
        this.userToDelete.set(null);
        this.toast.show('users.deleted', ToastType.Success);
        this.grid()?.reload();
      },
      error: () => {
        this.deleting.set(false);
      },
    });
  }

  protected cancelDelete(): void {
    if (this.deleting()) return;
    this.deleteDialogOpen.set(false);
    this.userToDelete.set(null);
  }
}

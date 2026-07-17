import { ChangeDetectionStrategy, Component, inject, OnInit, signal, viewChild } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { DataGridComponent } from '../../../shared/components/data-grid/data-grid.component';
import { GridAction, GridColumnDef, GridRequestDto } from '../../../core/models/grid.model';
import { SelectorOption } from '../../../core/models/selector.model';
import { Guid } from '../../../core/models/guid.model';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ToastType } from '../../../shared/components/toast/toast.type';
import { StatusApiService } from '../../../core/api/status-api.service';
import { CustomersGridService } from '../services/customers-grid.service';
import { CustomerApiService } from '../services/customer-api.service';
import { CustomerGridDto } from '../models/customer.model';
import { CUSTOMERS_GRID_COLUMNS } from '../config/customers-grid-columns.config';
import {
  CustomerFormModalComponent,
  CustomerFormMode,
} from '../components/customer-form-modal/customer-form-modal.component';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [
    TranslateModule,
    DataGridComponent,
    IconComponent,
    ConfirmDialogComponent,
    CustomerFormModalComponent,
  ],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomersComponent implements OnInit {
  private readonly customersGridService = inject(CustomersGridService);
  private readonly customerApiService = inject(CustomerApiService);
  private readonly statusApiService = inject(StatusApiService);
  private readonly toast = inject(ToastService);

  grid = viewChild(DataGridComponent<CustomerGridDto>);

  protected readonly loading = signal(false);
  protected columns: GridColumnDef<CustomerGridDto>[] = [];
  protected actions: GridAction<CustomerGridDto>[] = [];

  // Form modal state
  protected readonly formModalOpen = signal(false);
  protected readonly formMode = signal<CustomerFormMode>('create');
  protected readonly editingCustomerId = signal<Guid | null>(null);

  // Delete confirmation state
  protected readonly deleteDialogOpen = signal(false);
  protected readonly deleting = signal(false);
  protected readonly customerToDelete = signal<CustomerGridDto | null>(null);
  protected readonly deleteMessageParams = signal<Record<string, unknown> | undefined>(undefined);

  ngOnInit(): void {
    this.initializeColumns();
    this.initializeActions();
  }

  private initializeColumns(): void {
    this.columns = CUSTOMERS_GRID_COLUMNS.map((column) => {
      // Status selector: filter by statusId with a select control
      if (column.field === 'statusName') {
        return {
          ...column,
          filterField: 'statusId',
          filterType: 'select',
          filterOptions: this.loadStatusOptions.bind(this),
        };
      }
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

  private async loadStatusOptions(searchQuery?: string): Promise<SelectorOption[]> {
    return await firstValueFrom(this.statusApiService.getStatusForSelector('Base', searchQuery));
  }

  protected onGridStateChange(request: GridRequestDto): void {
    this.loading.set(true);

    this.customersGridService.getGrid(request).subscribe({
      next: (response) => {
        this.grid()?.setData(response);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading customers grid:', error);
        this.loading.set(false);
      },
    });
  }

  // ------------------------------------------------------------------
  // Create / Edit
  // ------------------------------------------------------------------
  protected openCreate(): void {
    this.formMode.set('create');
    this.editingCustomerId.set(null);
    this.formModalOpen.set(true);
  }

  protected openEdit(row: CustomerGridDto): void {
    this.formMode.set('edit');
    this.editingCustomerId.set(row.id);
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
  protected openDelete(row: CustomerGridDto): void {
    this.customerToDelete.set(row);
    this.deleteMessageParams.set({
      name: row.tradeName?.trim() || row.legalName,
    });
    this.deleteDialogOpen.set(true);
  }

  protected confirmDelete(): void {
    const customer = this.customerToDelete();
    if (!customer || this.deleting()) return;

    this.deleting.set(true);

    this.customerApiService.delete(customer.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteDialogOpen.set(false);
        this.customerToDelete.set(null);
        this.toast.show('customers.deleted', ToastType.Success);
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
    this.customerToDelete.set(null);
  }
}

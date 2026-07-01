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
import { BudgetsGridService } from '../services/budgets-grid.service';
import { BudgetApiService } from '../services/budget-api.service';
import { BudgetGridDto } from '../models/budget.model';
import { BUDGETS_GRID_COLUMNS } from '../config/budgets-grid-columns.config';
import {
  BudgetFormModalComponent,
  BudgetFormMode,
} from '../components/budget-form-modal/budget-form-modal.component';

@Component({
  selector: 'app-budgets',
  standalone: true,
  imports: [
    TranslateModule,
    DataGridComponent,
    IconComponent,
    ConfirmDialogComponent,
    BudgetFormModalComponent,
  ],
  templateUrl: './budgets.component.html',
  styleUrl: './budgets.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BudgetsComponent implements OnInit {
  private readonly budgetsGridService = inject(BudgetsGridService);
  private readonly budgetApiService = inject(BudgetApiService);
  private readonly statusApiService = inject(StatusApiService);
  private readonly toast = inject(ToastService);

  grid = viewChild(DataGridComponent<BudgetGridDto>);

  protected readonly loading = signal(false);
  protected columns: GridColumnDef<BudgetGridDto>[] = [];
  protected actions: GridAction<BudgetGridDto>[] = [];

  // Form modal state
  protected readonly formModalOpen = signal(false);
  protected readonly formMode = signal<BudgetFormMode>('create');
  protected readonly editingBudgetId = signal<Guid | null>(null);

  // Delete confirmation state
  protected readonly deleteDialogOpen = signal(false);
  protected readonly deleting = signal(false);
  protected readonly budgetToDelete = signal<BudgetGridDto | null>(null);
  protected readonly deleteMessageParams = signal<Record<string, unknown> | undefined>(undefined);

  ngOnInit(): void {
    this.initializeColumns();
    this.initializeActions();
  }

  private initializeColumns(): void {
    this.columns = BUDGETS_GRID_COLUMNS.map((column) => {
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
    return await firstValueFrom(this.statusApiService.getStatusForSelector('Budget', searchQuery));
  }

  protected onGridStateChange(request: GridRequestDto): void {
    this.loading.set(true);

    this.budgetsGridService.getGrid(request).subscribe({
      next: (response) => {
        this.grid()?.setData(response);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading budgets grid:', error);
        this.loading.set(false);
      },
    });
  }

  // ------------------------------------------------------------------
  // Create / Edit
  // ------------------------------------------------------------------
  protected openCreate(): void {
    this.formMode.set('create');
    this.editingBudgetId.set(null);
    this.formModalOpen.set(true);
  }

  protected openEdit(row: BudgetGridDto): void {
    this.formMode.set('edit');
    this.editingBudgetId.set(row.id);
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
  protected openDelete(row: BudgetGridDto): void {
    this.budgetToDelete.set(row);
    this.deleteMessageParams.set({
      name: row.number,
    });
    this.deleteDialogOpen.set(true);
  }

  protected confirmDelete(): void {
    const budget = this.budgetToDelete();
    if (!budget || this.deleting()) return;

    this.deleting.set(true);

    this.budgetApiService.delete(budget.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteDialogOpen.set(false);
        this.budgetToDelete.set(null);
        this.toast.show('budgets.deleted', ToastType.Success);
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
    this.budgetToDelete.set(null);
  }
}

import { ChangeDetectionStrategy, Component, inject, OnInit, signal, viewChild } from '@angular/core';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DataGridComponent } from '../../../shared/components/data-grid/data-grid.component';
import { GridAction, GridColumnDef, GridRequestDto } from '../../../core/models/grid.model';
import { SelectorOption } from '../../../core/models/selector.model';
import { Guid } from '../../../core/models/guid.model';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ToastType } from '../../../shared/components/toast/toast.type';
import { ItemsGridService } from '../services/items-grid.service';
import { ItemApiService } from '../services/item-api.service';
import { ItemGridDto, ItemType } from '../models/item.model';
import { buildItemsGridColumns } from '../config/items-grid-columns.config';
import {
  ItemFormModalComponent,
  ItemFormMode,
} from '../components/item-form-modal/item-form-modal.component';

@Component({
  selector: 'app-items',
  standalone: true,
  imports: [
    TranslateModule,
    DataGridComponent,
    IconComponent,
    ConfirmDialogComponent,
    ItemFormModalComponent,
  ],
  templateUrl: './items.component.html',
  styleUrl: './items.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemsComponent implements OnInit {
  private readonly itemsGridService = inject(ItemsGridService);
  private readonly itemApiService = inject(ItemApiService);
  private readonly translate = inject(TranslateService);
  private readonly toast = inject(ToastService);

  grid = viewChild(DataGridComponent<ItemGridDto>);

  protected readonly loading = signal(false);
  protected columns: GridColumnDef<ItemGridDto>[] = [];
  protected actions: GridAction<ItemGridDto>[] = [];

  // Form modal state
  protected readonly formModalOpen = signal(false);
  protected readonly formMode = signal<ItemFormMode>('create');
  protected readonly editingItemId = signal<Guid | null>(null);

  // Delete confirmation state
  protected readonly deleteDialogOpen = signal(false);
  protected readonly deleting = signal(false);
  protected readonly itemToDelete = signal<ItemGridDto | null>(null);
  protected readonly deleteMessageParams = signal<Record<string, unknown> | undefined>(undefined);

  ngOnInit(): void {
    this.initializeColumns();
    this.initializeActions();
  }

  private initializeColumns(): void {
    const baseColumns = buildItemsGridColumns((type) =>
      this.translate.instant(
        type === ItemType.Service ? 'items.types.service' : 'items.types.product'
      )
    );

    this.columns = baseColumns.map((column) => {
      // Type selector: filter by 'type' with a static list of options.
      if (column.field === 'type') {
        return {
          ...column,
          filterType: 'select',
          filterOptions: this.loadTypeOptions.bind(this),
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

  private async loadTypeOptions(): Promise<SelectorOption[]> {
    // ItemType values are numeric on the backend, but the grid filter treats the id as a string.
    return [
      {
        id: String(ItemType.Product) as unknown as Guid,
        name: this.translate.instant('items.types.product'),
      },
      {
        id: String(ItemType.Service) as unknown as Guid,
        name: this.translate.instant('items.types.service'),
      },
    ];
  }

  protected onGridStateChange(request: GridRequestDto): void {
    this.loading.set(true);

    this.itemsGridService.getGrid(request).subscribe({
      next: (response) => {
        this.grid()?.setData(response);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading items grid:', error);
        this.loading.set(false);
      },
    });
  }

  // ------------------------------------------------------------------
  // Create / Edit
  // ------------------------------------------------------------------
  protected openCreate(): void {
    this.formMode.set('create');
    this.editingItemId.set(null);
    this.formModalOpen.set(true);
  }

  protected openEdit(row: ItemGridDto): void {
    this.formMode.set('edit');
    this.editingItemId.set(row.id);
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
  protected openDelete(row: ItemGridDto): void {
    this.itemToDelete.set(row);
    this.deleteMessageParams.set({
      name: row.name,
    });
    this.deleteDialogOpen.set(true);
  }

  protected confirmDelete(): void {
    const item = this.itemToDelete();
    if (!item || this.deleting()) return;

    this.deleting.set(true);

    this.itemApiService.delete(item.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteDialogOpen.set(false);
        this.itemToDelete.set(null);
        this.toast.show('items.deleted', ToastType.Success);
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
    this.itemToDelete.set(null);
  }
}

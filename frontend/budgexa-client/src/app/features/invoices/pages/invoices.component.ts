import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  signal,
  viewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
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
import { InvoicesGridService } from '../services/invoices-grid.service';
import { InvoiceApiService } from '../services/invoice-api.service';
import { InvoiceGridDto } from '../models/invoice.model';
import { INVOICES_GRID_COLUMNS } from '../config/invoices-grid-columns.config';
import {
  InvoiceFormModalComponent,
  InvoiceFormMode,
} from '../components/invoice-form-modal/invoice-form-modal.component';
import { InvoicePaymentModalComponent } from '../components/invoice-payment-modal/invoice-payment-modal.component';
import { InvoiceAiGeneratorComponent } from '../components/invoice-ai-generator/invoice-ai-generator.component';
import { PrivateInvoiceAiResponseDto } from '../../../shared/models/ai.model';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [
    TranslateModule,
    DataGridComponent,
    IconComponent,
    ConfirmDialogComponent,
    InvoiceFormModalComponent,
    InvoicePaymentModalComponent,
    InvoiceAiGeneratorComponent,
  ],
  templateUrl: './invoices.component.html',
  styleUrl: './invoices.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvoicesComponent implements OnInit {
  private readonly invoicesGridService = inject(InvoicesGridService);
  private readonly invoiceApiService = inject(InvoiceApiService);
  private readonly statusApiService = inject(StatusApiService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  grid = viewChild(DataGridComponent<InvoiceGridDto>);

  protected readonly loading = signal(false);
  protected columns: GridColumnDef<InvoiceGridDto>[] = [];
  protected actions: GridAction<InvoiceGridDto>[] = [];

  // Form modal state
  protected readonly formModalOpen = signal(false);
  protected readonly formMode = signal<InvoiceFormMode>('create');
  protected readonly editingInvoiceId = signal<Guid | null>(null);
  protected readonly preselectedBudgetId = signal<Guid | null>(null);

  // Payment modal state
  protected readonly paymentModalOpen = signal(false);
  protected readonly paymentInvoice = signal<InvoiceGridDto | null>(null);

  // Delete confirmation state
  protected readonly deleteDialogOpen = signal(false);
  protected readonly deleting = signal(false);
  protected readonly invoiceToDelete = signal<InvoiceGridDto | null>(null);
  protected readonly deleteMessageParams = signal<Record<string, unknown> | undefined>(undefined);

  // AI generator modal state
  protected readonly aiModalOpen = signal(false);
  protected readonly aiGeneratedData = signal<PrivateInvoiceAiResponseDto | null>(null);

  ngOnInit(): void {
    this.initializeColumns();
    this.initializeActions();

    // Check if navigated from budgets with a preselected budget
    const budgetId = this.route.snapshot.queryParamMap.get('budgetId');
    if (budgetId) {
      this.preselectedBudgetId.set(budgetId as Guid);
      this.formMode.set('create');
      this.editingInvoiceId.set(null);
      this.formModalOpen.set(true);
      // Clean up the query param
      this.router.navigate([], { queryParams: {}, replaceUrl: true });
    }
  }

  private initializeColumns(): void {
    this.columns = INVOICES_GRID_COLUMNS.map((column) => {
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
        kind: 'custom',
        label: 'invoices.payment.register',
        icon: 'plus',
        handler: (row) => this.openPayment(row),
        visible: (row) => Number(row.amountDue ?? 0) > 0,
      },
      {
        kind: 'custom',
        label: 'invoices.downloadPdf',
        handler: (row) => this.downloadPdf(row),
      },
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
    return await firstValueFrom(this.statusApiService.getStatusForSelector('Invoice', searchQuery));
  }

  protected onGridStateChange(request: GridRequestDto): void {
    this.loading.set(true);

    this.invoicesGridService.getGrid(request).subscribe({
      next: (response) => {
        this.grid()?.setData(response);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading invoices grid:', error);
        this.loading.set(false);
      },
    });
  }

  // ------------------------------------------------------------------
  // Create / Edit
  // ------------------------------------------------------------------
  protected openCreate(): void {
    this.formMode.set('create');
    this.editingInvoiceId.set(null);
    this.formModalOpen.set(true);
  }

  protected openEdit(row: InvoiceGridDto): void {
    this.formMode.set('edit');
    this.editingInvoiceId.set(row.id);
    this.formModalOpen.set(true);
  }

  protected onFormSaved(): void {
    this.formModalOpen.set(false);
    this.preselectedBudgetId.set(null);
    this.grid()?.reload();
  }

  protected onFormClosed(): void {
    this.formModalOpen.set(false);
    this.preselectedBudgetId.set(null);
    this.aiGeneratedData.set(null); // Clear AI data after closing
  }

  // ------------------------------------------------------------------
  // AI Generator
  // ------------------------------------------------------------------
  protected openAiGenerator(): void {
    this.aiModalOpen.set(true);
  }

  protected onAiGenerated(data: PrivateInvoiceAiResponseDto): void {
    this.aiGeneratedData.set(data);
    this.aiModalOpen.set(false);
    // Open the form modal in create mode with the AI data
    this.formMode.set('create');
    this.editingInvoiceId.set(null);
    this.preselectedBudgetId.set(null);
    this.formModalOpen.set(true);
  }

  protected onAiModalClosed(): void {
    this.aiModalOpen.set(false);
  }

  // ------------------------------------------------------------------
  // Payment
  // ------------------------------------------------------------------
  protected openPayment(row: InvoiceGridDto): void {
    this.paymentInvoice.set(row);
    this.paymentModalOpen.set(true);
  }

  protected onPaymentRegistered(): void {
    this.paymentModalOpen.set(false);
    this.paymentInvoice.set(null);
    this.grid()?.reload();
  }

  protected onPaymentClosed(): void {
    this.paymentModalOpen.set(false);
    this.paymentInvoice.set(null);
  }

  // ------------------------------------------------------------------
  // Delete
  // ------------------------------------------------------------------
  protected openDelete(row: InvoiceGridDto): void {
    this.invoiceToDelete.set(row);
    this.deleteMessageParams.set({
      number: `${row.series}-${row.number}`,
    });
    this.deleteDialogOpen.set(true);
  }

  protected confirmDelete(): void {
    const invoice = this.invoiceToDelete();
    if (!invoice || this.deleting()) return;

    this.deleting.set(true);

    this.invoiceApiService.delete(invoice.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteDialogOpen.set(false);
        this.invoiceToDelete.set(null);
        this.toast.show('invoices.deleted', ToastType.Success);
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
    this.invoiceToDelete.set(null);
  }

  // ------------------------------------------------------------------
  // Download PDF
  // ------------------------------------------------------------------
  private downloadPdf(row: InvoiceGridDto): void {
    this.invoiceApiService.downloadPdf(row.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `${row.series}-${row.number}.pdf`;
        anchor.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.toast.show('downloadFailed', ToastType.Error);
      },
    });
  }
}

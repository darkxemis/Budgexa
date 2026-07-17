import {
  ChangeDetectionStrategy,
  Component,
  computed,
  HostListener,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import {
  FormArray,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { FormErrorComponent } from '../../../../shared/components/form-error/form-error.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { AutocompleteSelectorComponent } from '../../../../shared/components/autocomplete-selector/autocomplete-selector.component';
import { StatusChangeMenuComponent } from '../../../../shared/components/status-change-menu/status-change-menu.component';
import { Guid } from '../../../../core/models/guid.model';
import { SelectorOption } from '../../../../core/models/selector.model';
import { ToastService } from '../../../../shared/components/toast/toast.service';
import { ToastType } from '../../../../shared/components/toast/toast.type';
import { InvoiceApiService } from '../../services/invoice-api.service';
import { CustomerSelectorService } from '../../services/customer-selector.service';
import { BudgetSelectorService } from '../../services/budget-selector.service';
import {
  InvoiceCreateDto,
  InvoiceDto,
  InvoiceLineUpsertDto,
  InvoiceUpdateDto,
  computeLineTotals,
} from '../../models/invoice.model';
import {
  InvoiceLineFormGroup,
  InvoiceLinesEditorComponent,
  InvoiceTotals,
} from '../invoice-lines-editor/invoice-lines-editor.component';

export type InvoiceFormMode = 'create' | 'edit';

@Component({
  selector: 'app-invoice-form-modal',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslateModule,
    SpinnerComponent,
    FormErrorComponent,
    AutocompleteSelectorComponent,
    InvoiceLinesEditorComponent,
    StatusChangeMenuComponent,
  ],
  templateUrl: './invoice-form-modal.component.html',
  styleUrl: './invoice-form-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvoiceFormModalComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly invoiceApi = inject(InvoiceApiService);
  private readonly customerSelector = inject(CustomerSelectorService);
  private readonly budgetSelector = inject(BudgetSelectorService);
  private readonly toast = inject(ToastService);

  readonly mode = input<InvoiceFormMode>('create');
  /** Existing invoice id (required when mode is 'edit'). */
  readonly invoiceId = input<Guid | null>(null);

  readonly saved = output<InvoiceDto>();
  readonly close = output<void>();

  protected readonly loading = signal(false);
  protected readonly loadingData = signal(false);
  protected readonly changingStatus = signal(false);
  protected readonly totals = signal<InvoiceTotals>({
    grossSubtotal: 0,
    discountAmount: 0,
    subtotal: 0,
    taxAmount: 0,
    withholdingAmount: 0,
    total: 0,
  });
  protected readonly initialCustomerId = signal<Guid | null>(null);
  protected readonly initialBudgetId = signal<Guid | null>(null);
  protected readonly currentStatusId = signal<Guid | null>(null);
  protected readonly currentStatusName = signal<string | null>(null);

  protected readonly isEdit = computed(() => this.mode() === 'edit');
  protected readonly titleKey = computed(() =>
    this.isEdit() ? 'invoices.form.editTitle' : 'invoices.form.createTitle'
  );
  protected readonly submitKey = computed(() => {
    if (this.loading()) {
      return this.isEdit() ? 'saving' : 'invoices.form.creating';
    }
    return this.isEdit() ? 'save' : 'invoices.form.create';
  });

  readonly form = this.fb.group({
    series: ['', [Validators.required, Validators.maxLength(10)]],
    number: ['', [Validators.required, Validators.maxLength(50)]],
    issueDate: [this.today(), [Validators.required]],
    dueDate: [this.today(), [Validators.required]],
    customerId: this.fb.control<Guid | null>(null, [Validators.required]),
    budgetId: this.fb.control<Guid | null>(null),
    currency: ['EUR', [Validators.required, Validators.maxLength(3)]],
    notes: ['', [Validators.maxLength(2000)]],
    lines: this.fb.array<InvoiceLineFormGroup>([]),
  });

  /** Exposed for the editor input. */
  protected get linesArray(): FormArray<InvoiceLineFormGroup> {
    return this.form.controls.lines;
  }

  ngOnInit(): void {
    if (this.isEdit() && this.invoiceId()) {
      this.loadInvoice();
    } else {
      // Start create mode with at least one blank line for convenience.
      this.linesArray.push(InvoiceLinesEditorComponent.createLine(this.fb));
      this.recomputeTotals();
    }
  }

  @HostListener('document:keydown.escape')
  protected onEscape() {
    if (!this.loading()) this.onClose();
  }

  protected loadCustomerOptions = async (searchQuery?: string): Promise<SelectorOption[]> => {
    try {
      return await firstValueFrom(this.customerSelector.getForSelector(searchQuery));
    } catch {
      return [];
    }
  };

  protected loadBudgetOptions = async (searchQuery?: string): Promise<SelectorOption[]> => {
    try {
      return await firstValueFrom(this.budgetSelector.getForSelector(searchQuery));
    } catch {
      return [];
    }
  };

  protected onCustomerSelected(value: Guid | null): void {
    this.form.controls.customerId.setValue(value);
    this.form.controls.customerId.markAsTouched();
  }

  protected onBudgetSelected(value: Guid | null): void {
    this.form.controls.budgetId.setValue(value);
    this.form.controls.budgetId.markAsTouched();
  }

  protected onTotalsChange(totals: InvoiceTotals): void {
    this.totals.set(totals);
  }

  private loadInvoice(): void {
    const id = this.invoiceId();
    if (!id) return;

    this.loadingData.set(true);
    this.invoiceApi.getById(id).subscribe({
      next: (invoice) => {
        this.form.patchValue({
          series: invoice.series,
          number: invoice.number,
          issueDate: invoice.issueDate,
          dueDate: invoice.dueDate,
          customerId: invoice.customerId,
          budgetId: invoice.budgetId ?? null,
          currency: invoice.currency,
          notes: invoice.notes ?? '',
        });

        this.initialCustomerId.set(invoice.customerId);
        this.initialBudgetId.set(invoice.budgetId ?? null);
        this.currentStatusId.set(invoice.statusId);
        this.currentStatusName.set(invoice.statusName);

        // Budget cannot be changed once the invoice exists.
        this.form.controls.budgetId.disable();

        // Reset lines array from the loaded invoice.
        this.linesArray.clear();
        const sortedLines = [...invoice.lines].sort((a, b) => a.sortOrder - b.sortOrder);
        for (const line of sortedLines) {
          const formLine = InvoiceLinesEditorComponent.createLine(this.fb, {
            id: line.id,
            itemId: line.itemId ?? null,
            description: line.description,
            unit: line.unit,
            quantity: Number(line.quantity),
            unitPrice: Number(line.unitPrice),
            discountPercentage: Number(line.discountPercentage),
            taxRate: Number(line.taxRate),
            withholdingRate: Number(line.withholdingRate),
          });

          // If line references a catalog item, lock the catalog-sourced fields
          if (line.itemId) {
            formLine.controls.description.disable();
            formLine.controls.unit.disable();
            formLine.controls.unitPrice.disable();
            formLine.controls.taxRate.disable();
          }

          this.linesArray.push(formLine);
        }
        this.recomputeTotals();
        this.loadingData.set(false);
      },
      error: () => {
        this.loadingData.set(false);
        this.onClose();
      },
    });
  }

  protected onSubmit(): void {
    if (this.loading()) return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.linesArray.length === 0) {
      this.toast.show('invoices.form.noLines', ToastType.Error);
      return;
    }

    const value = this.form.getRawValue();

    const lines: InvoiceLineUpsertDto[] = this.linesArray.controls.map((control, index) => {
      const line = control.getRawValue();
      return {
        id: this.isEdit() ? line.id ?? null : null,
        itemId: line.itemId ?? null,
        sortOrder: index,
        description: (line.description ?? '').trim(),
        unit: (line.unit ?? '').trim(),
        // Allow decimal quantities: 2.5 hours, 1.75 kg, etc.
        quantity: Math.max(0.01, Number(line.quantity) || 0.01),
        unitPrice: Number(line.unitPrice) || 0,
        discountPercentage: Number(line.discountPercentage) || 0,
        taxRate: Number(line.taxRate) || 0,
        withholdingRate: Number(line.withholdingRate) || 0,
      };
    });

    this.loading.set(true);

    if (this.isEdit()) {
      const updatePayload: InvoiceUpdateDto = {
        customerId: value.customerId!,
        series: value.series.trim(),
        number: value.number.trim(),
        issueDate: value.issueDate,
        dueDate: value.dueDate,
        currency: (value.currency ?? 'EUR').trim().toUpperCase(),
        notes: this.toNullable(value.notes),
        lines,
      };

      this.invoiceApi.update(this.invoiceId()!, updatePayload).subscribe({
        next: (invoice) => this.handleSaveSuccess(invoice, true),
        error: () => this.loading.set(false),
      });
    } else {
      const createPayload: InvoiceCreateDto = {
        customerId: value.customerId!,
        budgetId: value.budgetId ?? null,
        series: value.series.trim(),
        number: value.number.trim(),
        issueDate: value.issueDate,
        dueDate: value.dueDate,
        currency: (value.currency ?? 'EUR').trim().toUpperCase(),
        notes: this.toNullable(value.notes),
        lines,
      };

      this.invoiceApi.create(createPayload).subscribe({
        next: (invoice) => this.handleSaveSuccess(invoice, false),
        error: () => this.loading.set(false),
      });
    }
  }

  private handleSaveSuccess(invoice: InvoiceDto, isEdit: boolean): void {
    this.loading.set(false);
    this.toast.show(
      isEdit ? 'invoices.form.updated' : 'invoices.form.created',
      ToastType.Success
    );
    this.saved.emit(invoice);
  }

  protected onClose(): void {
    this.close.emit();
  }

  protected onStatusSelected(statusId: Guid): void {
    const id = this.invoiceId();
    if (!id || this.changingStatus()) return;

    this.changingStatus.set(true);
    this.invoiceApi.changeStatus(id, statusId).subscribe({
      next: (invoice) => {
        this.changingStatus.set(false);
        this.currentStatusId.set(invoice.statusId);
        this.currentStatusName.set(invoice.statusName);
        this.toast.show('invoices.status.changed', ToastType.Success);
        this.saved.emit(invoice);
      },
      error: () => {
        this.changingStatus.set(false);
      },
    });
  }

  private toNullable(value: string | null | undefined): string | null {
    const trimmed = (value ?? '').trim();
    return trimmed.length === 0 ? null : trimmed;
  }

  private today(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  private recomputeTotals(): void {
    let grossSubtotal = 0;
    let discountAmount = 0;
    let subtotal = 0;
    let taxAmount = 0;
    let withholdingAmount = 0;
    for (const ctrl of this.linesArray.controls) {
      const raw = ctrl.getRawValue();
      const gross = raw.quantity * raw.unitPrice;
      const discount = gross * (raw.discountPercentage / 100);
      const t = computeLineTotals(raw);
      grossSubtotal += gross;
      discountAmount += discount;
      subtotal += t.subtotal;
      taxAmount += t.taxAmount;
      withholdingAmount += t.withholdingAmount;
    }
    this.totals.set({
      grossSubtotal,
      discountAmount,
      subtotal,
      taxAmount,
      withholdingAmount,
      total: subtotal + taxAmount - withholdingAmount,
    });
  }
}

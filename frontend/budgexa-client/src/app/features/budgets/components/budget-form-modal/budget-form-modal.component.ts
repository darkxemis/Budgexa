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
import { BudgetApiService } from '../../services/budget-api.service';
import { CustomerSelectorService } from '../../services/customer-selector.service';
import {
  BudgetCreateDto,
  BudgetDto,
  BudgetLineUpsertDto,
  BudgetUpdateDto,
  computeLineTotals,
} from '../../models/budget.model';
import {
  BudgetLineFormGroup,
  BudgetLinesEditorComponent,
  BudgetTotals,
} from '../budget-lines-editor/budget-lines-editor.component';

export type BudgetFormMode = 'create' | 'edit';

@Component({
  selector: 'app-budget-form-modal',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslateModule,
    SpinnerComponent,
    FormErrorComponent,
    AutocompleteSelectorComponent,
    BudgetLinesEditorComponent,
    StatusChangeMenuComponent,
  ],
  templateUrl: './budget-form-modal.component.html',
  styleUrl: './budget-form-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BudgetFormModalComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly budgetApi = inject(BudgetApiService);
  private readonly customerSelector = inject(CustomerSelectorService);
  private readonly toast = inject(ToastService);

  readonly mode = input<BudgetFormMode>('create');
  /** Existing budget id (required when mode is 'edit'). */
  readonly budgetId = input<Guid | null>(null);

  readonly saved = output<BudgetDto>();
  readonly close = output<void>();

  protected readonly loading = signal(false);
  protected readonly loadingData = signal(false);
  protected readonly changingStatus = signal(false);
  protected readonly totals = signal<BudgetTotals>({ grossSubtotal: 0, discountAmount: 0, subtotal: 0, taxAmount: 0, total: 0 });
  protected readonly initialCustomerId = signal<Guid | null>(null);
  protected readonly currentStatusId = signal<Guid | null>(null);
  protected readonly currentStatusName = signal<string | null>(null);

  protected readonly isEdit = computed(() => this.mode() === 'edit');
  protected readonly titleKey = computed(() =>
    this.isEdit() ? 'budgets.form.editTitle' : 'budgets.form.createTitle'
  );
  protected readonly submitKey = computed(() => {
    if (this.loading()) {
      return this.isEdit() ? 'saving' : 'budgets.form.creating';
    }
    return this.isEdit() ? 'save' : 'budgets.form.create';
  });

  readonly form = this.fb.group({
    number: ['', [Validators.required, Validators.maxLength(50)]],
    issueDate: [this.today(), [Validators.required]],
    validUntil: this.fb.control<string | null>(null),
    customerId: this.fb.control<Guid | null>(null, [Validators.required]),
    currency: ['EUR', [Validators.required, Validators.maxLength(3)]],
    notes: ['', [Validators.maxLength(2000)]],
    termsAndConditions: ['', [Validators.maxLength(2000)]],
    lines: this.fb.array<BudgetLineFormGroup>([]),
  });

  /** Exposed for the editor input. */
  protected get linesArray(): FormArray<BudgetLineFormGroup> {
    return this.form.controls.lines;
  }

  ngOnInit(): void {
    if (this.isEdit() && this.budgetId()) {
      this.loadBudget();
    } else {
      // Start create mode with at least one blank line for convenience.
      this.linesArray.push(BudgetLinesEditorComponent.createLine(this.fb));
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

  protected onCustomerSelected(value: Guid | null): void {
    this.form.controls.customerId.setValue(value);
    this.form.controls.customerId.markAsTouched();
  }

  protected onTotalsChange(totals: BudgetTotals): void {
    this.totals.set(totals);
  }

  private loadBudget(): void {
    const id = this.budgetId();
    if (!id) return;

    this.loadingData.set(true);
    this.budgetApi.getById(id).subscribe({
      next: (budget) => {
        this.form.patchValue({
          number: budget.number,
          issueDate: budget.issueDate,
          validUntil: budget.validUntil ?? null,
          customerId: budget.customerId,
          currency: budget.currency,
          notes: budget.notes ?? '',
          termsAndConditions: budget.termsAndConditions ?? '',
        });

        this.initialCustomerId.set(budget.customerId);
        this.currentStatusId.set(budget.statusId);
        this.currentStatusName.set(budget.statusName);

        // Reset lines array from the loaded budget.
        this.linesArray.clear();
        const sortedLines = [...budget.lines].sort((a, b) => a.sortOrder - b.sortOrder);
        for (const line of sortedLines) {
          const formLine = BudgetLinesEditorComponent.createLine(this.fb, {
            id: line.id,
            itemId: line.itemId ?? null,
            description: line.description,
            unit: line.unit,
            quantity: Number(line.quantity),
            unitPrice: Number(line.unitPrice),
            discountPercentage: Number(line.discountPercentage),
            taxRate: Number(line.taxRate),
          });
          
          // If line has an itemId (references catalog), disable protected fields
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
      this.toast.show('budgets.form.noLines', ToastType.Error);
      return;
    }

    const value = this.form.getRawValue();

    const lines: BudgetLineUpsertDto[] = this.linesArray.controls.map((control, index) => {
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
      };
    });

    const basePayload = {
      customerId: value.customerId!,
      number: value.number.trim(),
      issueDate: value.issueDate,
      validUntil: value.validUntil && value.validUntil.length > 0 ? value.validUntil : null,
      currency: (value.currency ?? 'EUR').trim().toUpperCase(),
      notes: this.toNullable(value.notes),
      termsAndConditions: this.toNullable(value.termsAndConditions),
      lines,
    };

    this.loading.set(true);

    const request$ = this.isEdit()
      ? this.budgetApi.update(this.budgetId()!, basePayload as BudgetUpdateDto)
      : this.budgetApi.create(basePayload as BudgetCreateDto);

    request$.subscribe({
      next: (budget) => {
        this.loading.set(false);
        this.toast.show(
          this.isEdit() ? 'budgets.form.updated' : 'budgets.form.created',
          ToastType.Success
        );
        this.saved.emit(budget);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  protected onClose(): void {
    this.close.emit();
  }

  protected onStatusSelected(statusId: Guid): void {
    const id = this.budgetId();
    if (!id || this.changingStatus()) return;

    this.changingStatus.set(true);
    this.budgetApi.changeStatus(id, statusId).subscribe({
      next: (budget) => {
        this.changingStatus.set(false);
        this.currentStatusId.set(budget.statusId);
        this.currentStatusName.set(budget.statusName);
        this.toast.show('budgets.status.changed', ToastType.Success);
        this.saved.emit(budget);
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
    for (const ctrl of this.linesArray.controls) {
      const raw = ctrl.getRawValue();
      const gross = raw.quantity * raw.unitPrice;
      const discount = gross * (raw.discountPercentage / 100);
      const t = computeLineTotals(raw);
      grossSubtotal += gross;
      discountAmount += discount;
      subtotal += t.subtotal;
      taxAmount += t.taxAmount;
    }
    this.totals.set({ grossSubtotal, discountAmount, subtotal, taxAmount, total: subtotal + taxAmount });
  }
}

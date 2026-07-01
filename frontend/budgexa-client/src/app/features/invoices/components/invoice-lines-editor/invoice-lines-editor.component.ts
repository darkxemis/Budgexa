import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import {
  FormArray,
  FormControl,
  FormGroup,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { firstValueFrom, switchMap } from 'rxjs';
import { FormErrorComponent } from '../../../../shared/components/form-error/form-error.component';
import { AutocompleteSelectorComponent } from '../../../../shared/components/autocomplete-selector/autocomplete-selector.component';
import { Guid } from '../../../../core/models/guid.model';
import { SelectorOption } from '../../../../core/models/selector.model';
import { ItemSelectorService } from '../../../items/services/item-selector.service';
import { ItemApiService } from '../../../items/services/item-api.service';
import { computeLineTotals } from '../../models/invoice.model';

/**
 * Reactive form group for a single invoice line.
 * Kept public so the parent modal can consume the array shape safely.
 */
export type InvoiceLineFormGroup = FormGroup<{
  id: FormControl<Guid | null>;
  itemId: FormControl<Guid | null>;
  description: FormControl<string>;
  unit: FormControl<string>;
  quantity: FormControl<number>;
  unitPrice: FormControl<number>;
  discountPercentage: FormControl<number>;
  taxRate: FormControl<number>;
  withholdingRate: FormControl<number>;
}>;

export interface InvoiceTotals {
  subtotal: number;
  taxAmount: number;
  withholdingAmount: number;
  total: number;
}

@Component({
  selector: 'app-invoice-lines-editor',
  standalone: true,
  imports: [ReactiveFormsModule, TranslateModule, FormErrorComponent, AutocompleteSelectorComponent],
  templateUrl: './invoice-lines-editor.component.html',
  styleUrl: './invoice-lines-editor.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvoiceLinesEditorComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly itemSelector = inject(ItemSelectorService);
  private readonly itemApi = inject(ItemApiService);
  private readonly destroyRef = inject(DestroyRef);

  /** Bound `FormArray` from the parent form. */
  readonly lines = input.required<FormArray<InvoiceLineFormGroup>>();

  /** Currency code shown next to computed totals (e.g. "EUR"). */
  readonly currency = input<string>('EUR');

  /** Emitted every time totals recompute. */
  readonly totalsChange = output<InvoiceTotals>();

  /**
   * Incremented after any programmatic patchValue() so that the computed()
   * — which cannot track FormControl value changes directly — re-evaluates.
   */
  private readonly _lineVersion = signal(0);

  /** Reactive totals derived from all rows in the array. */
  protected readonly totals = computed<InvoiceTotals>(() => {
    this._lineVersion(); // tracked dependency → rerun on value changes
    const rows = this.lines().controls.map(control => control.getRawValue());
    let subtotal = 0;
    let taxAmount = 0;
    let withholdingAmount = 0;
    for (const row of rows) {
      const t = computeLineTotals({
        quantity: row.quantity,
        unitPrice: row.unitPrice,
        discountPercentage: row.discountPercentage,
        taxRate: row.taxRate,
        withholdingRate: row.withholdingRate,
      });
      subtotal          += Math.round(t.subtotal          * 100) / 100;
      taxAmount         += Math.round(t.taxAmount         * 100) / 100;
      withholdingAmount += Math.round(t.withholdingAmount * 100) / 100;
    }
    const total = Math.round((subtotal + taxAmount - withholdingAmount) * 100) / 100;
    return { subtotal, taxAmount, withholdingAmount, total };
  });

  constructor() {
    // Emit totals whenever they change.
    effect(() => this.totalsChange.emit(this.totals()));

    // Bridge FormControl.valueChanges → _lineVersion so the computed() re-evaluates
    // when the user types manually (FormControl is not a signal).
    toObservable(this.lines)
      .pipe(
        switchMap(arr => arr.valueChanges),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this._lineVersion.update(v => v + 1));
  }

  protected addLine(): void {
    this.lines().push(InvoiceLinesEditorComponent.createLine(this.fb));
  }

  protected removeLine(index: number): void {
    this.lines().removeAt(index);
  }

  protected lineSubtotal(index: number): number {
    this._lineVersion();
    const row = this.lines().at(index).getRawValue();
    return computeLineTotals(row).subtotal;
  }

  protected lineTotal(index: number): number {
    this._lineVersion();
    const row = this.lines().at(index).getRawValue();
    return computeLineTotals(row).total;
  }

  protected loadItemOptions = async (searchQuery?: string): Promise<SelectorOption[]> => {
    try {
      return await firstValueFrom(this.itemSelector.getForSelector(searchQuery));
    } catch {
      return [];
    }
  };

  protected onItemSelected(index: number, itemId: Guid | null): void {
    const line = this.lines().at(index);

    this.itemApi.getById(itemId!).subscribe({
      next: (item) => {
        line.patchValue({
          itemId: item.id,
          description: item.name,
          unit: item.unit,
          unitPrice: Number(item.unitPrice),
          taxRate: Number(item.taxRate),
        });
        // Lock catalog-sourced fields; quantity, discount and withholding remain editable
        line.controls.description.disable();
        line.controls.unit.disable();
        line.controls.unitPrice.disable();
        line.controls.taxRate.disable();
        this._lineVersion.update(v => v + 1);
      },
      error: () => {
        line.controls.itemId.setValue(itemId);
      },
    });
  }

  protected isItemSelected(index: number): boolean {
    return this.lines().at(index).controls.itemId.value !== null;
  }

  /**
   * Static factory so both the editor and the parent modal can build lines
   * with identical validators.
   */
  static createLine(
    fb: NonNullableFormBuilder,
    preset?: {
      id?: Guid | null;
      itemId?: Guid | null;
      description?: string;
      unit?: string;
      quantity?: number;
      unitPrice?: number;
      discountPercentage?: number;
      taxRate?: number;
      withholdingRate?: number;
    }
  ): InvoiceLineFormGroup {
    return fb.group({
      id: fb.control<Guid | null>(preset?.id ?? null),
      itemId: fb.control<Guid | null>(preset?.itemId ?? null),
      description: [preset?.description ?? '', [Validators.required, Validators.maxLength(500)]],
      unit: [preset?.unit ?? '', [Validators.required, Validators.maxLength(50)]],
      quantity: [preset?.quantity ?? 1, [Validators.required, Validators.min(1)]],
      unitPrice: [preset?.unitPrice ?? 0, [Validators.required, Validators.min(0)]],
      discountPercentage: [
        preset?.discountPercentage ?? 0,
        [Validators.required, Validators.min(0), Validators.max(100)],
      ],
      taxRate: [
        preset?.taxRate ?? 0,
        [Validators.required, Validators.min(0), Validators.max(100)],
      ],
      withholdingRate: [
        preset?.withholdingRate ?? 0,
        [Validators.required, Validators.min(0), Validators.max(100)],
      ],
    });
  }
}


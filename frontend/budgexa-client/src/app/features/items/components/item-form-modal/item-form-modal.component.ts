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
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { FormErrorComponent } from '../../../../shared/components/form-error/form-error.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { Guid } from '../../../../core/models/guid.model';
import { ToastService } from '../../../../shared/components/toast/toast.service';
import { ToastType } from '../../../../shared/components/toast/toast.type';
import { ItemApiService } from '../../services/item-api.service';
import { ItemDto, ItemType } from '../../models/item.model';

export type ItemFormMode = 'create' | 'edit';

@Component({
  selector: 'app-item-form-modal',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslateModule,
    SpinnerComponent,
    FormErrorComponent,
  ],
  templateUrl: './item-form-modal.component.html',
  styleUrl: './item-form-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemFormModalComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly itemApi = inject(ItemApiService);
  private readonly toast = inject(ToastService);

  readonly mode = input<ItemFormMode>('create');
  /** Existing item id (required when mode is 'edit'). */
  readonly itemId = input<Guid | null>(null);

  readonly saved = output<ItemDto>();
  readonly close = output<void>();

  protected readonly loading = signal(false);
  protected readonly loadingData = signal(false);

  protected readonly ItemType = ItemType;

  protected readonly isEdit = computed(() => this.mode() === 'edit');
  protected readonly titleKey = computed(() =>
    this.isEdit() ? 'items.form.editTitle' : 'items.form.createTitle'
  );
  protected readonly submitKey = computed(() => {
    if (this.loading()) {
      return this.isEdit() ? 'saving' : 'items.form.creating';
    }
    return this.isEdit() ? 'save' : 'items.form.create';
  });

  readonly form = this.fb.group({
    sku: ['', [Validators.maxLength(100)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    type: [ItemType.Product as ItemType, [Validators.required]],
    unit: ['', [Validators.required, Validators.maxLength(50)]],
    unitPrice: [0, [Validators.required, Validators.min(0)]],
    taxRate: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
    currency: ['EUR', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
  });

  ngOnInit(): void {
    if (this.isEdit() && this.itemId()) {
      this.loadItem();
    }
  }

  @HostListener('document:keydown.escape')
  protected onEscape() {
    if (!this.loading()) this.onClose();
  }

  private loadItem(): void {
    const id = this.itemId();
    if (!id) return;

    this.loadingData.set(true);
    this.itemApi.getById(id).subscribe({
      next: (item) => {
        this.form.patchValue({
          sku: item.sku ?? '',
          name: item.name,
          description: item.description ?? '',
          type: item.type,
          unit: item.unit,
          unitPrice: item.unitPrice,
          taxRate: item.taxRate,
          currency: item.currency,
        });
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

    const value = this.form.getRawValue();

    const payload = {
      sku: this.toNullable(value.sku),
      name: value.name.trim(),
      description: this.toNullable(value.description),
      type: Number(value.type) as ItemType,
      unit: value.unit.trim(),
      unitPrice: Number(value.unitPrice),
      taxRate: Number(value.taxRate),
      currency: value.currency.trim().toUpperCase(),
    };

    this.loading.set(true);

    const request$ = this.isEdit()
      ? this.itemApi.update(this.itemId()!, payload)
      : this.itemApi.create(payload);

    request$.subscribe({
      next: (item) => {
        this.loading.set(false);
        this.toast.show(
          this.isEdit() ? 'items.form.updated' : 'items.form.created',
          ToastType.Success
        );
        this.saved.emit(item);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  protected onClose(): void {
    this.close.emit();
  }

  private toNullable(value: string | null | undefined): string | null {
    const trimmed = (value ?? '').trim();
    return trimmed.length === 0 ? null : trimmed;
  }
}

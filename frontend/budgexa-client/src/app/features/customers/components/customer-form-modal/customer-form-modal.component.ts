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
import { CustomerApiService } from '../../services/customer-api.service';
import { CustomerDto } from '../../models/customer.model';

export type CustomerFormMode = 'create' | 'edit';

@Component({
  selector: 'app-customer-form-modal',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslateModule,
    SpinnerComponent,
    FormErrorComponent,
  ],
  templateUrl: './customer-form-modal.component.html',
  styleUrl: './customer-form-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerFormModalComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly customerApi = inject(CustomerApiService);
  private readonly toast = inject(ToastService);

  readonly mode = input<CustomerFormMode>('create');
  /** Existing customer id (required when mode is 'edit'). */
  readonly customerId = input<Guid | null>(null);

  readonly saved = output<CustomerDto>();
  readonly close = output<void>();

  protected readonly loading = signal(false);
  protected readonly loadingData = signal(false);

  protected readonly isEdit = computed(() => this.mode() === 'edit');
  protected readonly titleKey = computed(() =>
    this.isEdit() ? 'customers.form.editTitle' : 'customers.form.createTitle'
  );
  protected readonly submitKey = computed(() => {
    if (this.loading()) {
      return this.isEdit() ? 'saving' : 'customers.form.creating';
    }
    return this.isEdit() ? 'save' : 'customers.form.create';
  });

  readonly form = this.fb.group({
    legalName: ['', [Validators.required, Validators.maxLength(200)]],
    tradeName: ['', [Validators.maxLength(200)]],
    taxId: ['', [Validators.required, Validators.maxLength(50)]],
    email: ['', [Validators.email, Validators.maxLength(200)]],
    phone: ['', [Validators.maxLength(50)]],
    addressLine: ['', [Validators.maxLength(300)]],
    postalCode: ['', [Validators.maxLength(20)]],
    city: ['', [Validators.maxLength(150)]],
    province: ['', [Validators.maxLength(150)]],
    country: ['', [Validators.maxLength(100)]],
    notes: ['', [Validators.maxLength(2000)]],
  });

  ngOnInit(): void {
    if (this.isEdit() && this.customerId()) {
      this.loadCustomer();
    }
  }

  @HostListener('document:keydown.escape')
  protected onEscape() {
    if (!this.loading()) this.onClose();
  }

  private loadCustomer(): void {
    const id = this.customerId();
    if (!id) return;

    this.loadingData.set(true);
    this.customerApi.getById(id).subscribe({
      next: (customer) => {
        this.form.patchValue({
          legalName: customer.legalName,
          tradeName: customer.tradeName ?? '',
          taxId: customer.taxId,
          email: customer.email ?? '',
          phone: customer.phone ?? '',
          addressLine: customer.addressLine ?? '',
          postalCode: customer.postalCode ?? '',
          city: customer.city ?? '',
          province: customer.province ?? '',
          country: customer.country ?? '',
          notes: customer.notes ?? '',
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
      legalName: value.legalName.trim(),
      tradeName: this.toNullable(value.tradeName),
      taxId: value.taxId.trim(),
      email: this.toNullable(value.email),
      phone: this.toNullable(value.phone),
      addressLine: this.toNullable(value.addressLine),
      postalCode: this.toNullable(value.postalCode),
      city: this.toNullable(value.city),
      province: this.toNullable(value.province),
      country: this.toNullable(value.country),
      notes: this.toNullable(value.notes),
    };

    this.loading.set(true);

    const request$ = this.isEdit()
      ? this.customerApi.update(this.customerId()!, payload)
      : this.customerApi.create(payload);

    request$.subscribe({
      next: (customer) => {
        this.loading.set(false);
        this.toast.show(
          this.isEdit() ? 'customers.form.updated' : 'customers.form.created',
          ToastType.Success
        );
        this.saved.emit(customer);
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

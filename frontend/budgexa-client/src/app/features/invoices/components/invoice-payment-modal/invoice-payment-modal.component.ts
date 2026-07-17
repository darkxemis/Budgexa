import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { FormErrorComponent } from '../../../../shared/components/form-error/form-error.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { Guid } from '../../../../core/models/guid.model';
import { ToastService } from '../../../../shared/components/toast/toast.service';
import { ToastType } from '../../../../shared/components/toast/toast.type';
import { InvoiceApiService } from '../../services/invoice-api.service';
import {
  InvoiceDto,
  PaymentMethod,
  RegisterInvoicePaymentDto,
} from '../../models/invoice.model';

interface PaymentMethodOption {
  value: PaymentMethod;
  labelKey: string;
}

@Component({
  selector: 'app-invoice-payment-modal',
  standalone: true,
  imports: [ReactiveFormsModule, TranslateModule, SpinnerComponent, FormErrorComponent],
  templateUrl: './invoice-payment-modal.component.html',
  styleUrl: './invoice-payment-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvoicePaymentModalComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly invoiceApi = inject(InvoiceApiService);
  private readonly toast = inject(ToastService);

  readonly invoiceId = input.required<Guid>();
  readonly invoiceNumber = input<string>('');
  readonly currency = input<string>('EUR');
  readonly amountDue = input<number>(0);

  readonly registered = output<InvoiceDto>();
  readonly close = output<void>();

  protected readonly loading = signal(false);

  protected readonly methods: PaymentMethodOption[] = [
    { value: PaymentMethod.BankTransfer, labelKey: 'invoices.payment.methods.bankTransfer' },
    { value: PaymentMethod.Cash, labelKey: 'invoices.payment.methods.cash' },
    { value: PaymentMethod.Card, labelKey: 'invoices.payment.methods.card' },
    { value: PaymentMethod.DirectDebit, labelKey: 'invoices.payment.methods.directDebit' },
    { value: PaymentMethod.Bizum, labelKey: 'invoices.payment.methods.bizum' },
    { value: PaymentMethod.Other, labelKey: 'invoices.payment.methods.other' },
  ];

  readonly form = this.fb.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    method: [PaymentMethod.BankTransfer, [Validators.required]],
    reference: ['', [Validators.maxLength(200)]],
  });

  ngOnInit(): void {
    // Preload with the outstanding balance for convenience.
    const due = Number(this.amountDue()) || 0;
    if (due > 0) {
      this.form.controls.amount.setValue(Number(due.toFixed(2)));
    }
  }

  @HostListener('document:keydown.escape')
  protected onEscape() {
    if (!this.loading()) this.onClose();
  }

  protected onSubmit(): void {
    if (this.loading()) return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const payload: RegisterInvoicePaymentDto = {
      amount: Number(value.amount) || 0,
      method: value.method,
      reference: this.toNullable(value.reference),
    };

    this.loading.set(true);
    this.invoiceApi.registerPayment(this.invoiceId(), payload).subscribe({
      next: (invoice) => {
        this.loading.set(false);
        this.toast.show('invoices.payment.registered', ToastType.Success);
        this.registered.emit(invoice);
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

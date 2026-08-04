import { Component, signal, inject, ChangeDetectionStrategy, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { VoiceInputComponent } from '../../../../shared/components/voice-input/voice-input.component';
import { InvoiceAiApiService } from '../../services/invoice-ai-api.service';
import { PrivateInvoiceAiResponseDto } from '../../../../shared/models/ai.model';

@Component({
  selector: 'app-invoice-ai-generator',
  standalone: true,
  imports: [FormsModule, TranslateModule, SpinnerComponent, VoiceInputComponent],
  templateUrl: './invoice-ai-generator.component.html',
  styleUrl: './invoice-ai-generator.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvoiceAiGeneratorComponent {
  private readonly api = inject(InvoiceAiApiService);

  readonly close = output<void>();
  readonly generated = output<PrivateInvoiceAiResponseDto>();

  protected readonly userRequest = signal('');
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected get isValid(): boolean {
    return this.userRequest().trim().length >= 10;
  }

  protected onSubmit(): void {
    if (!this.isValid || this.loading()) return;

    this.loading.set(true);
    this.error.set(null);

    this.api.generateWithAi({ userRequest: this.userRequest().trim() }).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.generated.emit(response);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 400) {
          this.error.set('invoices.ai.errors.invalidRequest');
        } else if (err.status === 429) {
          this.error.set('invoices.ai.errors.rateLimit');
        } else if (err.status === 401) {
          this.error.set('invoices.ai.errors.unauthorized');
        } else {
          this.error.set('invoices.ai.errors.generic');
        }
      },
    });
  }

  protected onCancel(): void {
    this.close.emit();
  }
}

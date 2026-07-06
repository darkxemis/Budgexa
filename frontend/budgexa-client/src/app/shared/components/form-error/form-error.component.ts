import { ChangeDetectionStrategy, Component, computed, effect, input, signal } from '@angular/core';
import { AbstractControl, ValidationErrors } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-form-error',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './form-error.component.html',
  styleUrls: ['./form-error.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormErrorComponent {
  readonly control = input.required<AbstractControl | null>();

  private readonly errorMessages: Record<string, string> = {
    required: 'validations.required',
    email: 'validations.email',
    minlength: 'validations.minlength',
    maxlength: 'validations.maxlength',
    pattern: 'validations.pattern',
  };

  private readonly touched = signal(false);
  private readonly errors = signal<ValidationErrors | null>(null);

  readonly showError = computed(() => this.touched() && this.errors() != null);

  readonly errorMessage = computed(() => {
    const keys = Object.keys(this.errors() || {});
    const key = keys.length > 0 ? keys[0] : '';
    return this.errorMessages[key] || 'validations.invalid';
  });

  constructor() {
    effect((onCleanup) => {
      const ctrl = this.control();
      if (!ctrl) return;

      this.touched.set(ctrl.touched);
      this.errors.set(ctrl.errors);

      const sub = ctrl.events.subscribe(() => {
        this.touched.set(ctrl.touched);
        this.errors.set(ctrl.errors);
      });

      onCleanup(() => sub.unsubscribe());
    });
  }
}

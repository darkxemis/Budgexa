import { Component, input, computed, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl } from '@angular/forms';
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
  control = input.required<AbstractControl | null>();

  private readonly errorMessages: Record<string, string> = {
    required: 'validations.required',
    email: 'validations.email',
    minlength: 'validations.minlength',
    maxlength: 'validations.maxlength',
    pattern: 'validations.pattern'
  };

  errorKey = computed(() => {
    const keys = Object.keys(this.control()?.errors || {});
    return keys.length > 0 ? keys[0] : '';
  });

  errorMessage = computed(() => {
    return this.errorMessages[this.errorKey()] || 'validations.invalid';
  });
}

import { Component, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-form-error',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './form-error.component.html',
  styleUrls: ['./form-error.component.scss']
})
export class FormErrorComponent {
  @Input() control!: AbstractControl | null | any;

  private readonly errorMessages: Record<string, string> = {
    required: 'validations.required',
    email: 'validations.email',
    minlength: 'validations.minlength',
    maxlength: 'validations.maxlength',
    pattern: 'validations.pattern'
  };

  get errorKey(): string {
    const keys = Object.keys(this.control?.errors || {});
    return keys.length > 0 ? keys[0] : ''; 
  }

  get errorMessage(): string {
    return this.errorMessages[this.errorKey] || 'validations.invalid';
  }
}

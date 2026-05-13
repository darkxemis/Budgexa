import { Component, inject, signal, output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { UserService } from '../../../core/services/user.service';
import { UserStore } from '../../../core/state/user.store';
import { SpinnerComponent } from '../spinner/spinner.component';
import { FormErrorComponent } from '../form-error/form-error.component';
import { ToastService } from '../toast/toast.service';
import { ToastType } from '../toast/toast.type';
import { LanguageDataService } from '../../../core/services/language-data.service';

@Component({
  selector: 'app-user-settings-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    SpinnerComponent,
    FormErrorComponent,
  ],
  templateUrl: './user-settings-modal.component.html',
  styleUrl: './user-settings-modal.component.scss',
})
export class UserSettingsModalComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly userService = inject(UserService);
  private readonly userStore = inject(UserStore);
  private readonly toastService = inject(ToastService);
  private readonly languageDataService = inject(LanguageDataService);

  readonly close = output<void>();
  readonly loading = signal(false);
  readonly loadingLanguages = signal(true);

  readonly languages = this.languageDataService.languages;

  readonly form = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    password: ['', [Validators.minLength(6)]],
    languageId: ['', [Validators.required]],
  });

  ngOnInit() {
    this.form.controls.languageId.disable();

    this.languageDataService.loadLanguages().subscribe({
      next: (languages) => {
        this.loadingLanguages.set(false);
        this.form.controls.languageId.enable();
        
        const user = this.userStore.user();
        if (user) {
          const currentLang = languages.find((l) => l.code === user.language);
          this.form.patchValue({
            firstName: user.firstName,
            lastName: user.lastName,
            languageId: currentLang?.id || user.languageId || languages[0]?.id || '',
          });
        }
      },
      error: () => {
        this.loadingLanguages.set(false);
        this.form.controls.languageId.enable();
      },
    });
  }

  onSubmit() {
    if (this.loading() || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    const formValue = this.form.getRawValue();
    const dto = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      languageId: formValue.languageId,
      ...(formValue.password && { password: formValue.password }),
    };

    this.userService.updateUser(dto).subscribe({
      next: () => {
        this.loading.set(false);
        this.toastService.show('settingsUpdated', ToastType.Success);
        this.close.emit();
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  onClose() {
    this.close.emit();
  }
}

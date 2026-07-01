import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
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
import { forkJoin } from 'rxjs';
import { FormErrorComponent } from '../../../../shared/components/form-error/form-error.component';
import { IconComponent } from '../../../../shared/components/icon/icon.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { LanguageApiService } from '../../../../core/api/language-api.service';
import { RoleApiService } from '../../../../core/api/role-api.service';
import { UserApiService } from '../../../../core/api/user-api.service';
import { Guid } from '../../../../core/models/guid.model';
import { Language, RoleInfo, UserDetailDto } from '../../../../core/models/user.model';
import { ToastService } from '../../../../shared/components/toast/toast.service';
import { ToastType } from '../../../../shared/components/toast/toast.type';

export type UserFormMode = 'create' | 'edit';

const STRONG_PASSWORD_PATTERN =
  /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

@Component({
  selector: 'app-user-form-modal',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslateModule,
    SpinnerComponent,
    FormErrorComponent,
    IconComponent,
  ],
  templateUrl: './user-form-modal.component.html',
  styleUrl: './user-form-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserFormModalComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly userApi = inject(UserApiService);
  private readonly roleApi = inject(RoleApiService);
  private readonly languageApi = inject(LanguageApiService);
  private readonly toast = inject(ToastService);

  readonly mode = input<UserFormMode>('create');
  /** Existing user id (required when mode is 'edit'). */
  readonly userId = input<Guid | null>(null);

  readonly saved = output<UserDetailDto>();
  readonly close = output<void>();

  protected readonly loading = signal(false);
  protected readonly loadingData = signal(true);
  protected readonly languages = signal<Language[]>([]);
  protected readonly roles = signal<RoleInfo[]>([]);
  protected readonly selectedRoleIds = signal<Set<Guid>>(new Set());

  protected readonly isEdit = computed(() => this.mode() === 'edit');
  protected readonly titleKey = computed(() =>
    this.isEdit() ? 'users.form.editTitle' : 'users.form.createTitle'
  );
  protected readonly submitKey = computed(() => {
    if (this.loading()) {
      return this.isEdit() ? 'saving' : 'users.form.creating';
    }
    return this.isEdit() ? 'save' : 'users.form.create';
  });
  protected readonly hasRoleError = signal(false);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', []],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    languageId: ['' as Guid | '', [Validators.required]],
  });

  constructor() {
    // Recompute password validators when mode changes (required for create only).
    effect(() => {
      const isEdit = this.isEdit();
      const passwordControl = this.form.controls.password;
      const baseValidators = [
        Validators.minLength(8),
        Validators.pattern(STRONG_PASSWORD_PATTERN),
      ];
      passwordControl.setValidators(
        isEdit ? baseValidators : [Validators.required, ...baseValidators]
      );
      passwordControl.updateValueAndValidity({ emitEvent: false });
    });
  }

  ngOnInit(): void {
    this.loadInitialData();
  }

  @HostListener('document:keydown.escape')
  protected onEscape() {
    if (!this.loading()) this.onClose();
  }

  private loadInitialData(): void {
    this.loadingData.set(true);

    forkJoin({
      languages: this.languageApi.getLanguages(),
      roles: this.roleApi.getAllRoles(),
    }).subscribe({
      next: ({ languages, roles }) => {
        this.languages.set(languages);
        this.roles.set(roles);

        if (this.isEdit() && this.userId()) {
          this.loadUser();
        } else {
          // Default to the first available language for new users
          if (languages.length > 0) {
            this.form.patchValue({ languageId: languages[0].id });
          }
          this.loadingData.set(false);
        }
      },
      error: () => {
        this.loadingData.set(false);
        this.toast.show('server.internalError', ToastType.Error);
      },
    });
  }

  private loadUser(): void {
    const id = this.userId();
    if (!id) {
      this.loadingData.set(false);
      return;
    }

    this.userApi.getById(id).subscribe({
      next: (user) => {
        this.form.patchValue({
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName,
          languageId: user.language.id,
        });
        this.selectedRoleIds.set(new Set(user.roles.map((r) => r.id)));
        this.loadingData.set(false);
      },
      error: () => {
        this.loadingData.set(false);
        this.onClose();
      },
    });
  }

  protected toggleRole(roleId: Guid): void {
    const next = new Set(this.selectedRoleIds());
    if (next.has(roleId)) {
      next.delete(roleId);
    } else {
      next.add(roleId);
    }
    this.selectedRoleIds.set(next);
    this.hasRoleError.set(next.size === 0);
  }

  protected isRoleSelected(roleId: Guid): boolean {
    return this.selectedRoleIds().has(roleId);
  }

  protected onSubmit(): void {
    if (this.loading()) return;

    const rolesSelected = this.selectedRoleIds().size > 0;
    this.hasRoleError.set(!rolesSelected);

    if (this.form.invalid || !rolesSelected) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const roleIds = Array.from(this.selectedRoleIds());
    const languageId = value.languageId as Guid;

    this.loading.set(true);

    const request$ = this.isEdit()
      ? this.userApi.update(this.userId()!, {
          email: value.email,
          password: value.password ?? '',
          firstName: value.firstName,
          lastName: value.lastName,
          languageId,
          roleIds,
        })
      : this.userApi.create({
          email: value.email,
          password: value.password,
          firstName: value.firstName,
          lastName: value.lastName,
          languageId,
          roleIds,
        });

    request$.subscribe({
      next: (user) => {
        this.loading.set(false);
        this.toast.show(
          this.isEdit() ? 'users.form.updated' : 'users.form.created',
          ToastType.Success
        );
        this.saved.emit(user);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  protected onClose(): void {
    this.close.emit();
  }
}

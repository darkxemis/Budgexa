import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  input,
  output,
} from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { IconComponent, IconName } from '../icon/icon.component';
import { SpinnerComponent } from '../spinner/spinner.component';

export type ConfirmVariant = 'danger' | 'primary';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [TranslateModule, IconComponent, SpinnerComponent],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialogComponent {
  /** Translation key for the dialog title. */
  readonly title = input.required<string>();
  /** Translation key for the dialog message. */
  readonly message = input.required<string>();
  /** Optional translation parameters interpolated in the message. */
  readonly messageParams = input<Record<string, unknown> | undefined>(undefined);
  /** Translation key for the confirm button. Defaults to a generic confirm. */
  readonly confirmLabel = input<string>('common.confirm');
  /** Translation key for the cancel button. */
  readonly cancelLabel = input<string>('cancel');
  /** Visual style: 'danger' shows a red CTA + warning icon (typical for delete). */
  readonly variant = input<ConfirmVariant>('danger');
  /** Disables the confirm button and shows a spinner while in-flight. */
  readonly loading = input<boolean>(false);
  /** Optional icon name override. */
  readonly icon = input<IconName | undefined>(undefined);

  readonly confirm = output<void>();
  readonly cancel = output<void>();

  protected resolvedIcon(): IconName {
    return this.icon() ?? (this.variant() === 'danger' ? 'warning' : 'check');
  }

  protected onConfirm() {
    if (this.loading()) return;
    this.confirm.emit();
  }

  protected onCancel() {
    if (this.loading()) return;
    this.cancel.emit();
  }

  @HostListener('document:keydown.escape')
  protected onEscape() {
    this.onCancel();
  }
}

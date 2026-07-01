import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { StatusApiService } from '../../../core/api/status-api.service';
import { SelectorOption } from '../../../core/models/selector.model';
import { Guid } from '../../../core/models/guid.model';
import { SpinnerComponent } from '../spinner/spinner.component';

/**
 * Small dropdown that lets the user pick a new status for an entity.
 * The list of options is fetched from the status selector endpoint using
 * the provided `group` (e.g. "Budget", "Invoice").
 */
@Component({
  selector: 'app-status-change-menu',
  standalone: true,
  imports: [TranslateModule, SpinnerComponent],
  templateUrl: './status-change-menu.component.html',
  styleUrl: './status-change-menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:click)': 'onDocumentClick($event)',
  },
})
export class StatusChangeMenuComponent {
  private readonly statusApi = inject(StatusApiService);

  /** Status selector group (e.g. "Budget" or "Invoice"). */
  readonly group = input.required<string>();
  /** Current status id, hidden from the option list. */
  readonly currentStatusId = input<Guid | null>(null);
  /** Optional disabled flag propagated from the parent. */
  readonly disabled = input<boolean>(false);
  /** Translation key for the button label (default: "status.change"). */
  readonly buttonLabel = input<string>('status.change');

  /** Emitted after the user picks a new status id. */
  readonly statusSelected = output<Guid>();

  protected readonly open = signal(false);
  protected readonly loading = signal(false);
  protected readonly options = signal<SelectorOption[]>([]);

  constructor() {
    // Reload options whenever the group changes.
    effect(() => {
      const group = this.group();
      if (group) {
        void this.loadOptions(group);
      }
    });
  }

  protected toggle(event: MouseEvent): void {
    event.stopPropagation();
    if (this.disabled()) return;
    this.open.update(value => !value);
  }

  protected select(option: SelectorOption): void {
    this.open.set(false);
    this.statusSelected.emit(option.id);
  }

  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.status-change-menu')) {
      this.open.set(false);
    }
  }

  private async loadOptions(group: string): Promise<void> {
    this.loading.set(true);
    try {
      const result = await firstValueFrom(this.statusApi.getStatusForSelector(group));
      this.options.set(result ?? []);
    } catch {
      this.options.set([]);
    } finally {
      this.loading.set(false);
    }
  }
}
